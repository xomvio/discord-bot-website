using Enyim.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Bitguard.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IMemcachedClient mc;
        private MySqlConnection conn;

        public BotController(IMemcachedClient memcachedClient, IConfiguration config)
        {
            conn = new MySqlConnection(config["ConnectionStrings:DefaultConnection"]);
            mc = memcachedClient;

        }
        [HttpGet("getnewdata")]
        public async Task<IActionResult> ProcessRequest()
        {
            bool allsexy = false;
            string gobot = mc.Get<string>("gobot");
            string nodeclam = mc.Get<string>("nodeclam");
            using (MySqlCommand cmd = new MySqlCommand(@"SELECT table_schema 'bgmain3162',
sum( data_length + index_length ) * 100 /sum(data_length + index_length + data_free)
'boyut'
FROM information_schema.TABLES
where table_schema = DATABASE();"))
            {
                cmd.Connection = conn;
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    allsexy = allsexy && reader.GetInt16("boyut") < 80;

                }
                conn.Close();
            }

            if (gobot == "ping" && nodeclam == "ping")
            {
                await mc.SetAsync("gobot", "pong", TimeSpan.FromMinutes(60));
                await mc.SetAsync("nodeclam", "pong", TimeSpan.FromMinutes(60));
                allsexy = allsexy && true;
            }
            return allsexy ? Ok() : BadRequest();
        }
    }
}
