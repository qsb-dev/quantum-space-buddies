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
        public static NetworkInstanceId LocalPlayerId => PlayerTransformSync.LocalInstance.GetComponent<NetworkIdentity>()?.netId ?? NetworkInstanceId.Invalid;
        public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        public static List<PlayerSyncObject> PlayerSyncObjects { get; } = new List<PlayerSyncObject>();

        public static PlayerInfo GetPlayer(NetworkInstanceId id)
        {
            if (id == NetworkInstanceId.Invalid || id.Value == 0U)
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

        public static void RemovePlayer(NetworkInstanceId id)
        {
            DebugLog.DebugWrite($"Removing player {GetPlayer(id).Name} id {id.Value}", MessageType.Info);
            PlayerList.Remove(GetPlayer(id));
        }

        public static bool PlayerExists(NetworkInstanceId id)
        {
            if (id == NetworkInstanceId.Invalid)
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

        public static T GetSyncObject<T>(NetworkInstanceId id) where T : PlayerSyncObject
        {
            return GetSyncObjects<T>().FirstOrDefault(x => x != null && x.NetId == id);
        }

        public static bool IsBelongingToLocalPlayer(NetworkInstanceId id)
        {
            var behaviours = Resources.FindObjectsOfTypeAll<NetworkBehaviour>();
            return behaviours.Where(x => x.netId == id).First().isLocalPlayer;
        }

        public static NetworkInstanceId GetPlayerOfObject(this PlayerSyncObject syncObject)
        {
            var behaviour = (NetworkBehaviour)syncObject;
            // Function to get a player id from any netid, for example input of 7 and players 1 and 5

            // Get all players (eg 1, 5)
            var playerIds = PlayerList.Select(x => x.PlayerId).ToList();
            // Get all players below the given netid (eg 1, 5)
            var lowerPlayerIds = playerIds.Where(x => x.Value <= behaviour.netId.Value);
            // Get the highest of those (eg 5)
            var lowerBound = lowerPlayerIds.ToList().Find(x => x.Value == lowerPlayerIds.Select(n => n.Value).Max());
            // When a new player joins, the objects spawn before the join event is sent. So in the event that happens, we
            // don't want to send the wrong player. The problem with this is that the objects for the previous player will also stop working (as it will see a mismatch)
            if (PlayerList.Count != PlayerSyncObjects.Count(x => x.GetType() == behaviour.GetType()) && lowerBound.Value == playerIds.Select(n => n.Value).ToList().Max())
            {
                if (syncObject.PreviousPlayerId != NetworkInstanceId.Invalid)
                {
                    return syncObject.PreviousPlayerId;
                }
                if (behaviour.GetType() == typeof(PlayerTransformSync) && behaviour.netId.Value != 0U)
                {
                    return GetPlayer(behaviour.netId).PlayerId;
                }
                syncObject.PreviousPlayerId = NetworkInstanceId.Invalid;
                return NetworkInstanceId.Invalid;
            }
            syncObject.PreviousPlayerId = lowerBound;
            return lowerBound;
        }

        public static List<NetworkInstanceId> GetPlayerNetIds(PlayerInfo player)
        {
            var ints = Enumerable.Range((int)player.PlayerId.Value, PlayerSyncObjects.DistinctBy(x => x.NetId).Count(x => x.Player.PlayerId == player.PlayerId)).Select(x => (uint)x).ToList();
            var networkInstances = Resources.FindObjectsOfTypeAll<NetworkIdentity>().Select(x => x.netId).DistinctBy(x => x.Value);
            return networkInstances.Where(x => ints.Contains(x.Value)).ToList();
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
