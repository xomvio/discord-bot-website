using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
namespace Bitguard.DiscordRazor
{
    public class UserData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string GlobalName { get; set; }

        public UserData() { }

        public static async Task<UserData> FetchAsync(string nekot)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nekot);

            //apiden data alınır
            HttpClient httpClient = new HttpClient();
            var res = httpClient.Send(req, HttpCompletionOption.ResponseContentRead);
            res.EnsureSuccessStatusCode();

            JObject userjson = JObject.Parse(await res.Content.ReadAsStringAsync());
            UserData userData = new UserData()
            {
                Id = userjson["id"].ToString(),
                Name = userjson["username"].ToString(),
                Avatar = userjson["avatar"].ToString(),
                GlobalName = userjson["global_name"].ToString()
            };

            return userData;
        }
    }
}
