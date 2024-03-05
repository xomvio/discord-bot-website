using Newtonsoft.Json.Linq;

namespace Bitguard.DiscordRazor;
public class PageActions
{
    //compares id's from discord api with selected servers id. if it matches index is accepted.
    //blocks unprivilieged activities. so, dont remove it
    public static int GetSelectedServerIndex(ServerData[] servers, string ssid)
    {
        int s = -1;
        for (int i = 0; i < servers.Count(); i++)
        {
            if (servers[i].Id == ssid)
            {
                s = i;
                break;
            }
        }
        return s;
    }

    //in dashboard, if a server doesnt have a picture, replaces image field with servers names first letters.
    public static string FirstLetters(string s) 
    {
        string[] a = s.Split(' ');
        int c = a.Length;
        if (a.Length > 5)
            c = 5;
        s = "";
        for (int i = 0; i < c; i++)
        {
            s += a[i][0];
        }
        return s;
    }

    public static async Task<bool> EmailValid(string mail)  //email validator for communication modal.
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://www.disify.com/api/email/" + mail);

        HttpClient client = new HttpClient();
        var res = client.Send(req);

        JObject obj = JObject.Parse(await res.Content.ReadAsStringAsync());
        bool disposable = bool.Parse(obj["disposable"].ToString());

        return !disposable;
    }

    public static string StatusErrorMsg(string status)
    {
        switch (status)
        {
            case "400":
                status = "400: Bad Request";
                    break;
            case "401":
                status = "401: Unauthorized";
                break;
            case "404":
                status = "404: Not Found";
                break;
            case "408":
                status = "408: Request Timeout";
                break;
            case "409":
                status = "409: Conflict; Multiple Errors occured.";
                break;
            case "429":
                status = "429: Too Many Request";
                break;
            default:
                break;
        }
        return status;
    }
}
