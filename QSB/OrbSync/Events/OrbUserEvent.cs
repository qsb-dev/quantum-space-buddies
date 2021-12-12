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
	public class OrbUserEvent : QSBEvent<WorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<int>.AddListener(EventNames.QSBOrbUser, Handler);
		public override void CloseListener() => GlobalMessenger<int>.RemoveListener(EventNames.QSBOrbUser, Handler);

		private void Handler(int id) => SendEvent(CreateMessage(id));

		private WorldObjectMessage CreateMessage(int id) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = id
		};

		public override void OnReceiveLocal(bool server, WorldObjectMessage message)
		{
			if (server)
			{
				HandleServer(message);
			}
			else
			{
				HandleClient(message);
			}
		}

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			if (server)
			{
				HandleServer(message);
			}
			else
			{
				HandleClient(message);
			}
		}

		private static void HandleServer(WorldObjectMessage message)
		{
			if (NomaiOrbTransformSync.OrbTransformSyncs == null || NomaiOrbTransformSync.OrbTransformSyncs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty or null. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty or null. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			var orbSync = NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null)
				.FirstOrDefault(x => x.AttachedObject == QSBWorldSync.OldOrbList[message.ObjectId].transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No orb found for user event. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			orbSync.NetIdentity.SetAuthority(message.FromId);
			orbSync.enabled = true;
		}

		private static void HandleClient(WorldObjectMessage message)
		{
			if (NomaiOrbTransformSync.OrbTransformSyncs == null || NomaiOrbTransformSync.OrbTransformSyncs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty or null. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty or null. (ID {message.ObjectId})", MessageType.Error);
				return;
			}

			if (!NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null).Any(x => x.AttachedObject == QSBWorldSync.OldOrbList[message.ObjectId].transform))
			{
				DebugLog.ToConsole($"Error - No NomaiOrbTransformSync has AttachedOrb with objectId {message.ObjectId}!");
				return;
			}

			var orb = NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null)
				.First(x => x.AttachedObject == QSBWorldSync.OldOrbList[message.ObjectId].transform);
			orb.enabled = true;
		}
	}
}
