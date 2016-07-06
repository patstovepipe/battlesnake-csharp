using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;

namespace battlesnake_csharp.Controllers
{
    public class DefaultController : ApiController
    {
        public class Game : Start
        {
            public int? turn { get; set; }

            public string snake { get; set; }

            public bool topleftMovement { get; set; }

            public Game()
            {
                this.turn = 0;
                this.topleftMovement = true;
            }
        }

        public class Start
        {
            public string game_id { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Move
        {
            public List<List<int>> food { get; set; }
            public string game_id { get; set; }
            public List<Snake> snakes { get; set; }
            public List<List<BoardTile>> board { get; set; }
            public int? turn { get; set; }
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
            public List<List<int>> coords { get; set; }
            public int score { get; set; }
            public string head_url { get; set; }
            public string taunt { get; set; }
        }

        public static List<Game> games = new List<Game>();

        public static Random rnd = new Random();

        public static List<string> taunts = new List<string>()
        {
            "Is that the best you've got?",
            "Ha ha ha!",
            "Moving here and there.",
            "Snaking around...",
            "It's time to intertwine.",
            "Slither OFF!"
        };

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
            var game = JsonConvert.DeserializeObject<Game>(data.ToString());

            game.snake = "Stovepipe-C#";

            var color = "#" + getRandomHexColor();
            var taunt = string.Format("{0} crushes all opposition.", game.snake);

            if (games.Exists(g => g.game_id == game.game_id))
                game = games.Where(g => g.game_id == game.game_id).FirstOrDefault();
            else
                games.Add(game);

            return Request.CreateResponse(HttpStatusCode.OK, new { name = game.snake, color = color, taunt = taunt });
        }

        [HttpPost]
        [Route("move")]
        public HttpResponseMessage move(object data)
        {
            var reqmove = JsonConvert.DeserializeObject<Move>(data.ToString());
            var game = games.Where(g => g.game_id == reqmove.game_id).FirstOrDefault();

            if (reqmove.turn != null && game != null)
                game.turn = reqmove.turn;

            var move = GetMove(reqmove);
            var taunt = taunts.ElementAt(rnd.Next(0, taunts.Count()));

            return Request.CreateResponse(HttpStatusCode.OK, new { move = move, taunt = taunt });
        }

        private class PossibleMove
        {
            public string move { get; set; }
            public List<int> pos { get; set; }

            public PossibleMove(string prMove, List<int> prPos)
            {
                this.move = prMove;
                this.pos = prPos;
            }
        }

        private string GetMove(Move reqmove)
        {
            var game = games.Where(g => g.game_id == reqmove.game_id).FirstOrDefault();
            List<int> headPos = reqmove.snakes.Where(s => s.name == game.snake).First().coords.First();

            // Add all possible moves up, left, down, right
            var possibleMoves = new List<PossibleMove>();
            possibleMoves.Add(new PossibleMove("up", new List<int> { headPos.First(), headPos.Last() - 1 }));
            possibleMoves.Add(new PossibleMove("left",new List<int> { headPos.First() - 1, headPos.Last() }));
            possibleMoves.Add(new PossibleMove("down", new List<int> { headPos.First(), headPos.Last() + 1 }));
            possibleMoves.Add(new PossibleMove("right", new List<int> { headPos.First() + 1, headPos.Last() }));

            if (headPos.First() == 0)
                game.topleftMovement = false;

            if (headPos.First() == game.width - 1)
                game.topleftMovement = true;

            // Change the moves to go bottom right
            if (!game.topleftMovement)
            {
                var downRightOrder = new List<string> { "down", "right", "up", "left" };
                possibleMoves = possibleMoves.OrderBy(pm => downRightOrder.IndexOf(pm.move)).ToList();
            }

            // Add all occupied areas to list -- needs improvement
            var occupiedAreas = new List<List<int>>();
            reqmove.snakes.ForEach(s => occupiedAreas.AddRange(s.coords));

            // Remove any possible moves if they will be in an occupied area -- this doesnt prevent occupying a space that a snake will be going to
            possibleMoves.RemoveAll(pm => occupiedAreas.Any(oa => oa.SequenceEqual(pm.pos)));

            // Remove any possible moves if they go out of bounds
            possibleMoves.RemoveAll(pm => pm.pos.First() < 0 || pm.pos.Last() < 0 || pm.pos.First() > (game.width - 1) || pm.pos.Last() > (game.height - 1));

            return possibleMoves.First().move;
        }

        private static string getRandomHexColor()
        {
            int value = rnd.Next(0, 16777216);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            string hex = BitConverter.ToString(bytes);
            hex = hex.Substring(3, hex.Length - 3).Replace("-", "");
            return hex;
        }
    }
}
