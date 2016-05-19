using System;using System.Collections.ObjectModel;using System.Linq;using System.Windows.Input;using CallOfService.Mobile.Services.Abstracts;using CallOfService.Mobile.UI;using PropertyChanged;using Xamarin.Forms;using PubSub;using CallOfService.Mobile.Core.SystemServices;using Acr.UserDialogs;using CallOfService.Mobile.Messages;namespace CallOfService.Mobile.Features.Jobs{
    [ImplementPropertyChanged]
    public class JobsViewModel : IViewAwareViewModel
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserDialogs _userDialogs;
        private ImageSource _imageSource;
        private IMediaPicker _mediaPicker;
        private IImageCompressor _imageCompressor;
        public JobsViewModel(IAppointmentService appointmentService, IUserDialogs userDialogs)
        {
            _userDialogs = userDialogs;
            _appointmentService = appointmentService;

            Appointments = new ObservableCollection<AppointmentViewModel>();
            this.Subscribe<JobSelected>(async m =>
            {
                await NavigationService.NavigateToJobDetails();
                this.Publish(new ViewJobDetails(m.Appointment.JobId));
            });
            this.Subscribe<NewDateSelected>(m =>
            {
                Date = m.DateTime;
                OnAppearing();
            });
            Date = DateTime.Today;
        }

        public ObservableCollection<AppointmentViewModel> Appointments { get; set; }

        public DateTime Date { get; set; }

        public ICommand GoToNextDay
        {
            get
            {
                return new Command(() =>
                {
                    Date = Date.AddDays(1);
                    OnAppearing();
                });
            }
        }

        public ICommand GoToPrevDay
        {
            get
            {
                return new Command(() =>
                {
                    Date = Date.AddDays(-1);
                    OnAppearing();
                });
            }
        }

        public bool IsRefreshing { get; set; }

        public ICommand ShowCalendarView
        {
            get
            {
                return new Command(() =>
                {
                    this.Publish(new ShowCalendarView());
                });
            }
        }

        public ICommand RefreshListOfJobsCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;
                    await _appointmentService.RetrieveAndSaveAppointments();
                    OnAppearing();
                    IsRefreshing = false;
                });
            }
        }

        public void Dispose()
        {
            Appointments.Clear();
        }

        public async void OnAppearing()
        {
            IsRefreshing = true;
            var appointments = await _appointmentService.AppointmentsByDay(Date);
            Appointments.Clear();
            foreach (var appointment in appointments.Where(a => !a.IsCancelled).OrderBy(a => a.Start))
            {
                Appointments.Add(new AppointmentViewModel
                {
                    Title = appointment.Title,                    Location = appointment.Location,                    IsFinished = appointment.IsFinished,                    IsInProgress = appointment.IsInProgress,                    IsCancelled = appointment.IsCancelled,
                    StartTimeEndTimeFormated = $"{appointment.Start.ToUniversalTime().ToString("hh:mm tt")} - {appointment.End.ToUniversalTime().ToString("hh:mm tt")}",
                    JobId = appointment.JobId
                });
            }
            IsRefreshing = false;
        }

        public void OnDisappearing()
        {
        }
    }

    [ImplementPropertyChanged]
    public class AppointmentViewModel
    {
        public string Title { get; set; }

        public string StartTimeEndTimeFormated { get; set; }

        public string Location { get; set; }

        public int JobId { get; set; }        public bool IsFinished { get; set; }        public bool IsInProgress { get; set; }        public bool IsCancelled { get; set; }        public bool IsScheduled { get { return !IsFinished && !IsInProgress && !IsCancelled; } }        public ICommand ViewDetails { get { return new Command(() => this.Publish(new JobSelected(this))); } }
    }}