using System;
using OWML.Common;
using QSB.AuthoritySync;
using QSB.Events;
using QSB.OrbSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
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

		public override void OnReceiveLocal(bool isServer, BoolWorldObjectMessage message)
		{
			var orbSync = GetOrbSync(message);
			if (orbSync == null)
			{
				return;
			}

			if (message.State)
			{
				if (isServer)
				{
					orbSync.NetIdentity.SetAuthority(message.FromId);
				}
				orbSync.enabled = true;

				orbSync.OtherDragging = false;
			}
		}

		public override void OnReceiveRemote(bool isServer, BoolWorldObjectMessage message)
		{
			var orbSync = GetOrbSync(message);
			if (orbSync == null)
			{
				return;
			}

			if (message.State)
			{
				if (isServer)
				{
					orbSync.NetIdentity.SetAuthority(message.FromId);
				}
				orbSync.enabled = true;
			}

			orbSync.OtherDragging = message.State;
		}

		private static NomaiOrbTransformSync GetOrbSync(WorldObjectMessage message)
		{
			if (NomaiOrbTransformSync.Instances.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty. (ID {message.ObjectId})", MessageType.Error);
				return null;
			}

			if (OrbManager.Orbs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty. (ID {message.ObjectId})", MessageType.Error);
				return null;
			}

			var orbSync = NomaiOrbTransformSync.Instances.Where(x => x != null)
				.FirstOrDefault(x => x.AttachedObject == OrbManager.Orbs[message.ObjectId].transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No orb found for user event. (ID {message.ObjectId})", MessageType.Error);
			}
			return orbSync;
		}
	}
}
