namespace PaintBot.Messaging.Request
{
    using System.Threading;

    public class HeartBeatSender
    {
        private const int DefaultHeartbeatPeriodInSeconds = 30;
        private readonly PaintBotClient _paintBotClient;
        private readonly string _playerId;

        public HeartBeatSender(string playerId, PaintBotClient paintBotClient)
        {
            _playerId = playerId;
            _paintBotClient = paintBotClient;
        }

        public void Run()
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(DefaultHeartbeatPeriodInSeconds * 1000);
                var heartBeatRequest = new HeartBeatRequest(_playerId);
                await _paintBotClient.SendAsync(heartBeatRequest, CancellationToken.None);
            }).Start();
        }
    }
}