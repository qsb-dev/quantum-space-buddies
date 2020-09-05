using OWML.Common;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static uint LocalPlayerId => PlayerTransformSync.LocalInstance.GetComponent<NetworkIdentity>()?.netId.Value ?? uint.MaxValue;
        public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<PlayerSyncObject> PlayerSyncObjects { get; } = new List<PlayerSyncObject>();

        public static PlayerInfo GetPlayer(uint id)
        {
            if (id == uint.MaxValue || id == 0U)
            {
                return default;
            }
            var player = PlayerList.FirstOrDefault(x => x.PlayerId == id);
            if (player != null)
            {
                return player;
            }
            DebugLog.DebugWrite($"Creating player id {id}", MessageType.Info);
            player = new PlayerInfo(id);
            PlayerList.Add(player);
            return player;
        }

        public static void RemovePlayer(uint id)
        {
            DebugLog.DebugWrite($"Removing player {GetPlayer(id).Name} id {id}", MessageType.Info);
            PlayerList.Remove(GetPlayer(id));
        }

        public static bool PlayerExists(uint id)
        {
            if (id == uint.MaxValue)
            {
                return false;
            }
            return PlayerList.Any(x => x.PlayerId == id);
        }

        public static void HandleFullStateMessage(PlayerStateMessage message)
        {
            var player = GetPlayer(message.AboutId);
            player.Name = message.PlayerName;
            player.IsReady = message.PlayerReady;
            player.State = message.PlayerState;
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
            return GetSyncObjects<T>().FirstOrDefault(x => x != null && x.AttachedNetId == id);
        }

        public static bool IsBelongingToLocalPlayer(uint id)
        {
            return id == LocalPlayerId ||
                PlayerSyncObjects.Any(x => x != null && x.AttachedNetId == id && x.isLocalPlayer);
        }

        public static uint GetPlayerOfObject(this PlayerSyncObject syncObject)
        {
            var playerIds = PlayerList.Select(x => x.PlayerId).ToList();
            var lowerBound = playerIds.Where(x => x <= syncObject.AttachedNetId).ToList().Max();
            if (PlayerList.Count != PlayerSyncObjects.Count(x => x.GetType() == syncObject.GetType()) && lowerBound == playerIds.Max())
            {
                if (syncObject.PreviousPlayerId != uint.MaxValue)
                {
                    return syncObject.PreviousPlayerId;
                }
                if (syncObject.GetType() == typeof(PlayerTransformSync) && syncObject.AttachedNetId != 0U)
                {
                    return GetPlayer(syncObject.AttachedNetId).PlayerId;
                }
                syncObject.PreviousPlayerId = uint.MaxValue;
                return uint.MaxValue;
            }
            syncObject.PreviousPlayerId = lowerBound;
            return lowerBound;
        }

        public static List<uint> GetPlayerNetIds(PlayerInfo player)
        {
            var ints = Enumerable.Range((int)player.PlayerId, PlayerSyncObjects.DistinctBy(x => x.AttachedNetId).Count(x => x.Player.PlayerId == player.PlayerId)).Select(x => (uint)x).ToList();
            return ints;
        }

        private static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
