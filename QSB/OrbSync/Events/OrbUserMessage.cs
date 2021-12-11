using System.Linq;
using OWML.Common;
using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.OrbSync.TransformSync;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Events
{
	/// the world object message without the world object .-.
	public class OrbUserMessage : QSBMessage
	{
		public int OrbId;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OrbId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			OrbId = reader.ReadInt32();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;


		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				HandleServer(QSBPlayerManager.LocalPlayerId);
			}
			else
			{
				HandleClient();
			}
		}

		public override void OnReceiveRemote(uint from)
		{
			if (QSBCore.IsHost)
			{
				HandleServer(from);
			}
			else
			{
				HandleClient();
			}
		}

		private void HandleServer(uint from)
		{
			if (NomaiOrbTransformSync.OrbTransformSyncs == null || NomaiOrbTransformSync.OrbTransformSyncs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty or null. (ID {OrbId})", MessageType.Error);
				return;
			}

			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty or null. (ID {OrbId})", MessageType.Error);
				return;
			}

			var orbSync = NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null)
				.FirstOrDefault(x => x.AttachedObject == QSBWorldSync.OldOrbList[OrbId].transform);
			if (orbSync == null)
			{
				DebugLog.ToConsole($"Error - No orb found for user event. (ID {OrbId})", MessageType.Error);
				return;
			}

			orbSync.NetIdentity.SetAuthority(from);
			orbSync.enabled = true;
		}

		private void HandleClient()
		{
			if (NomaiOrbTransformSync.OrbTransformSyncs == null || NomaiOrbTransformSync.OrbTransformSyncs.Count == 0)
			{
				DebugLog.ToConsole($"Error - OrbTransformSyncs is empty or null. (ID {OrbId})", MessageType.Error);
				return;
			}

			if (QSBWorldSync.OldOrbList == null || QSBWorldSync.OldOrbList.Count == 0)
			{
				DebugLog.ToConsole($"Error - OldOrbList is empty or null. (ID {OrbId})", MessageType.Error);
				return;
			}

			if (!NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null).Any(x => x.AttachedObject == QSBWorldSync.OldOrbList[OrbId].transform))
			{
				DebugLog.ToConsole($"Error - No NomaiOrbTransformSync has AttachedOrb with objectId {OrbId}!");
				return;
			}

			var orb = NomaiOrbTransformSync.OrbTransformSyncs.Where(x => x != null)
				.First(x => x.AttachedObject == QSBWorldSync.OldOrbList[OrbId].transform);
			orb.enabled = true;
		}
	}
}
