using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using OWML.ModHelper.Events;
using System;
using System.Reflection;
using System.Collections;

namespace QSB.Events
{
    public class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, name => StartSendEvent(CreateMessage(name)));
        }

        public override void CloseListener()
        {
            DebugLog.ToConsole("Close listener for join event");
            GlobalMessenger<string>.RemoveListener(EventNames.QSBPlayerJoin, name => StartSendEvent(CreateMessage(name)));
        }

        private void StartSendEvent(PlayerJoinMessage message)
        {
            DebugLog.ToConsole("Got fire event for player join, sending message");
            SendEvent(message);
        }

        private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
        {
            SenderId = PlayerTransformSync.LocalInstance.netId.Value,
            PlayerName = name
        };

        public override void OnReceiveRemote(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(message.SenderId);
            player.Name = message.PlayerName;
            var text = $"{player.Name} joined!";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            DebugLog.ToConsole($"OnReceiveLocal player join event, from {message.SenderId}");
            var player = PlayerRegistry.CreatePlayer(PlayerTransformSync.LocalInstance.netId.Value);
            player.Name = message.PlayerName;
            var text = $"Connected to server as {player.Name}.";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }
    }
}
