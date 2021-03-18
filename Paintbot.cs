namespace PaintBot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Game.Configuration;
    using Messaging;
    using Messaging.Request;
    using Messaging.Response;
    using Action = Game.Action.Action;
    using static Serilog.Log;

    public abstract class Paintbot
    {
        private Client _client;
        private bool _hasGameEnded;
        private bool _hasTournamentEnded;
        private string _playerId;

        public abstract GameMode GameMode { get; }
        public abstract string Name { get; }
        public abstract Action GetAction(MapUpdated mapUpdated);

        public async Task Run(CancellationToken ct)
        {
            try
            {
                var url = $"wss://server.paintbot.cygni.se/{GameMode.ToString().ToLower()}";
                _client = await Client.ConnectAsync(new Uri(url), ct);

                await _client.SendAsync(new RegisterPlayer(Name, new GameSettings()), ct);

                await foreach (var response in _client.ReceiveEnumerableAsync<Response>(ct))
                {
                    await HandleResponseAsync(response, ct);
                    if (!IsPlaying()) break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message); // TODO: Add proper logging
            }
            finally
            {
                _client?.Close();
            }
        }

        private Task HandleResponseAsync(Response response, CancellationToken ct) => response switch
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

        private Task OnTournamentEnded(TournamentEnded tournamentEnded)
        {
            _hasTournamentEnded = true;
            Logger.Information(tournamentEnded.ToString());
            return Task.CompletedTask;
        }

        private async Task OnPlayerRegistered(PlayerRegistered playerRegistered, CancellationToken ct)
        {
            _playerId = playerRegistered.ReceivingPlayerId;
            SendHearBeat();
            await _client.SendAsync(new StartGame(playerRegistered.ReceivingPlayerId), ct);
            await _client.SendAsync(new ClientInfo("C#", "8", "Windows", "10", "1", playerRegistered.ReceivingPlayerId), ct);
            Logger.Information(playerRegistered.ToString());
        }

        private async Task OnMapUpdated(MapUpdated mapUpdated, CancellationToken ct)
        {
            var action = GetAction(mapUpdated);
            await _client.SendAsync(
                new RegisterMove(mapUpdated.ReceivingPlayerId)
                {
                    GameId = mapUpdated.GameId,
                    GameTick = mapUpdated.GameTick,
                    Direction = action.ToString().ToUpper()
                }, ct);
        }

        private Task OnInfoEvent(Response response)
        {
            Logger.Information(response.ToString());
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
            Logger.Information(response.ToString());
            return Task.CompletedTask;
        }

        private bool IsPlaying()
        {
            if (GameMode == GameMode.Training)
                return !_hasGameEnded;
            return !_hasTournamentEnded;
        }

        private void SendHearBeat()
        {
            new HeartBeatSender(_playerId, _client).Run();
        }
    }
}