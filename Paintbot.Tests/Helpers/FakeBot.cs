namespace Paintbot.Tests.Helpers
{
    using PaintBot;
    using PaintBot.Game.Action;
    using PaintBot.Game.Configuration;
    using PaintBot.Messaging;
    using PaintBot.Messaging.Request.HeartBeat;
    using PaintBot.Messaging.Response;

    public class FakeBot : PaintBot
    {
        public FakeBot(IPaintBotClient paintBotClient, IHearBeatSender heartBeatSender) : base(paintBotClient, heartBeatSender)
        {
        }

        public override GameMode GameMode => GameMode.Training;
        public override string Name => "FakeBot";

        public override Action GetAction(MapUpdated mapUpdated)
        {
            return Action.Stay;
        }
    }
}