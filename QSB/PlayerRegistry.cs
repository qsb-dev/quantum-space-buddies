using OWML.Common;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace QSB
{
    public static class PlayerRegistry
    {
        public const int NetworkObjectCount = 4;

        public static uint LocalPlayerId => PlayerTransformSync.LocalInstance.GetComponent<NetworkIdentity>()?.netId.Value ?? 0;
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
            DebugLog.DebugWrite($"Creating player with id {id}", MessageType.Info);
            player = new PlayerInfo(id);
            PlayerList.Add(player);
            return player;
        }

        public static void RemovePlayer(uint id)
        {
            DebugLog.DebugWrite($"Removing player with id {id}", MessageType.Info);
            PlayerList.Remove(GetPlayer(id));
        }

        public static bool PlayerExists(uint id)
        {
            return PlayerList.Any(x => x.NetId == id);
        }

        public static void HandleFullStateMessage(PlayerStateMessage message)
        {
            DebugLog.DebugWrite($"Handle full state message for player {message.AboutId}");
            var player = GetPlayer(message.AboutId);
            player.Name = message.PlayerName;
            player.IsReady = message.PlayerReady;
            DebugLog.DebugWrite($"* Is ready? : {player.IsReady}");
            player.State = message.PlayerState;
            DebugLog.DebugWrite($"* Suit is on? : {FlagsHelper.IsSet(player.State, State.Suit)}");
            //DebugLog.DebugWrite($"Updating state of player {player.NetId} to : {Environment.NewLine}" +
            //    $"{DebugLog.GenerateTable(Enum.GetNames(typeof(State)).ToList(), FlagsHelper.FlagsToListSet(player.State))}");
            if (LocalPlayer.IsReady)
            {
                player.UpdateStateObjects();
            }
        }

        public static IEnumerable<T> GetSyncObjects<T>() where T : PlayerSyncObject
        {
            return PlayerSyncObjects.OfType<T>().Where(x => x != null);
        }

        public static T GetSyncObject<T>(uint id) where T : PlayerSyncObject
        {
            return GetSyncObjects<T>().FirstOrDefault(x => x != null && x.NetId == id);
        }

        public static bool IsBelongingToLocalPlayer(uint id)
        {
            return id == LocalPlayerId ||
                   PlayerSyncObjects.Any(x => x != null && x.NetId == id && x.PlayerId == LocalPlayerId);
        }

        public static List<uint> GetPlayerNetIds(PlayerInfo player)
        {
            return Enumerable.Range((int)player.NetId, NetworkObjectCount).Select(x => (uint)x).ToList();
        }
    }
}
