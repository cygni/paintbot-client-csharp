namespace PaintBot
{
    using System.Linq;
    using Game.Action;
    using Game.Configuration;
    using Game.Map;
    using Messaging;
    using Messaging.Request.HeartBeat;
    using Messaging.Response;
    using Serilog;

    public class MyPaintBot : PaintBot
    {
        private IMapUtils _mapUtils;

        public MyPaintBot(PaintBotConfig paintBotConfig, IPaintBotClient paintBotClient, IHearBeatSender hearBeatSender, ILogger logger) : base(paintBotConfig, paintBotClient, hearBeatSender, logger)
        {
            GameMode = paintBotConfig.GameMode;
        }

        public override GameMode GameMode { get; }

        public override string Name => "My Bot";

        public override Action GetAction(MapUpdated mapUpdated)
        {
            _mapUtils = new MapUtils(mapUpdated.Map); // Keep this

            // Implement your bot here! 

            var myCharacter = _mapUtils.GetCharacterInfoFor(mapUpdated.ReceivingPlayerId);
            var myCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position);
            var myColouredTiles = _mapUtils.GetCoordinatesFrom(myCharacter.ColouredPositions);

            if (myCharacter.CarryingPowerUp)
            {
                return Action.Explode;
            }

            var coordinateLeft = myCoordinate.MoveIn(Action.Left);
            if (!myColouredTiles.Contains(coordinateLeft) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Left))
            {
                return Action.Left;
            }

            var coordinateRight = myCoordinate.MoveIn(Action.Right);
            if (!myColouredTiles.Contains(coordinateRight) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Right))
            {
                return Action.Right;
            }

            var coordinateUp = myCoordinate.MoveIn(Action.Up);
            if (!myColouredTiles.Contains(coordinateUp) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Up))
            {
                return Action.Up;
            }

            var coordinateDown = myCoordinate.MoveIn(Action.Down);
            if (!myColouredTiles.Contains(coordinateDown) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Down))
            {
                return Action.Down;
            }

            return Action.Stay;
        }
    }
}