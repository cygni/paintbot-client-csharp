namespace PaintBot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Game.Configuration;
    using Messaging;
    using Messaging.Request;
    using Messaging.Request.HeartBeat;
    using Messaging.Response;
    using static Serilog.Log;
    using Action = Game.Action.Action;

    public abstract class PaintBot
    {
        private readonly IPaintBotClient _paintBotClient;
        private readonly IHearBeatSender _heartBeatSender;

        private bool _hasGameEnded;
        private bool _hasTournamentEnded;
        private string _playerId;

        protected PaintBot(IPaintBotClient paintBotClient, IHearBeatSender heartBeatSender)
        {
            _paintBotClient = paintBotClient;
            _heartBeatSender = heartBeatSender;
        }

        public abstract GameMode GameMode { get; }
        public abstract string Name { get; }
        public abstract Action GetAction(MapUpdated mapUpdated);

        public async Task Run(CancellationToken ct)
        {
            try
            {
                await _paintBotClient.ConnectAsync(GameMode, ct);

                var gameSettings = new GameSettings {GameDurationInSeconds = 20}; // TODO: Remove
                await _paintBotClient.SendAsync(new RegisterPlayer(Name, gameSettings), ct);

                await foreach (var response in _paintBotClient.ReceiveEnumerableAsync<Response>(ct))
                {
                    await HandleResponseAsync(response, ct);
                    if (!IsPlaying())
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message); // TODO: Add proper logging
            }
            finally
            {
                _paintBotClient?.Close();
            }
        }

        private Task HandleResponseAsync(Response response, CancellationToken ct)
        {
            return response switch
            {
                PlayerRegistered playerRegistered => OnPlayerRegistered(playerRegistered, ct),
                MapUpdated mapUpdated => OnMapUpdated(mapUpdated, ct),
                GameLink gameLink => OnInfoEvent(gameLink),
                GameStarting gameStarting => OnInfoEvent(gameStarting),
                GameResult gameResult => OnInfoEvent(gameResult),
                CharacterStunned characterStunned => OnInfoEvent(characterStunned),
                HeartBeatResponse heartBeatResponse => OnHearBeatEvent(heartBeatResponse),
                GameEnded gameEnded => OnGameEnded(gameEnded),
                TournamentEnded tournamentEnded => OnTournamentEnded(tournamentEnded),
                InvalidPlayerName invalidPlayerName => OnInfoEvent(invalidPlayerName),
                _ => Task.CompletedTask
            };
        }

        private Task OnTournamentEnded(TournamentEnded tournamentEnded)
        {
            _hasTournamentEnded = true;
            Logger.Information("The tournament has ended"); // Don't spoil the results in the console. 
            return Task.CompletedTask;
        }

        private async Task OnPlayerRegistered(PlayerRegistered playerRegistered, CancellationToken ct)
        {
            _playerId = playerRegistered.ReceivingPlayerId;
            SendHearBeat();
            await _paintBotClient.SendAsync(new StartGame(playerRegistered.ReceivingPlayerId), ct);
            await _paintBotClient.SendAsync(new ClientInfo("C#", "8", "Windows", "10", "1", playerRegistered.ReceivingPlayerId), ct);
            Logger.Information(playerRegistered.ToString());
        }

        private async Task OnMapUpdated(MapUpdated mapUpdated, CancellationToken ct)
        {
            Logger.Information($"Tick {mapUpdated.GameTick}");
            var action = GetAction(mapUpdated);
            await _paintBotClient.SendAsync(
                new RegisterMove(mapUpdated.ReceivingPlayerId)
                {
                    GameId = mapUpdated.GameId,
                    GameTick = mapUpdated.GameTick,
                    Direction = action.ToString().ToUpper()
                }, ct);
        }

        private Task OnInfoEvent(Response response)
        {
            if (response is GameResult)
            {
                if (GameMode == GameMode.Training)
                {
                    Logger.Information($"The On info event {response}");
                }
            }

            return Task.CompletedTask;
        }

        private Task OnHearBeatEvent(HeartBeatResponse heartBeat)
        {
            Logger.Information(heartBeat.ToString());
            SendHearBeat();
            return Task.CompletedTask;
        }

        private Task OnGameEnded(GameEnded response)
        {
            _hasGameEnded = true;
            if (GameMode == GameMode.Training)
            {
                Logger.Information(response.ToString());
            }
            else if (GameMode == GameMode.Tournament)
            {
                Logger.Information("The game has ended"); // Don't spoil the result in the console.
            }

            return Task.CompletedTask;
        }

        private bool IsPlaying()
        {
            if (GameMode == GameMode.Training)
            {
                return !_hasGameEnded;
            }

            return !_hasTournamentEnded;
        }

        private void SendHearBeat()
        {
            _heartBeatSender.SendHeartBeatFrom(_playerId);
        }
    }
}