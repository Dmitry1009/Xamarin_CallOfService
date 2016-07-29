using System;
using CallOfService.Mobile.Core.DI;
using CallOfService.Mobile.Core.SystemServices;
using CallOfService.Mobile.Features.Calendar;
using CallOfService.Mobile.Features.Jobs;
using CallOfService.Mobile.Messages;
using CallOfService.Mobile.Services.Abstracts;
using PubSub;
using Xamarin.Forms;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CallOfService.Mobile.Core;
using Plugin.Connectivity;
using Plugin.Geolocator.Abstractions;
using Page = Xamarin.Forms.Page;

namespace CallOfService.Mobile.Features.Dashboard
{
    public partial class DashboardPage : TabbedPage
    {
        private bool _shouldInit;
        private Page _jobsPage;
        private Page _calendarPage;

        public DashboardPage()
        {
            InitializeComponent();

            Title = "Call of Service";

            _shouldInit = true;
            this.Subscribe<NewDateSelected>(m =>
            {
                this.CurrentPage = _jobsPage;
            });

            this.Subscribe<ShowCalendarView>(m =>
            {
                this.CurrentPage = _calendarPage;
            });

            this.Subscribe<UserUnauthorized>(async m => await RefreshToken());
        }

        private async Task RefreshToken()
        {
            var userService = DependencyResolver.Resolve<IUserService>();
            var loginService = DependencyResolver.Resolve<ILoginService>();
            var cred = userService.GetUserCredentials();
            var loginResult = await loginService.Login(cred.Email, cred.Password);
            if (!loginResult.IsSuccessful)
            {
                await NavigationService.ShowLoginPage();
            }
        }

        protected override async void OnAppearing()
        {
            if (!_shouldInit)
                return;

            var dialog = DependencyResolver.Resolve<IUserDialogs>();
            dialog.ShowLoading("Loading...");

            var analyticsService = DependencyResolver.Resolve<IAnalyticsService>();
            await analyticsService.Identify();
            analyticsService.Track("Loading App");

            CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
            {
                var userDialogs = DependencyResolver.Resolve<IUserDialogs>();

                if (!args.IsConnected)
                    userDialogs.Toast("Connection went offline.");
                else
                    userDialogs.Toast("Connection is back.");
            };

            var appointmentService = DependencyResolver.Resolve<IAppointmentService>();
            await appointmentService.RetrieveAndSaveAppointments();

            StartLocationUpdateRegistration(dialog);

            base.OnAppearing();

            Children.Clear();
            _jobsPage = NavigationService.CreateAndBind<JobsPage>(DependencyResolver.Resolve<JobsViewModel>());
            _jobsPage.Title = "JOBS";
            _jobsPage.Icon = "Jobs.png";
            Children.Add(_jobsPage);
            _calendarPage = NavigationService.CreateAndBind<CalendarPage>(DependencyResolver.Resolve<CalendarViewModel>());
            _calendarPage.Title = "CALENDAR";
            _calendarPage.Icon = "Calendar.png";
            Children.Add(_calendarPage);

            dialog.HideLoading();

            _shouldInit = false;
        }

        private void StartLocationUpdateRegistration(IUserDialogs dialog)
        {
            var locationService = DependencyResolver.Resolve<ILocationService>();

            Task.Run(async () =>
            {
                try
                {
                    var locationSentSuccessfully = await locationService.SendCurrentLocationUpdate();
                    dialog.Toast(!locationSentSuccessfully ? "Errro while sending current location" : "Current location sent successfully");

                    var locationUpdateRegisteredSuccessfully = await locationService.StartListening();
                    dialog.Toast(!locationUpdateRegisteredSuccessfully ? "Errro while registering location update" : "Location update registered successully");
                }
                catch (Exception e)
                {
                    dialog.Toast("Errro while registering location update");
                }
            });
        }
    }
}