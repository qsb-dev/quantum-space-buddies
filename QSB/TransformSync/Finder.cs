using QSB.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.TransformSync
{
    public static class Finder
    {
        private static readonly List<PlayerInfo> playerList = new List<PlayerInfo>();

        public static void RegisterPlayer(uint id, GameObject body)
        {
            if (!playerList.Any(x => x.NetId == id))
            {
                DebugLog.ToAll("Registering new player - id of", id);
                playerList.Add(new PlayerInfo(id, body, "", false));
            }
            else
            {
                DebugLog.ToAll("Updating player body - id of", id);
                GetPlayer(id).Body = body;
            }
        }

        public static void RemovePlayer(uint id)
        {
            playerList.Remove(playerList.Find(x => x.NetId == id));
        }

        private static PlayerInfo GetPlayer(uint id)
        {
            if (playerList.Find(x => x.NetId == id) != null)
            {
                return playerList.Find(x => x.NetId == id);
            }
            DebugLog.ToAll("Couldn't find playerinfo for id", id);
            playerList.Add(new PlayerInfo(id, null, "", false));
            return playerList.Find(x => x.NetId == id);
        }

        public static GameObject GetPlayerBody(uint id)
        {
            return GetPlayer(id).Body;
        }

        public static QSBFlashlight GetPlayerFlashlight(uint id)
        {
            return GetPlayerBody(id).GetComponentInChildren<QSBFlashlight>();
        }

        public static void UpdatePlayerName(uint id, string name)
        {
            if (GetPlayer(id) == null)
            {
                DebugLog.ToScreen("updating name of non-existant player - creating player for id ", id);
                playerList.Add(new PlayerInfo(id, null, name, false));
            }
            else
            {
                GetPlayer(id).Name = name;
            }
        }

        public static void UpdatePlayerNames(Dictionary<uint, string> newDict)
        {
            foreach (var item in newDict)
            {
                GetPlayer(item.Key).Name = item.Value;
            }
        }

        public static string GetPlayerName(uint id)
        {
            return GetPlayer(id).Name;
        }

        public static Dictionary<uint, string> GetPlayerNames()
        {
            var dict = new Dictionary<uint, string>();
            foreach (var item in playerList)
            {
                dict.Add(item.NetId, item.Name);
            }
            return dict;
        }

        public static bool IsPlayerReady(uint id)
        {
            return GetPlayer(id).Ready;
        }

        public static void SetReadiness(uint id, bool ready)
        {
            GetPlayer(id).Ready = ready;
        }

        public static void UpdateSector(uint id, Transform sector)
        {
            GetPlayer(id).ReferenceSector = sector;
        }

        public static Transform GetSector(uint id)
        {
            return GetPlayer(id).ReferenceSector;
        }

        public static void UpdateState(uint id, State state, bool value)
        {
            var states = GetPlayer(id).State;
            if (value)
            {
                FlagsHelper.Set(ref states, state);
            }
            else
            {
                FlagsHelper.Unset(ref states, state);
            }
            GetPlayer(id).State = states;
        }

        public static bool GetState(uint id, State state)
        {
            var states = GetPlayer(id).State;
            return FlagsHelper.IsSet(states, state);
        }
    }

    public class PlayerInfo
    {
        public uint NetId { get; set; }
        public GameObject Body { get; set; }
        public string Name { get; set; }
        public bool Ready { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; set; }

        public PlayerInfo(uint netId, GameObject body, string name, bool ready)
        {
            NetId = netId;
            Body = body;
            Name = name;
            Ready = ready;
        }
    }

    [Flags]
    public enum State
    {
        Flashlight = 0,
        Suit = 1,
        ProbeLauncher = 2
    }
}
