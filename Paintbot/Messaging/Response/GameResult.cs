namespace PaintBot.Messaging.Response
{
    using System;
    using System.Text;
    using Game.Result;

    public class GameResult : Response
    {
        public Guid GameId { get; set; }
        public PlayerRank[] PlayerRanks { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(base.ToString());
            foreach (var rank in PlayerRanks)
                sb.Append($"{rank}\n");

            return sb.ToString();
        }

    }
}