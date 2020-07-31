using System.Collections.Generic;
using System.Linq;
using QSB.Events;
using QSB.TransformSync;
using QSB.Utility;
using OWML.Common;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static PlayerInfo LocalPlayer => GetPlayer(PlayerTransformSync.LocalInstance.netId.Value);

        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static PlayerInfo CreatePlayer(uint id)
        {
            if (PlayerExists(id))
            {
                return null;
            }
            DebugLog.ToConsole($"Creating player id {id}", MessageType.Info);
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
            DebugLog.ToConsole($"Removing player id {id}", MessageType.Info);
            PlayerList.Remove(GetPlayer(id));
        }

        public static void HandleFullStateMessage(FullStateMessage message)
        {
            var player = GetPlayer(message.SenderId) ?? CreatePlayer(message.SenderId);
            player.Name = message.PlayerName;
        }
    }
}
