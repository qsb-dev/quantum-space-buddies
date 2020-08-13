using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public static class WorldRegistry
    {
        public static List<GeyserController> GeyserControllers = new List<GeyserController>();

        public static void GenerateComponentList()
        {
            GeyserControllers = Resources.FindObjectsOfTypeAll<GeyserController>().ToList();

            foreach (var component in GeyserControllers)
            {
                if (NetworkServer.active)
                {
                    component.OnGeyserActivateEvent += () => GlobalMessenger<GeyserController, bool>.FireEvent(EventNames.QSBGeyserState, component, true);
                    component.OnGeyserDeactivateEvent += () => GlobalMessenger<GeyserController, bool>.FireEvent(EventNames.QSBGeyserState, component, false);
                }
            }
        }

        public static int GetObjectID(SyncObjects type, object component)
        {
            switch (type)
            {
                case SyncObjects.Geysers:
                    return GeyserControllers.FindIndex(x => x == component);
            }
            return 0;
        }
    }
}
