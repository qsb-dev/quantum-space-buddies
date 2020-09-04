using OWML.Common;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                DebugLog.ToConsole("Warning - tried to create player with id 0", MessageType.Warning);
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
            var behaviours = Resources.FindObjectsOfTypeAll<NetworkBehaviour>();
            return behaviours.Where(x => x.netId.Value == id).First().isLocalPlayer;
        }

        public static uint GetPlayerOfObject(this PlayerSyncObject syncObject)
        {
            var behaviour = (NetworkBehaviour)syncObject;
            var playerIds = PlayerList.Select(x => x.PlayerId).ToList();
            var lowerPlayerIds = playerIds.Where(x => x <= behaviour.netId.Value);
            var lowerBound = lowerPlayerIds.ToList().Find(x => x == lowerPlayerIds.Select(n => n).Max());
            if (PlayerList.Count != PlayerSyncObjects.Count(x => x.GetType() == behaviour.GetType()) && lowerBound == playerIds.Select(n => n).ToList().Max())
            {
                if (syncObject.PreviousPlayerId != uint.MaxValue)
                {
                    return syncObject.PreviousPlayerId;
                }
                if (behaviour.GetType() == typeof(PlayerTransformSync) && behaviour.netId.Value != 0U)
                {
                    return GetPlayer(behaviour.netId.Value).PlayerId;
                }
                syncObject.PreviousPlayerId = uint.MaxValue;
                return uint.MaxValue;
            }
            syncObject.PreviousPlayerId = lowerBound;
            return lowerBound;
        }

        public static List<uint> GetPlayerNetIds(PlayerInfo player)
        {
            var ints = Enumerable.Range((int)player.PlayerId, PlayerSyncObjects.DistinctBy(x => x.NetId).Count(x => x.Player.PlayerId == player.PlayerId)).Select(x => (uint)x).ToList();
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
