namespace Paintbot.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Helpers;
    using NSubstitute;
    using PaintBot.Messaging;
    using PaintBot.Messaging.Request;
    using PaintBot.Messaging.Request.HeartBeat;
    using PaintBot.Messaging.Response;
    using Xunit;

    public class PaintBotTests
    {
        private readonly Fixture _fixture;

        public PaintBotTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Run_ShouldConnectToClient_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender);

            await sut.Run(CancellationToken.None);

            await client.Received(1).ConnectAsync(sut.GameMode, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldRegisterPlayer_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender);

            await sut.Run(CancellationToken.None);

            await client.Received(1).SendAsync(Arg.Is<RegisterPlayer>(registerPlayer => registerPlayer.PlayerName == sut.Name),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Run_ShouldReceiveEventsFromClient_WhenCalled()
        {
            var client = Substitute.For<IPaintBotClient>();
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender);

            await sut.Run(CancellationToken.None);

            await foreach (var _ in client.Received(1).ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()))
            {
            }
        }

        [Fact]
        public async Task Run_ShouldSendHeartBeat_OnPlayerRegisteredEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            const string playerId = "some-id";
            var events = new List<Response>
            {
                new PlayerRegistered {ReceivingPlayerId = playerId}
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender);

            await sut.Run(CancellationToken.None);

            heartBeatSender.Received(1).SendHeartBeatFrom(playerId);
        }

        [Fact]
        public async Task Run_ShouldStartGame_OnPlayerRegisteredEvent()
        {
            var client = Substitute.For<IPaintBotClient>();
            var playerId = _fixture.Create<Guid>();
            var events = new List<Response>
            {
                new PlayerRegistered {ReceivingPlayerId = playerId.ToString()}
            };
            client.ReceiveEnumerableAsync<Response>(Arg.Any<CancellationToken>()).Returns(GetTestValues(events));
            var heartBeatSender = Substitute.For<IHearBeatSender>();
            var sut = new FakeBot(client, heartBeatSender);

            await sut.Run(CancellationToken.None);

            await client.Received(1).SendAsync(Arg.Is<StartGame>(startGame => startGame.ReceivingPlayerId.Value == playerId),
                Arg.Any<CancellationToken>());
        }

        private static async IAsyncEnumerable<Response> GetTestValues(IEnumerable<Response> events)
        {
            foreach (var e in events) yield return e;

            await Task.CompletedTask;
        }
    }
}