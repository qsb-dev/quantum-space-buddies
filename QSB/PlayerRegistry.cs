using OWML.Common;
using QSB.Animation;
using QSB.Events;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB
{
    public static class PlayerRegistry
    {
        public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

        // Methods relating to the PlayerList :

        public static void CreatePlayer(uint id, string name)
        {
            if (PlayerExists(id))
            {
                return;
            }
            DebugLog.ToConsole("Creating player " + id);
            var player = new PlayerInfo
            {
                NetId = id,
                Name = name
            };
            PlayerList.Add(player);
        }

        private static PlayerInfo GetPlayer(uint id)
        {
            return PlayerList.Find(x => x.NetId == id);
        }

        public static bool PlayerExists(uint id)
        {
            return PlayerList.Any(x => x.NetId == id);
        }

        public static void RemovePlayer(uint id)
        {
            PlayerList.Remove(PlayerList.Find(x => x.NetId == id));
        }

        public static Dictionary<uint, string> GetPlayerNames()
        {
            var dict = new Dictionary<uint, string>();
            PlayerList.ForEach(x => dict.Add(x.NetId, x.Name));
            return dict;
        }

        // Registering player objects :

        public static void RegisterPlayerBody(uint id, GameObject body)
        {
            DebugLog.ToConsole("register player body " + id);
            GetPlayer(id).Body = body;
        } 

        public static void RegisterPlayerCamera(uint id, GameObject camera)
        {
            DebugLog.ToConsole("register player camera " + id);
            GetPlayer(id).Camera = camera;
        }

        // Get player objects :

        public static GameObject GetPlayerCamera(uint id)
        {
            return GetPlayer(id).Camera;
        }

        public static QSBFlashlight GetPlayerFlashlight(uint id)
        {
            return GetPlayerCamera(id).GetComponentInChildren<QSBFlashlight>();
        }

        public static QSBTool GetPlayerSignalscope(uint id)
        {
            var tools = GetPlayer(id).Camera.GetComponentsInChildren<QSBTool>();
            if (tools.Length == 0)
            {
                DebugLog.ToConsole("Error - Zero items in QSBTool list while trying to get Signalscope", MessageType.Error);
                return null;
            }
            return tools.First(x => x.Type == ToolType.Signalscope);
        }

        // Update player data :

        public static void HandleFullStateMessage(FullStateMessage message)
        {
            if (!PlayerExists(message.SenderId))
            {
                CreatePlayer(message.SenderId, message.PlayerName);
            }
            else
            {
                GetPlayer(message.SenderId).Name = message.PlayerName;
            }
        }

        public static void SetReadiness(uint id, bool ready)
        {
            GetPlayer(id).Ready = ready;
        }

        public static void UpdateSector(uint id, Transform sector)
        {
            GetPlayer(id).ReferenceSector = sector;
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

        // Get player data :

        public static string GetPlayerName(uint id)
        {
            return GetPlayer(id).Name;
        }

        public static bool IsPlayerReady(uint id)
        {
            return GetPlayer(id).Ready;
        } 

        public static Transform GetSector(uint id)
        {
            return GetPlayer(id).ReferenceSector;
        }   

        public static bool GetState(uint id, State state)
        {
            var states = GetPlayer(id).State;
            return FlagsHelper.IsSet(states, state);
        }
    }
}
