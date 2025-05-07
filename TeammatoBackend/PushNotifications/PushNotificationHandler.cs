using System.Net.Http;
using TeammatoBackend.Abstractions;
using System.Text;
using System.Text.Json.Serialization;
namespace TeammatoBackend.PushNotifications
{
    
    public class PushNotificationHandler
    {
        private static string appId = "3f107787-d140-4cee-a820-f7904d3911c6";
        private static HttpClient _httpClient = new HttpClient();
        public static async Task SendNotificationByGameSession(GameSession session, string title, string text)
        {
            

            foreach(var user in session.Participants)
            {
                await SendNotification(title, text, "UserId", user.Id);
            }
        }
        public static async Task SendNotificationByChat(Chat chat, string title, string text)
        {
            

            foreach(var user in chat.Participants)
            {
                await SendNotification(title, text, "UserId", user.Id);
            }
        }
        public static async Task SendNotification(string title, string text, string tagName, string tagValue)
        {
            var jsonData = $@"
            {{
                ""app_id"": ""{appId}"",
                ""contents"": {{
                    ""en"": ""{text}""
                }},
                ""headings"":{{
                    ""en"":""{title}""
                }},
                ""filters"": [
                    {{
                        ""field"": ""tag"",
                        ""key"": ""{tagName}"",
                        ""relation"": ""="",
                        ""value"": ""{tagValue}""
                    }}
                ]
            }}";
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = content;
            var response = await _httpClient.PostAsync("https://api.onesignal.com/notifications?c=push", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Notification sent successfully!");
            }
            else
            {
                Console.WriteLine($"Failed to send notification: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        
        public static void Init(string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Key {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
           
        }

    }
    
        
}