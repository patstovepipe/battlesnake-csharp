using battlesnake_csharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battlesnake_csharp.Models
{
    public static class Taunt
    {
        private enum TauntType
        {
            Random,
            Marquee
        }

        private static List<string> taunts = new List<string>()
        {
            "Is that the best you've got?",
            "Ha ha ha!",
            "Moving here and there.",
            "Snaking around...",
            "It's time to intertwine.",
            "Slither OFF!"
        };

        private static string tauntLine = "";

        private static TauntType tauntType = TauntType.Marquee;

        private static Random rnd = new Random();

        private static int marqueeLength = 20;

        public static string GetTaunt(int? reqmoveTurn)
        {
            int turn = reqmoveTurn ?? 0;
            switch (tauntType)
            {
                case TauntType.Marquee:
                    return GetMarqueeTaunt(reqmoveTurn, turn);
                case TauntType.Random:
                    return taunts.ElementAt(rnd.Next(0, taunts.Count()));
                default:
                    return "Default taunt!";
            }
            // This exception should never be thrown...
            throw new Exception("Default taunt not found.");
        }

        private static string GetMarqueeTaunt(int? reqmoveTurn, int turn)
        {
            string taunt = "";
            if (string.IsNullOrEmpty(tauntLine))
            {
                for (int i = 0; i < taunts.Count(); i++)
                {
                    tauntLine += ".........." + taunts[i];
                }
            }

            if (turn >= tauntLine.Length)
            {
                int x = (int)Math.Floor((turn) / (decimal)tauntLine.Length);
                turn = turn - (tauntLine.Length * x);
            }

            if (turn + marqueeLength > tauntLine.Length)
            {
                int numover = (turn + marqueeLength) - tauntLine.Length;
                taunt = tauntLine.Substring(turn, marqueeLength - numover) + tauntLine.Substring(0, numover);
            }
            else
                taunt = tauntLine.Substring(turn, marqueeLength);

            Logger.Log("taunt", string.Format("{0} {1} {2}", reqmoveTurn, turn, taunt));
            return taunt;
        }
    }
}
