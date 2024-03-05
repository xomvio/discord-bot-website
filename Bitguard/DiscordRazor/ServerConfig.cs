namespace Bitguard.DiscordRazor
{
    public class ServerConfig
    {
        public string serverId = "";
        public string susCheck = "";
        public string blockSpam = "";
        public string badWords = "";
        public string badWordsWarn = "";
        public string ntfChannel = "";
        public ServerChannel[] serverChannels;
        public ServerConfig() { }
        public ServerConfig(string _serverId, bool _susCheck, bool _blockSpam, string _badWords, string _badWordsWarn, string _ntfChannel, ServerChannel[] _serverChannels)
        {
            serverId = _serverId;
            susCheck = _susCheck == true ? "checked" : "";
            blockSpam = _blockSpam == true ? "checked" : "";
            badWords = _badWords;
            badWordsWarn = _badWordsWarn;
            serverChannels = _serverChannels;
            ntfChannel = _ntfChannel;
        }

    }
}
