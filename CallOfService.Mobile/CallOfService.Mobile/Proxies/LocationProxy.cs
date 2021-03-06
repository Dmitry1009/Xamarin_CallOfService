using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CallOfService.Mobile.Core.Networking;
using CallOfService.Mobile.Core.SystemServices;
using CallOfService.Mobile.Proxies.Abstratcs;
using CallOfService.Mobile.Services.Abstracts;
using Newtonsoft.Json;

namespace CallOfService.Mobile.Proxies
{
    public class LocationProxy : BaseProxy, ILocationProxy
    {
        public LocationProxy(ILogger logger, IUserService userService) : base(logger, userService)
        {
        }

        public async Task<bool> SendLocation(double latitude, double longitude)
        {
            var url = $"{UrlConstants.SendLocation}";
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { latitude, longitude }), Encoding.UTF8, "application/json");

            return await PostAsync(url, stringContent);
        }

        public async Task<AvailabilitiesInfo> GetAvailability()
        {
            var url = $"{UrlConstants.Availability}";
            return await GetAsync<AvailabilitiesInfo>(url);
        }
    }

    public class AvailabilitiesInfo
    {
        public AvailabilityInfo[] Availabilities { get; set; }
    }

    public class AvailabilityInfo
    {
        public string DayOfWeek { get; set; }
        public TimeSpan? From { get; set; }
        public TimeSpan? To { get; set; }
        public bool IsAvailable { get; set; }
    }
}