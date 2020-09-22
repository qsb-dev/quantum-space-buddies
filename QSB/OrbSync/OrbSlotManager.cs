using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync
{
    public class OrbSlotManager : MonoBehaviour
    {
        private void Awake()
        {
            QSB.Helper.Events.Unity.RunWhen(() => QSB.HasWokenUp, InitSlots);
        }

        private void InitSlots()
        {
            var orbSlots = Resources.FindObjectsOfTypeAll<NomaiInterfaceSlot>();
            for (var id = 0; id < orbSlots.Length; id++)
            {
                var qsbOrbSlot = WorldRegistry.GetObject<QSBOrbSlot>(id) ?? new QSBOrbSlot();
                qsbOrbSlot.Init(orbSlots[id], id);
            }
        }
    }
}