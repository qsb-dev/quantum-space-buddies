using OWML.Common;
using QSB.AuthoritySync;
using QSB.Events;
using QSB.OrbSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync.Events;
using System.Linq;

namespace QSB.OrbSync.Events
{
	public class OrbUserEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<int, bool>.AddListener(EventNames.QSBOrbUser, Handler);
		public override void CloseListener() => GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBOrbUser, Handler);

		private void Handler(int id, bool isDragging) => SendEvent(CreateMessage(id, isDragging));

		private BoolWorldObjectMessage CreateMessage(int id, bool isDragging) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = id,
			State = isDragging
		};

		public override void OnReceiveLocal(bool isServer, BoolWorldObjectMessage message) => OnReceive(true, isServer, message);
		public override void OnReceiveRemote(bool isServer, BoolWorldObjectMessage message) => OnReceive(false, isServer, message);

		private static void OnReceive(bool isLocal, bool isServer, BoolWorldObjectMessage message)
		{
			if (NomaiOrbTransformSync.Instances.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			if (OrbManager.Orbs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			var orbSync = NomaiOrbTransformSync.Instances.Where(x => x != null)
				.FirstOrDefault(x => x.AttachedObject == OrbManager.Orbs[message.ObjectId].transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No orb found for user event. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			if (message.State)
			{
				if (isServer)
				{
					orbSync.NetIdentity.SetAuthority(message.FromId);
				}

				if (!isLocal && !orbSync.Orb._isBeingDragged)
				{
					orbSync.Orb._isBeingDragged = true;
					orbSync.Orb._interactibleCollider.enabled = false;
					if (orbSync.Orb._orbAudio != null)
					{
						orbSync.Orb._orbAudio.PlayStartDragClip();
					}
				}
			}
			else
			{
				if (!isLocal && orbSync.Orb._isBeingDragged)
				{
					orbSync.Orb._isBeingDragged = false;
					orbSync.Orb._interactibleCollider.enabled = true;
				}
			}
		}
	}
}
