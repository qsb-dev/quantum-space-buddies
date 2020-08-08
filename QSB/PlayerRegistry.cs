using System.Collections.Generic;
using System.Linq;
using QSB.Events;
using QSB.TransformSync;
using QSB.Animation;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static PlayerInfo LocalPlayer => GetPlayer(PlayerTransformSync.LocalInstance.netId.Value);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<TransformSync.TransformSync> TransformSyncs { get; } = new List<TransformSync.TransformSync>();
        public static List<TransformSync.TransformSync> LocalTransformSyncs => TransformSyncs.Where(t => t != null && t.hasAuthority).ToList();

        public static List<AnimationSync> AnimationSyncs { get; } = new List<AnimationSync>();

        public static PlayerInfo CreatePlayer(uint id)
        {
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
            player.Name = message.PlayerName;
        }

        public static TransformSync.TransformSync GetTransformSync(uint id)
        {
            return TransformSyncs.First(x => x != null && x.netId.Value == id);
        }

        public static AnimationSync GetAnimationSync(uint id)
        {
            return AnimationSyncs.FirstOrDefault(x => x != null && x.netId.Value == id);
        }

    }
}
