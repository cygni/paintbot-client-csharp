namespace PaintBot.Messaging.Request
{
    using System.Threading;

    public class HeartBeatSender
    {
        private const int DefaultHeartbeatPeriodInSeconds = 30;
        private readonly Client _client;
        private readonly string _playerId;

        public HeartBeatSender(string playerId, Client client)
        {
            _playerId = playerId;
            _client = client;
        }

        public void Run()
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(DefaultHeartbeatPeriodInSeconds * 1000);
                var heartBeatRequest = new HeartBeatRequest(_playerId);
                await _client.SendAsync(heartBeatRequest, CancellationToken.None);
            }).Start();
        }
    }
}