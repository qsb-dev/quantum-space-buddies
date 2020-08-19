using System.Collections.Generic;
using System.Linq;
using QSB.TransformSync;
using QSB.Animation;
using QSB.Messaging;
using System;
using QSB.Utility;
using System.Diagnostics;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static uint LocalPlayerId => PlayerTransformSync.LocalInstance.netId.Value;
        public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<TransformSync.TransformSync> TransformSyncs { get; } = new List<TransformSync.TransformSync>();
        public static List<TransformSync.TransformSync> LocalTransformSyncs => TransformSyncs.Where(t => t != null && t.hasAuthority).ToList();
        public static List<AnimationSync> AnimationSyncs { get; } = new List<AnimationSync>();

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
            DebugLog.ToConsole($"Removing player with id {id}, called from {stacktrace.GetFrame(1).GetMethod().DeclaringType}.{stacktrace.GetFrame(1).GetMethod().Name}", OWML.Common.MessageType.Info);
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
            DebugLog.ToConsole($"Set player {player.NetId} to ready state {player.IsReady}");
            player.State = message.PlayerState;
            DebugLog.ToConsole($"Updating state of player {player.NetId} to : {Environment.NewLine}" +
                $"{DebugLog.GenerateTable(Enum.GetNames(typeof(State)).ToList(), FlagsHelper.FlagsToListSet(player.State))}");
            if (LocalPlayer.IsReady)
            {
                player.UpdateStateObjects();
            }
        }

        public static TransformSync.TransformSync GetTransformSync(uint id)
        {
            return TransformSyncs.FirstOrDefault(x => x != null && x.netId.Value == id);
        }

        public static bool IsBelongingToLocalPlayer(uint id)
        {
            return id == LocalPlayerId || GetTransformSync(id)?.PlayerId == LocalPlayerId;
        }

        public static AnimationSync GetAnimationSync(uint id)
        {
            return AnimationSyncs.FirstOrDefault(x => x != null && x.netId.Value == id);
        }
    }
}
