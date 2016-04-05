using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace battlesnake_csharp.Controllers
{
    public class DefaultController : ApiController
    {
        public class Game : Start
        {
            public string turn { get; set; }
        }

        public class Start
        {
            public string game_id { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Move
        {
            public int[,] food { get; set; }
            public string game_id { get; set; }
            public Snake[] snakes { get; set; }
            public BoardTile[][] board { get; set; }
            public string turn { get; set; }
        }

        public class BoardTile
        {
            public string state { get; set; }
            public string snake { get; set; }
        }

        public class Snake
        {
            public string name { get; set; }
            public string state { get; set; }
            public int[,] coords { get; set; }
            public int score { get; set; }
            public string head_url { get; set; }
            public string taunt { get; set; }
        }

        public static Game game { get; set; }

        [HttpGet]
        [Route("")]
        public HttpResponseMessage index()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Index page nothing to see here....");
        }

        [HttpPost]
        [Route("start")]
        public HttpResponseMessage start(object data)
        {
            game = JsonConvert.DeserializeObject<Game>(data.ToString());

            var name = "Stovepipe-C#";
            var color = "#ffffff";
            var taunt = "Ok then!";

            return Request.CreateResponse(HttpStatusCode.OK, new { name = name, color = color, taunt = taunt });
        }

        [HttpPost]
        [Route("move")]
        public HttpResponseMessage move(object data)
        {
            var reqmove = JsonConvert.DeserializeObject<Move>(data.ToString());
            game.turn = reqmove.turn;

            var move = "up";
            var taunt = "Moving!!";

            return Request.CreateResponse(HttpStatusCode.OK, new { move = move, taunt = taunt });
        }

    }
}
