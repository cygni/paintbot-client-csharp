namespace PaintBot
{
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;
    using Messaging.Request;
    using Messaging.Request.HeartBeat;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            ConfigureLogger();
            var myBot = serviceProvider.GetService<MyPaintBot>();
            await myBot.Run(CancellationToken.None);
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<MyPaintBot>();
            services.AddTransient<IHearBeatSender, HeartBeatSender>();
            services.AddSingleton<IPaintBotClient, PaintBotClient>();
            services.AddSingleton(new PaintBotServerConfig {BaseUrl = "wss://server.paintbot.cygni.se"});
            return services;
        }
    }
}