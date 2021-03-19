namespace PaintBot.Messaging.Request
{
    public class HeartBeatRequest : Request
    {
        public HeartBeatRequest(string receivingPlayerId) : base(receivingPlayerId)
        {
        }

        public override string Type => MessageType.HeartBeatRequest;
    }
}