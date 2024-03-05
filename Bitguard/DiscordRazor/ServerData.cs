using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Bitguard.DiscordRazor
{
    public class ServerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        public ServerData(string id, string name, string icon)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }
        public ServerData()
        {

        }

        public static async Task<ServerData[]> FetchAsync(string nekot)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nekot);

            //apiden data alınır
            HttpClient httpClient = new HttpClient();
            var res = httpClient.Send(req, HttpCompletionOption.ResponseContentRead);
            res.EnsureSuccessStatusCode();

            //var serverjson = JArray.Parse(await res.Content.ReadAsStringAsync()).Root;
            var serverjson = JArray.Parse(await res.Content.ReadAsStringAsync()).Root;
            List<ServerData> servers = new List<ServerData>();
            for (int i = 0; i < serverjson.Count(); i++)
                //if (serverjson[i].GetValue<string>("permissions").ToString() == "2147483647")
                if (serverjson[i]["permissions"].ToString() == "2147483647")
                    servers.Add(new ServerData()
                    {
                        Id = serverjson[i]["id"].ToString(),
                        Name = serverjson[i]["name"].ToString(),
                        Icon = serverjson[i]["icon"].ToString()
                    });
            
            return servers.ToArray();
        }
    }
}
