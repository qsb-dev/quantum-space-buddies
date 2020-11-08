using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB.Animation
{
    public static class AnimControllerPatch
    {
        public static RuntimeAnimatorController SuitedAnimController { get; private set; }

        public static void Init()
        {
            QSB.Helper.Events.Subscribe<PlayerAnimController>(OWML.Common.Events.BeforeStart);
            QSB.Helper.Events.Event += OnEvent;
        }

        private static void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour is PlayerAnimController playerAnimController &&
                ev == OWML.Common.Events.BeforeStart &&
                SuitedAnimController == null)
            {
                SuitedAnimController = playerAnimController.GetValue<RuntimeAnimatorController>("_baseAnimController");
            }
        }
    }
}