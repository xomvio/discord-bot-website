using Bitguard.DiscordRazor;
using System.Security.Claims;

namespace Bitguard.Models
{
	public class DashboardModel
	{
        public string selectedServerId { get; set; }
        public int selectedServerIndex { get; set; }
        public string[] links { get; set; }
		public UserData userData { get; set; }
		public ServerData[] servers { get; set; }
		public ServerConfig serverConfig { get; set; }

		public DashboardModel(IConfiguration config, string nekot, string ssid)
		{
			DbActions db = new(config);
			selectedServerId = ssid;

			var usertask = UserData.FetchAsync(nekot);
			userData = usertask.Result;
			
			var servertask = ServerData.FetchAsync(nekot);
			servers = servertask.Result;

			selectedServerIndex = PageActions.GetSelectedServerIndex(servers, selectedServerId);

			if (db.isServerExistsOnDb(selectedServerId))
			{
				serverConfig = db.GetServerConfig(selectedServerId);
			}
			else
			{
				serverConfig = new();	//to prevent potential errors future.
			}

			links = db.GetServerLinks(servers);
		}
    }
}
