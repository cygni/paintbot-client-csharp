﻿namespace PaintBot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Game.Configuration;
    using Messaging;
    using Messaging.Request;
    using Messaging.Request.HeartBeat;
    using Messaging.Response;
    using Serilog;
    using static Serilog.Log;
    using Action = Game.Action.Action;

    public abstract class PaintBot
    {
        private readonly IPaintBotClient _paintBotClient;
        private readonly IHearBeatSender _heartBeatSender;
        private readonly ILogger _logger;

        private bool _hasGameEnded;
        private bool _hasTournamentEnded;
        private string _playerId;

        protected PaintBot(IPaintBotClient paintBotClient, IHearBeatSender heartBeatSender, ILogger logger)
        {
            _paintBotClient = paintBotClient;
            _heartBeatSender = heartBeatSender;
            _logger = logger;
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
                _logger.Error(ex, ex.Message);
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
            _logger.Information("The tournament has ended"); // Don't spoil the results in the console. 
            return Task.CompletedTask;
        }

        private async Task OnPlayerRegistered(PlayerRegistered playerRegistered, CancellationToken ct)
        {
            _playerId = playerRegistered.ReceivingPlayerId;
            SendHearBeat();
            await _paintBotClient.SendAsync(new StartGame(playerRegistered.ReceivingPlayerId), ct);
            await _paintBotClient.SendAsync(new ClientInfo("C#", "8", "Windows", "10", "1", playerRegistered.ReceivingPlayerId), ct); //TODO: Change this
            _logger.Information(playerRegistered.ToString());
        }

        private async Task OnMapUpdated(MapUpdated mapUpdated, CancellationToken ct)
        {
            _logger.Information($"Tick {mapUpdated.GameTick}");
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
                    _logger.Information($"{response}");
                }
            }
            else
            {
                _logger.Information($"{response}");
            }

            return Task.CompletedTask;
        }

        private Task OnHearBeatEvent(HeartBeatResponse heartBeat)
        {
            _logger.Information(heartBeat.ToString());
            SendHearBeat();
            return Task.CompletedTask;
        }

        private Task OnGameEnded(GameEnded response)
        {
            _hasGameEnded = true;
            if (GameMode == GameMode.Training)
            {
                _logger.Information(response.ToString());
            }
            else if (GameMode == GameMode.Tournament)
            {
                _logger.Information("The game has ended"); // Don't spoil the result in the console.
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