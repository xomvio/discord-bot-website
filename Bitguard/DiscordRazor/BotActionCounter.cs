namespace Bitguard.DiscordRazor
{
    public static class BotActionCounter    //counts of bots activity. 
    {
        public static DateTime lastChange { get; set; }
        public static int serverCount { get; set; }
        public static int susCount { get; set; }
        public static void Updater(IConfiguration config)
        {
            if (DateTime.Now.Hour != lastChange.Hour)   //refresh every hour.
            {
                lastChange = DateTime.Now;
                DbActions db = new DbActions(config);
                int[] c = db.GetBotActionCounts();
                serverCount = c[0];
                susCount = c[1];
            }
        }
    }
}
