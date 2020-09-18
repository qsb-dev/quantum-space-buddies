using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ConversationSync
{
    public static class ConversationPatches
    {
        public static class OrbPatches
        {
            public static void ZoneExit(CharacterAnimController __instance)
            {

            }

            public static void AddPatches()
            {
                QSB.Helper.HarmonyHelper.AddPostfix<NomaiInterfaceOrb>("StartDragFromPosition", typeof(OrbPatches), nameof(StartDragCallEvent));
                QSB.Helper.HarmonyHelper.AddPrefix<NomaiInterfaceSlot>("CheckOrbCollision", typeof(OrbPatches), nameof(CheckOrbCollision));
            }
        }
    }
}
