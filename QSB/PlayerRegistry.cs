using System.Collections.Generic;
using System.Linq;
using QSB.TransformSync;
using QSB.Animation;
using QSB.Messaging;
using QSB.Utility;

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
        public static List<PlayerHUDMarker> PlayerHudMarkers { get; } = new List<PlayerHUDMarker>();

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
            DebugLog.ToConsole($"Removing player {id}");
            PlayerList.Remove(GetPlayer(id));
        }

        public static void HandleFullStateMessage(PlayerStateMessage message)
        {
            var player = GetPlayer(message.AboutId) ?? CreatePlayer(message.AboutId);
            player.Name = message.PlayerName;
            player.IsReady = message.PlayerReady;
            player.State = message.PlayerState;

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
            return id == LocalPlayerId || GetTransformSync(id).PlayerId == LocalPlayerId;
        }

        public static AnimationSync GetAnimationSync(uint id)
        {
            return AnimationSyncs.FirstOrDefault(x => x != null && x.netId.Value == id);
        }

        public static PlayerHUDMarker GetPlayerMarker(uint id)
        {
            return PlayerHudMarkers.FirstOrDefault(x => x != null && x._player.NetId == id);
        }
    }
}
