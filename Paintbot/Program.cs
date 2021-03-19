namespace PaintBot
{
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigureLogger();
            var myBot = new MyPaintbot();
            await myBot.Run(CancellationToken.None);
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}