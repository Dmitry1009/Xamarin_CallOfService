using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CallOfService.Mobile.Core.Networking;
using CallOfService.Mobile.Core.SystemServices;
using CallOfService.Mobile.Domain;
using CallOfService.Mobile.Proxies.Abstratcs;
using CallOfService.Mobile.Services.Abstracts;
using Newtonsoft.Json;

namespace CallOfService.Mobile.Proxies
{
    public class AppointmentProxy : BaseProxy, IAppointmentProxy
    {
        public AppointmentProxy(ILogger logger, IUserService userService) : base(logger, userService)
        {
        }

        public async Task<List<Appointment>> GetAppointments(int userId, DateTime? date = null, string view = "year")
        {
            if (date == null)
                date = DateTime.Now;
            var url = $"{UrlConstants.AppointmentsUrl}?View=year&UserId={userId}&date={date.Value.ToString("yyyy-MM-dd")}";
            return await GetAsync<List<Appointment>>(url);
        }

        public async Task<Job> GetJobById(int jobId)
        {
            var url = $"{UrlConstants.JobByIdUrl}/{jobId}";
            return await GetAsync<Job>(url);
        }

        private string _googleApiKey = "AIzaSyDwSOIXf8vGJ6fuU0_TUHuY2BAIXi9UZAE";

        public async Task<GpsPoint> GetJobLocation(AddressInfo location)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={location.FormattedAddress}&key={_googleApiKey}";
            var responseString = await GetStringAsync(url, 2, true);

            if (string.IsNullOrEmpty(responseString))
                return new GpsPoint { IsValid = false };

            try
            {
                var result = JsonConvert.DeserializeAnonymousType(responseString, new { results = new[] { new { geometry = new { location = new { lat = "", lng = "" } } } } });

                if (result.results != null && result.results.Any() && result.results[0].geometry != null && result.results[0].geometry.location != null)
                {

                    return new GpsPoint
                    {
                        Lat = result.results[0].geometry.location.lat,
                        Lng = result.results[0].geometry.location.lng,
                        IsValid = true
                    };
                }
                else
                    return new GpsPoint { IsValid = false };

            }
            catch (Exception)
            {
                return new GpsPoint {IsValid = false};
            }
        }

        public async Task<bool> StartJob(int jobId, double? latitude, double? longitude)
        {
            var url = $"{UrlConstants.StartJob}";
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { Id = jobId, latitude, longitude }), Encoding.UTF8, "application/json");

            return await PostAsync(url, stringContent);
        }

        public async Task<bool> FinishJob(int jobId, double? latitude, double? longitude)
        {
            var url = $"{UrlConstants.FinishJob}";
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { Id = jobId, latitude, longitude }), Encoding.UTF8, "application/json");

            return await PostAsync(url, stringContent);
        }

		public async Task<bool> AddNote(int jobNumber, string newNoteText, List<byte[]> attachments, DateTime now, double? latitude, double? longitude)
		{
		    var url = UrlConstants.NewNoteUrl;

		    try
		    {
		        var formContents = new Dictionary<StringContent, string>
		        {
		            {new StringContent(jobNumber.ToString(), Encoding.UTF8), "Id"},
		            {new StringContent(GetTime(now).ToString(), Encoding.UTF8), "Timestamp"},
		        };

		        if (!string.IsNullOrEmpty(newNoteText))
		        {
		            formContents.Add(new StringContent(newNoteText, Encoding.UTF8), "Note");
		        }

		        if (latitude.HasValue && longitude.HasValue)
		        {
		            formContents.Add(new StringContent(latitude.ToString(), Encoding.UTF8), "Latitude");
		            formContents.Add(new StringContent(longitude.ToString(), Encoding.UTF8), "Longitude");
		        }

		        var steamContents = new List<Tuple<StreamContent, string, string>>();
		        for (int index = 0; index < attachments.Count; index++)
		        {
		            var data = attachments[index];
		            var imageContent = new StreamContent(new MemoryStream(data));
		            imageContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg");
		            steamContents.Add(new Tuple<StreamContent, string, string>(imageContent, $"S{index + 1}", $"{jobNumber}-{Guid.NewGuid()}.jpg"));
		        }

		        return await PostFormDataAsync(url, formContents, steamContents, 5);
		    }
		    catch (Exception e)
		    {
		        Logger.WriteError("Exceptoin while sending note", exception:e);
		        return await Task.FromResult(false);
		    }
		}

		private long GetTime(DateTime datetime)
		{
			long retval=0;
			var st = new DateTime(1970,1,1);
			var t = datetime.ToUniversalTime()-st;
			retval = (long)(t.TotalMilliseconds+0.5);
			return retval;
		}
    }
}