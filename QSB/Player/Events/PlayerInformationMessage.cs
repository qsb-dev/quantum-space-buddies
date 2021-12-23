using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerInformationMessage : QSBMessage
	{
		private string PlayerName;
		private bool IsReady;
		private bool FlashlightActive;
		private bool SuitedUp;
		private bool ProbeLauncherEquipped;
		private bool SignalscopeEquipped;
		private bool TranslatorEquipped;
		private bool ProbeActive;
		private ClientState ClientState;

		public PlayerInformationMessage()
		{
			var player = QSBPlayerManager.LocalPlayer;
			PlayerName = player.Name;
			IsReady = player.IsReady;
			FlashlightActive = player.FlashlightActive;
			SuitedUp = player.SuitedUp;
			ProbeLauncherEquipped = player.ProbeLauncherEquipped;
			SignalscopeEquipped = player.SignalscopeEquipped;
			TranslatorEquipped = player.TranslatorEquipped;
			ProbeActive = player.ProbeActive;
			ClientState = player.State;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerName);
			writer.Write(IsReady);
			writer.Write(FlashlightActive);
			writer.Write(SuitedUp);
			writer.Write(ProbeLauncherEquipped);
			writer.Write(SignalscopeEquipped);
			writer.Write(TranslatorEquipped);
			writer.Write(ProbeActive);
			writer.Write((int)ClientState);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerName = reader.ReadString();
			IsReady = reader.ReadBoolean();
			FlashlightActive = reader.ReadBoolean();
			SuitedUp = reader.ReadBoolean();
			ProbeLauncherEquipped = reader.ReadBoolean();
			SignalscopeEquipped = reader.ReadBoolean();
			TranslatorEquipped = reader.ReadBoolean();
			ProbeActive = reader.ReadBoolean();
			ClientState = (ClientState)reader.ReadInt32();
		}

		public override void OnReceiveRemote()
		{
			RequestStateResyncEvent._waitingForEvent = false;
			if (QSBPlayerManager.PlayerExists(From))
			{
				var player = QSBPlayerManager.GetPlayer(From);
				player.Name = PlayerName;
				player.IsReady = IsReady;
				player.FlashlightActive = FlashlightActive;
				player.SuitedUp = SuitedUp;
				player.ProbeLauncherEquipped = ProbeLauncherEquipped;
				player.SignalscopeEquipped = SignalscopeEquipped;
				player.TranslatorEquipped = TranslatorEquipped;
				player.ProbeActive = ProbeActive;
				if (QSBPlayerManager.LocalPlayer.IsReady && player.IsReady)
				{
					player.UpdateObjectsFromStates();
				}

				player.State = ClientState;
			}
			else
			{
				DebugLog.ToConsole($"Warning - got player information message about player that doesnt exist!", MessageType.Warning);
			}
		}
	}
}
