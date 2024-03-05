using Bitguard.Controllers;
using Microsoft.AspNetCore.Diagnostics;
using MySql.Data.MySqlClient;
using System.Net;
namespace Bitguard.DiscordRazor
{
    public class DbActions
    {
        public MySqlConnection conn;
        public DbActions(IConfiguration config)
        {
            conn = new MySqlConnection(config["ConnectionStrings:DefaultConnection"]);
        }
        public bool isServerExistsOnDb(string serverId)
        {
            if (serverId == "")
                return false;
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM servers WHERE SERVER_ID =@id"))
            {
                cmd.Parameters.AddWithValue("id", serverId);
                cmd.Connection = conn;
                conn.Open();
                if (cmd.ExecuteScalar() != null)
                {
                    conn.Close();
                    return true;
                }
                conn.Close();
                return false;
            }
        }

        public ServerConfig GetServerConfig(string serverId)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM servers WHERE SERVER_ID=@sid"))
            {
                cmd.Parameters.AddWithValue("sid", serverId);
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();
                bool susCheck = true, blockSpam = true;
                string badWords = "", badWordsWarn = "", ntfChannel = "";
                while (reader.Read())
                {
                    susCheck = reader.GetBoolean("SUS_CHECK");
                    blockSpam = reader.GetBoolean("BLOCK_SPAM");
                    badWords = reader.GetString("BAD_WORDS");
                    badWordsWarn = reader.GetString("BAD_WORD_WARN");
                    ntfChannel = reader.GetString("NOTIFICATION_CHANNEL");
                }
                conn.Close();
                return new ServerConfig(serverId, susCheck, blockSpam, badWords, badWordsWarn, ntfChannel, GetChannels(serverId));
            }
        }

        public ServerChannel[] GetChannels(string serverId)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT CHANNEL_ID, CHANNEL_NAME FROM channels WHERE GUILD_ID=" + serverId))
            {
                List<ServerChannel> channels = new();
                cmd.Connection = conn;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    channels.Add(new ServerChannel() { ChannelId = reader.GetString(0), ChannelName = reader.GetString(1) });
                }
                conn.Close();
                return channels.ToArray();
            }
        }

        public string[] GetServerLinks(ServerData[] servers)
        {
            string[] links = new string[servers.Count()];

            for (int i = 0; i < servers.Count(); i++)
            {
                links[i] = "onclick=addBot()";  //sunucu dbde ekli değilse ön tanım

                string query = "SELECT EXISTS(SELECT * from servers WHERE SERVER_ID=@sid)";
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Parameters.AddWithValue("sid", servers[i].Id);
                    cmd.Connection = conn;
                    conn.Open();
                    if (cmd.ExecuteScalar().ToString() == "1")      //sunucu dbde ekli mi?
                    {
                        links[i] = "href=?s=" + servers[i].Id + "";
                    }
                    conn.Close();
                }
            }
            return links;
        }

        public void UpdateServerConfig(ServerConfig config)
        {
            using (MySqlCommand cmd = new MySqlCommand("UPDATE servers SET SUS_CHECK = @suscek, BLOCK_SPAM = @spamblock, BAD_WORDS = @badwords, BAD_WORD_WARN = @badwordwarn, NOTIFICATION_CHANNEL = @ntfchannel WHERE SERVER_ID = @serverid"))
            {
                cmd.Parameters.AddWithValue("suscek", config.susCheck);
                cmd.Parameters.AddWithValue("spamblock", config.blockSpam);
                cmd.Parameters.AddWithValue("badwords", config.badWords);
                cmd.Parameters.AddWithValue("badwordwarn", config.badWordsWarn);
                cmd.Parameters.AddWithValue("ntfchannel", config.ntfChannel);
                cmd.Parameters.AddWithValue("serverid", config.serverId);

                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void AddFeedback(IPAddress? aypi, string mail, string msg)
        {
            
            string ip = "";
            if (aypi != null)
                ip = aypi.ToString();

            var valid = PageActions.EmailValid(mail);
            valid.Wait();
            if (!valid.Result)
            {
                mail = "";
            }

            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO feedbacks (EMAIL, MESSAGE, IP_ADDRESS) VALUES (@mail, @msg, @ip)"))
            {
                cmd.Parameters.AddWithValue("mail", mail);
                cmd.Parameters.AddWithValue("msg", msg);
                cmd.Parameters.AddWithValue("ip", ip);

                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public int[] GetBotActionCounts()
        {
            int[] counts = new int[2];
            using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(SERVER_ID) FROM servers"))
            {
                cmd.Connection = conn;
                conn.Open();
                counts[0] = Int32.Parse(cmd.ExecuteScalar().ToString() ?? "0");
                conn.Close();
            }
            using(MySqlCommand cmd = new MySqlCommand("SELECT COUNT(USER_ID) FROM sus_users WHERE HAS_BANNED=1 OR HAS_QUARANTINED=1"))
            {
                cmd.Connection = conn;
                conn.Open();
                counts[1] = Int32.Parse(cmd.ExecuteScalar().ToString() ?? "0");
                conn.Close();
            }
            return counts;
        }

        public void AddErrorLog(IExceptionHandlerPathFeature handler)
        {
            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO errors (ERROR, MESSAGE, PATH, ENDPOINT) VALUES (@error, @message, @path, @endpoint)"))
            {
                cmd.Parameters.AddWithValue("@error", handler.Error);
                cmd.Parameters.AddWithValue("@message", handler.Error.Message);
                cmd.Parameters.AddWithValue("@path", handler.Path);
                cmd.Parameters.AddWithValue("@endpoint", handler.Endpoint);

                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
