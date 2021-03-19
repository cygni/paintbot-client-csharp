namespace PaintBot.Messaging.Response
{
    using Extensions;

    public abstract class Response
    {
        public string Type { get; set; }
        public string ReceivingPlayerId { get; set; }
        public long Timestamp { get; set; }

        public override string ToString()
        {
            return $"{Timestamp.ToDateTime().ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} ";
        }
    }
}
