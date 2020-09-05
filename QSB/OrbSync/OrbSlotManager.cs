using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync
{
    public class OrbSlotManager : MonoBehaviour
    {
        public static OrbSlotManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            var orbSlots = Resources.FindObjectsOfTypeAll<NomaiInterfaceSlot>();
            for (var id = 0; id < orbSlots.Length; id++)
            {
                var qsbOrbSlot = WorldRegistry.GetObject<QSBOrbSlot>(id) ?? new QSBOrbSlot();
                qsbOrbSlot.Init(orbSlots[id], id);
                WorldRegistry.AddObject(qsbOrbSlot);
            }
        }
    }
}