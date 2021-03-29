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
            var gameMode = GetGameMode(args);
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            var myBot = new MyPaintBot(gameMode, serviceProvider.GetService<IPaintBotClient>(),
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

        private static GameMode GetGameMode(string[] args)
        {
            const GameMode defaultGameMode = GameMode.Training;
            if (args == null || !args.Any())
            {
                return defaultGameMode;
            }

            var couldParseGameMode = Enum.TryParse<GameMode>(args.First(), true, out var parsedGameMode);

            return couldParseGameMode ? parsedGameMode : defaultGameMode;
        }
    }
}