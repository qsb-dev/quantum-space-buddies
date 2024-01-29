using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;
public class PartyPathResetMessage : QSBWorldObjectMessage<QSBGhostBrain, (int indexOne, int indexTwo, int proxyIndex)>
{
	public PartyPathResetMessage(int indexOne, int indexTwo, int proxyIndex) : base((indexOne, indexTwo, proxyIndex)) { }

	public override void OnReceiveRemote()
	{
		var __instance = QSBWorldSync.GetUnityObject<GhostPartyPathDirector>();
		var partyPathAction = (QSBPartyPathAction)WorldObject.GetCurrentAction();
		WorldObject.AttachedObject.transform.position = __instance._ghostSpawns[Data.indexOne].spawnTransform.position;
		WorldObject.AttachedObject.transform.eulerAngles = Vector3.up * __instance._ghostSpawns[Data.indexTwo].spawnTransform.eulerAngles.y;
		WorldObject.TabulaRasa();
		partyPathAction.ResetPath();

		__instance._numEnabledGhostProxies = Data.proxyIndex;

		if (!__instance._disableGhostProxies && __instance._numEnabledGhostProxies < __instance._ghostFinalDestinations.Length)
		{
			if (__instance._ghostFinalDestinations[__instance._numEnabledGhostProxies].proxyGhost != null)
			{
				__instance._ghostFinalDestinations[__instance._numEnabledGhostProxies].proxyGhost.Reveal();
			}

			__instance._numEnabledGhostProxies++;
		}
	}
}
