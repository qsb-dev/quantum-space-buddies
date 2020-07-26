using QSB.Animation;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB
{
    public static class Finder
    {
        private static readonly List<PlayerInfo> playerList = new List<PlayerInfo>();

        public static void RegisterPlayerBody(uint id, GameObject body)
        {
            DebugLog.ToConsole($"register player body {id}");
            GetPlayer(id).Body = body;
        }

        public static void CreatePlayer(uint id, string name)
        {
            DebugLog.ToConsole($"create player {id} {name}");
            playerList.Add(new PlayerInfo(id, null, null, name, false));
        }

        public static void RegisterPlayerCamera(uint id, GameObject camera)
        {
            DebugLog.ToConsole($"register player camera {id}");
            if (camera == null)
            {
                DebugLog.ToConsole("given camera object is null!");
            }
            var player = playerList.Find(x => x.NetId == id);
            player.Camera = camera;
            DebugLog.ToConsole($"*name {playerList.Find(x => x.NetId == id).Name}");
            DebugLog.ToConsole($"*body {playerList.Find(x => x.NetId == id).Body.name}");
            DebugLog.ToConsole($"*camera {playerList.Find(x => x.NetId == id).Camera.name}");
        }

        public static void RemovePlayer(uint id)
        {
            DebugLog.ToConsole($"remove player {id}");
            playerList.Remove(playerList.Find(x => x.NetId == id));
        }

        private static PlayerInfo GetPlayer(uint id)
        {
            return playerList.Find(x => x.NetId == id);
        }

        public static GameObject GetPlayerBody(uint id)
        {
            DebugLog.ToConsole($"get player body {id}");
            return GetPlayer(id).Body;
        }

        public static GameObject GetPlayerCamera(uint id)
        {
            DebugLog.ToConsole($"get player camera {id}");
            var player = playerList.Find(x => x.NetId == id);
            DebugLog.ToConsole($"*name {playerList.Find(x => x.NetId == id).Name}");
            DebugLog.ToConsole($"*body {playerList.Find(x => x.NetId == id).Body.name}");
            DebugLog.ToConsole($"*camera {playerList.Find(x => x.NetId == id).Camera.name}");
            if (GetPlayer(id).Camera == null && GetPlayer(id) != null)
            {
                DebugLog.ToScreen($"WARNING - Got player {id} but camera object is null");
            }
            return GetPlayer(id).Camera;
        }

        public static QSBFlashlight GetPlayerFlashlight(uint id)
        {
            DebugLog.ToConsole($"get player flashlight {id}");
            if (GetPlayerCamera(id) == null)
            {
                DebugLog.ToScreen($"WARNING - Camera for {id} is null");
            }
            return GetPlayerCamera(id).GetComponentInChildren<QSBFlashlight>();
        }

        public static void UpdatePlayerName(uint id, string name)
        {
            DebugLog.ToConsole($"update player name {id}");
            GetPlayer(id).Name = name;
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
            DebugLog.ToConsole($"get player name {id}");
            return GetPlayer(id).Name;
        }

        public static Dictionary<uint, string> GetPlayerNames()
        {
            var dict = new Dictionary<uint, string>();
            playerList.ForEach(x => dict.Add(x.NetId, x.Name));
            return dict;
        }

        public static bool IsPlayerReady(uint id)
        {
            return GetPlayer(id).Ready;
        }

        public static void SetReadiness(uint id, bool ready)
        {
            DebugLog.ToConsole($"set readiness {id}");
            GetPlayer(id).Ready = ready;
        }

        public static void UpdateSector(uint id, Transform sector)
        {
            DebugLog.ToConsole($"update sector {id}");
            GetPlayer(id).ReferenceSector = sector;
        }

        public static Transform GetSector(uint id)
        {
            return GetPlayer(id).ReferenceSector;
        }

        public static void UpdateState(uint id, State state, bool value)
        {
            DebugLog.ToConsole($"update state {id}.{state}.{value}");
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
            DebugLog.ToConsole($"get state {id}.{state}");
            var states = GetPlayer(id).State;
            return FlagsHelper.IsSet(states, state);
        }
    }

    public class PlayerInfo
    {
        public uint NetId { get; set; }
        public GameObject Body { get; set; }
        public GameObject Camera { get; set; }
        public string Name { get; set; }
        public bool Ready { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; set; }

        public PlayerInfo(uint netId, GameObject body, GameObject camera, string name, bool ready)
        {
            NetId = netId;
            Body = body;
            Camera = camera;
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
        //Increment these in binary to add more states
    }
}
