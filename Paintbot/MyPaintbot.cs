namespace PaintBot
{
    using System.Collections.Generic;
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

        public MyPaintBot(PaintBotConfig paintBotConfig, IPaintBotClient paintBotClient, IHearBeatSender hearBeatSender, ILogger logger) :
            base(paintBotConfig, paintBotClient, hearBeatSender, logger)
        {
            GameMode = paintBotConfig.GameMode;
            Name = paintBotConfig.Name ?? "My c# bot";
        }

        public override GameMode GameMode { get; }

        public override string Name { get; }

        public override Action GetAction(MapUpdated mapUpdated)
        {
            _mapUtils = new MapUtils(mapUpdated.Map); // Keep this

            // Implement your bot here!

            // The following is a simple example bot. It tries to
            // 1. Explode PowerUp
            // 2. Move to a tile that it is not currently owning
            // 3. Move in the direction where it can move for the longest time. 

            var directions = new List<Action> {Action.Down, Action.Right, Action.Left, Action.Up};

            var myCharacter = _mapUtils.GetCharacterInfoFor(mapUpdated.ReceivingPlayerId);
            var myCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position);
            var myColouredTiles = _mapUtils.GetCoordinatesFrom(myCharacter.ColouredPositions);

            if (myCharacter.CarryingPowerUp)
            {
                return Action.Explode;
            }

            var validActionsThatPaintsNotOwnedTile = directions.Where(dir =>
                !myColouredTiles.Contains(myCoordinate.MoveIn(dir)) && _mapUtils.IsMovementPossibleTo(myCoordinate.MoveIn(dir))).ToList();

            if (validActionsThatPaintsNotOwnedTile.Any())
            {
                return validActionsThatPaintsNotOwnedTile.First();
            }

            var possibleLeftMoves = 0;
            var possibleRightMoves = 0;
            var possibleUpMoves = 0;
            var possibleDownMoves = 0;

            var testCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position).MoveIn(Action.Left);
            while (_mapUtils.IsMovementPossibleTo(testCoordinate))
            {
                possibleLeftMoves++;
                testCoordinate = testCoordinate.MoveIn(Action.Left);
            }

            testCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position).MoveIn(Action.Right);
            while (_mapUtils.IsMovementPossibleTo(testCoordinate))
            {
                possibleRightMoves++;
                testCoordinate = testCoordinate.MoveIn(Action.Right);
            }

            testCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position).MoveIn(Action.Up);
            while (_mapUtils.IsMovementPossibleTo(testCoordinate))
            {
                possibleUpMoves++;
                testCoordinate = testCoordinate.MoveIn(Action.Up);
            }

            testCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position).MoveIn(Action.Down);
            while (_mapUtils.IsMovementPossibleTo(testCoordinate))
            {
                possibleDownMoves++;
                testCoordinate = testCoordinate.MoveIn(Action.Down);
            }

            var list = new List<(Action, int)>
            {
                (Action.Left, possibleLeftMoves),
                (Action.Right, possibleRightMoves),
                (Action.Up, possibleUpMoves),
                (Action.Down, possibleDownMoves)
            };

            list.Sort((first, second) => first.Item2.CompareTo(second.Item2));
            list.Reverse();

            return list.FirstOrDefault(l => l.Item2 > 0).Item1;
        }
    }
}