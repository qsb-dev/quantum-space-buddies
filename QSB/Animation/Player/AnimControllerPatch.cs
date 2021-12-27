using OWML.Utils;
using UnityEngine;

namespace QSB.Animation.Player
{
	public static class AnimControllerPatch
	{
		public static RuntimeAnimatorController SuitedAnimController { get; private set; }

		public static void Init()
		{
			QSBCore.Helper.Events.Subscribe<PlayerAnimController>(OWML.Common.Events.BeforeStart);
			QSBCore.Helper.Events.Event += OnEvent;
		}

		private static void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
		{
			if (behaviour is PlayerAnimController playerAnimController &&
				ev == OWML.Common.Events.BeforeStart &&
				SuitedAnimController == null)
			{
				SuitedAnimController = playerAnimController._baseAnimController;
			}
		}
	}
}