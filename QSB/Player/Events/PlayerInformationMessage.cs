using QSB.ClientServerStateSync;
using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerInformationMessage : PlayerMessage
	{
		public string PlayerName { get; set; }
		public bool IsReady { get; set; }
		public bool FlashlightActive { get; set; }
		public bool SuitedUp { get; set; }
		public bool ProbeLauncherEquipped { get; set; }
		public bool SignalscopeEquipped { get; set; }
		public bool TranslatorEquipped { get; set; }
		public bool ProbeActive { get; set; }
		public ClientState ClientState { get; set; }

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
	}
}
