namespace PaintBot
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Game.Configuration;
    using Messaging;
    using Messaging.Request.HeartBeat;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = GetConfig(args);
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var myBot = new MyPaintBot(config, serviceProvider.GetService<IPaintBotClient>(),
                serviceProvider.GetService<IHearBeatSender>(), serviceProvider.GetService<ILogger>());
            await myBot.Run(CancellationToken.None);
        }

        private static void ConfigureLogger(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            services.AddSingleton<ILogger>(logger);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureLogger(services);
            services.AddTransient<IHearBeatSender, HeartBeatSender>();
            services.AddSingleton<IPaintBotClient, PaintBotClient>();
            services.AddSingleton(new PaintBotServerConfig {BaseUrl = "wss://server.paintbot.cygni.se"});
            return services;
        }

        private static PaintBotConfig GetConfig(string[] args)
        {
            const GameMode defaultGameMode = GameMode.Training;
            const int defaultGameLengthInSeconds = 120;
            if (args == null || !args.Any())
            {
                return new PaintBotConfig(defaultGameMode, defaultGameLengthInSeconds);
            }

            var couldParseGameMode = Enum.TryParse<GameMode>(args.First(), true, out var parsedGameMode);
            if (!couldParseGameMode)
            {
                throw new ArgumentException($"Invalid game mode {args.First()}. Should be either Tournament or Training");
            }

            if (args.Length <= 1)
            {
                return new PaintBotConfig(parsedGameMode, defaultGameLengthInSeconds);
            }

            var couldParseGameLength = int.TryParse(args.ElementAt(1), out var parsedGameLength);

            if (!couldParseGameLength)
            {
                throw new ArgumentException($"Invalid game length {args.ElementAt(1)}");
            }

            return new PaintBotConfig(parsedGameMode, parsedGameLength);

        }
    }
}