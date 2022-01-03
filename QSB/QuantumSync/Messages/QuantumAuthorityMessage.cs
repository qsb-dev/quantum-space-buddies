using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Messages
{
	public class QuantumAuthorityMessage : QSBWorldObjectMessage<IQSBQuantumObject>
	{
		private uint ControllingPlayer;
		/// <summary>
		/// if true, force sets controlling player,
		/// without checking current controlling player
		/// or checking for other potential controllers
		/// </summary>
		private bool Force;

		public QuantumAuthorityMessage(uint controllingPlayer, bool force)
		{
			ControllingPlayer = controllingPlayer;
			Force = force;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ControllingPlayer);
			writer.Write(Force);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ControllingPlayer = reader.ReadUInt32();
			Force = reader.ReadBoolean();
		}

		public override bool ShouldReceive
		{
			get
			{
				if (!base.ShouldReceive)
				{
					return false;
				}

				if (WorldObject.ControllingPlayer == ControllingPlayer)
				{
					return false;
				}

				if (Force)
				{
					return true;
				}

				if (ControllingPlayer == uint.MaxValue)
				{
					return true;
				}

				return WorldObject.ControllingPlayer == uint.MaxValue;
			}
		}

		public override void OnReceiveLocal() => WorldObject.ControllingPlayer = ControllingPlayer;

		public override void OnReceiveRemote()
		{
			WorldObject.ControllingPlayer = ControllingPlayer;
			if (!Force && ControllingPlayer == uint.MaxValue && WorldObject.IsEnabled)
			{
				// object has no owner, but is still active for this player. request ownership
				WorldObject.SendMessage(new QuantumAuthorityMessage(QSBPlayerManager.LocalPlayerId, false));
			}
		}
	}
}
