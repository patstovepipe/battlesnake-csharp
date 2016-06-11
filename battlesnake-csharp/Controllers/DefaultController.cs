using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace battlesnake_csharp.Controllers
{
    public class DefaultController : ApiController
    {
        public class Game : Start
        {
            public string snake { get; set; }

            public bool topleftmovement { get; set; }
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

        public static MongoClient client = new MongoClient();
        
        public static IMongoDatabase database = client.GetDatabase("battlesnake");

        //public static Game game { get; set; }

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
            var start = JsonConvert.DeserializeObject<Start>(data.ToString());
            //game = lcGame;

            var snake = "Stovepipe-C#";
            //game.snake = snake;

            database.DropCollection("game");
            var collection = database.GetCollection<BsonDocument>("game");

            var document = new BsonDocument
            {
                { "game_id" , start.game_id },
                { "width", start.width },
                { "height", start.height },
                { "snake", snake },
                { "topleftmovement", true }
            };

            collection.InsertOne(document);

            

            var color = "#" + getRandomHexColor();
            var taunt = string.Format("{0} crushes all opposition.", snake);
            var head_url = "https://battlesnake-oh-ps.herokuapp.com/static/img/default_head.png";

            return Request.CreateResponse(HttpStatusCode.OK, new { name = snake, color = color, taunt = taunt, head_url = head_url });
        }

        [HttpPost]
        [Route("move")]
        public HttpResponseMessage move(object data)
        {
            var reqmove = JsonConvert.DeserializeObject<Move>(data.ToString());

            var collection = database.GetCollection<BsonDocument>("move");

            var document = new BsonDocument
            {
                { "game_id" , reqmove.game_id },
                { "turn" , reqmove.turn }
            };

            collection.InsertOne(document);
           
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
            var collection = database.GetCollection<BsonDocument>("game");

            var filter = Builders<BsonDocument>.Filter.Eq("game_id", reqmove.game_id);
            var projection = Builders<BsonDocument>.Projection.Exclude("_id");
            var document = collection.Find(filter).Project(projection).First();

            var game = BsonSerializer.Deserialize<Game>(document);

            List<int> headPos = reqmove.snakes.Where(s => s.name == game.snake).First().coords.First();

            // Add all possible moves up, left, down, right
            var possibleMoves = new List<PossibleMove>();
            possibleMoves.Add(new PossibleMove("up", new List<int> { headPos.First(), headPos.Last() - 1 }));
            possibleMoves.Add(new PossibleMove("left",new List<int> { headPos.First() - 1, headPos.Last() }));
            possibleMoves.Add(new PossibleMove("down", new List<int> { headPos.First(), headPos.Last() + 1 }));
            possibleMoves.Add(new PossibleMove("right", new List<int> { headPos.First() + 1, headPos.Last() }));

            UpdateDefinition<BsonDocument> update = null;

            if (headPos.First() == 0)
            {
                game.topleftmovement = false;
                update = Builders<BsonDocument>.Update.Set("topleftmovement", false);
            }

            if (headPos.First() == game.width - 1)
            {
                game.topleftmovement = true;
                update = Builders<BsonDocument>.Update.Set("topleftmovement", true);
            }
            
            if (update != null)
                collection.UpdateOne(filter, update);

            // Change the moves to go bottom right
            if (!game.topleftmovement)
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
