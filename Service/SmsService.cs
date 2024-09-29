using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace City_Taxi.Service
{
    public class SmsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smsApiUrl = "https://www.textit.biz/sendmsg";
        private readonly string _smsUserId;
        private readonly string _smsPassword;

        public SmsService(IConfiguration configuration)
        {
            _smsUserId = configuration["SMS:SMSUserId"];
            _smsPassword = configuration["SMS:SMSPassword"];
        }
        public async Task<string> SendSmsAsync(string to, string message)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"{_smsApiUrl}?id={_smsUserId}&pw={_smsPassword}&to={to}&text={Uri.EscapeDataString(message)}";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        return result; 
                    }
                    else
                    {
                        return $"Error: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
    }
}
