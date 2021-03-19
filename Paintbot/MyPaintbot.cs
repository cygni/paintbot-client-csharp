namespace PaintBot
{
    using System.Linq;
    using Game.Action;
    using Game.Configuration;
    using Game.Map;
    using Messaging.Response;

    public class MyPaintbot : Paintbot
    {
        private IMapUtils _mapUtils;
        public override GameMode GameMode => GameMode.Training;
        public override string Name => "My Bot";

        public override Action GetAction(MapUpdated mapUpdated)
        {
            _mapUtils = new MapUtils(mapUpdated.Map);

            var myCharacter = _mapUtils.GetCharacterInfoFor(mapUpdated.ReceivingPlayerId);
            var myCoordinate = _mapUtils.GetCoordinateFrom(myCharacter.Position);
            var myColouredTiles = _mapUtils.GetCoordinatesFrom(myCharacter.ColouredPositions);


            if (myCharacter.CarryingPowerUp)
                return Action.Explode;

            var coordinateLeft = myCoordinate.MoveIn(Action.Left);
            if (!myColouredTiles.Contains(coordinateLeft) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Left))
                return Action.Left;

            var coordinateRight = myCoordinate.MoveIn(Action.Right);
            if (!myColouredTiles.Contains(coordinateRight) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Right))
                return Action.Right;

            var coordinateUp = myCoordinate.MoveIn(Action.Up);
            if (!myColouredTiles.Contains(coordinateUp) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Up))
                return Action.Up;

            var coordinateDown = myCoordinate.MoveIn(Action.Down);
            if (!myColouredTiles.Contains(coordinateDown) &&
                _mapUtils.CanPlayerPerformAction(myCharacter.Id, Action.Down))
                return Action.Down;

            return Action.Stay;
        }

        private Action GetDirectionTo(MapCoordinate myPosition, MapCoordinate desired)
        {
            if (myPosition.X < desired.X)
                return Action.Right;
            if (myPosition.X > desired.X)
                return Action.Left;
            if (myPosition.Y < desired.Y)
                return Action.Down;
            if (myPosition.Y > desired.Y)
                return Action.Up;
            return Action.Stay;
        }
    }
}