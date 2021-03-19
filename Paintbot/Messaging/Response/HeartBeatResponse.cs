namespace PaintBot.Messaging.Response
{
    public class HeartBeatResponse : Response
    {
        public override string ToString()
        {
            return base.ToString() + "Received heartbeat from server";
        }
    }
}