namespace PaintBot
{
    using Game.Configuration;

    public class PaintBotConfig
    {
        public PaintBotConfig(GameMode gameMode, int gameLengthInSeconds)
        {
            GameMode = gameMode;
            GameLengthInSeconds = gameLengthInSeconds;
        }

        public GameMode GameMode { get; }
        public int GameLengthInSeconds { get; }
    }
}