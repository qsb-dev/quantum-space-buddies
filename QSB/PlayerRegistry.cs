using System.Collections.Generic;
using System.Linq;
using QSB.Events;
using QSB.TransformSync;
using QSB.Animation;
using QSB.Utility;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static PlayerInfo LocalPlayer => GetPlayer(PlayerTransformSync.LocalInstance.netId.Value);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<TransformSync.TransformSync> TransformSyncs { get; } = new List<TransformSync.TransformSync>();
        public static List<TransformSync.TransformSync> LocalTransformSyncs => TransformSyncs.Where(t => t.hasAuthority).ToList();

        public static List<AnimationSync> AnimationSyncs { get; } = new List<AnimationSync>();

        public static PlayerInfo CreatePlayer(uint id)
        {
            DebugLog.ToConsole($"Creating player {id}");
            if (PlayerExists(id))
            {
                return null;
            }
            var player = new PlayerInfo(id);
            PlayerList.Add(player);
            return player;
        }

        public static PlayerInfo GetPlayer(uint id)
        {
            return PlayerList.FirstOrDefault(x => x.NetId == id);
        }

        public static bool PlayerExists(uint id)
        {
            return GetPlayer(id) != null;
        }

        public static void RemovePlayer(uint id)
        {
            PlayerList.Remove(GetPlayer(id));
        }

        public static void HandleFullStateMessage(FullStateMessage message)
        {
            var player = GetPlayer(message.SenderId) ?? CreatePlayer(message.SenderId);
            DebugLog.ToConsole($"Player Id {message.SenderId} ----------------", OWML.Common.MessageType.Info);
            player.Name = message.PlayerName;
            DebugLog.ToConsole($"Name : {player.Name}", OWML.Common.MessageType.Info);
            player.IsReady = message.IsReady;
            DebugLog.ToConsole($"IsReady : {player.IsReady}", OWML.Common.MessageType.Info);
        }

        public static TransformSync.TransformSync GetTransformSync(uint id)
        {
            return TransformSyncs.Single(x => x.netId.Value == id);
        }

        public static AnimationSync GetAnimationSync(uint id)
        {
            return AnimationSyncs.SingleOrDefault(x => x.netId.Value == id);
        }

    }
}
