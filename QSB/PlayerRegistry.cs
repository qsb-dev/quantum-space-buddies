using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QSB
{
    public static class PlayerRegistry
    {
        public const int NetworkObjectCount = 4;

        public static uint LocalPlayerId => PlayerTransformSync.LocalInstance.netId.Value;
        public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<PlayerSyncObject> PlayerSyncObjects { get; } = new List<PlayerSyncObject>();

        public static PlayerInfo GetPlayer(uint id)
        {
            var player = PlayerList.FirstOrDefault(x => x.NetId == id);
            if (player != null)
            {
                return player;
            }
            var stacktrace = new StackTrace();
            DebugLog.ToConsole($"Creating player with id {id}, called from {stacktrace.GetFrame(1).GetMethod().DeclaringType}.{stacktrace.GetFrame(1).GetMethod().Name}", OWML.Common.MessageType.Info);
            player = new PlayerInfo(id);
            PlayerList.Add(player);
            return player;
        }

        public static void RemovePlayer(uint id)
        {
            var stacktrace = new StackTrace();
            DebugLog.ToConsole($"Removing player with id {id}, called from {stacktrace.GetFrame(1).GetMethod().DeclaringType.Name}.{stacktrace.GetFrame(1).GetMethod().Name}", OWML.Common.MessageType.Info);
            PlayerList.Remove(GetPlayer(id));
        }

        public static bool PlayerExists(uint id)
        {
            return PlayerList.Any(x => x.NetId == id);
        }

        public static void HandleFullStateMessage(PlayerStateMessage message)
        {
            var player = GetPlayer(message.AboutId);
            player.Name = message.PlayerName;
            player.IsReady = message.PlayerReady;
            player.State = message.PlayerState;
            DebugLog.ToConsole($"Updating state of player {player.NetId} to : {Environment.NewLine}" +
                $"{DebugLog.GenerateTable(Enum.GetNames(typeof(State)).ToList(), FlagsHelper.FlagsToListSet(player.State))}");
            if (LocalPlayer.IsReady)
            {
                player.UpdateStateObjects();
            }
        }

        public static IEnumerable<T> GetSyncObjects<T>() where T : PlayerSyncObject
        {
            return PlayerSyncObjects.OfType<T>();
        }

        public static T GetSyncObject<T>(uint id) where T : PlayerSyncObject
        {
            return GetSyncObjects<T>().FirstOrDefault(x => x.NetId == id);
        }

        public static bool IsBelongingToLocalPlayer(uint id)
        {
            return id == LocalPlayerId || GetSyncObject<PlayerSyncObject>(id)?.PlayerId == LocalPlayerId;
        }

        public static List<uint> GetPlayerNetIds(PlayerInfo player)
        {
            return Enumerable.Range((int)player.NetId, NetworkObjectCount).Select(x => (uint)x).ToList();
        }

    }
}
