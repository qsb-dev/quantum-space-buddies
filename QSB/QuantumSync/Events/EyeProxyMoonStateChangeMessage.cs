using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	internal class EyeProxyMoonStateChangeMessage : QSBWorldObjectMessage<QSBEyeProxyQuantumMoon>
	{
		private bool Active;
		private float Angle;

		public EyeProxyMoonStateChangeMessage(QSBEyeProxyQuantumMoon moon, bool active, float angle)
		{
			ObjectId = moon.ObjectId;
			Active = active;
			Angle = angle;
		}

		public EyeProxyMoonStateChangeMessage() { }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Active = reader.ReadBoolean();
			Angle = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Active);
			writer.Write(Angle);
		}

		public override void OnReceiveRemote()
		{
			DebugLog.DebugWrite($"Get moon state active:{Active} angle:{Angle}");
			var moon = QSBWorldSync.GetWorldFromId<QSBEyeProxyQuantumMoon>(ObjectId);
			moon.AttachedObject._moonStateRoot.SetActive(Active);
			if (Angle != -1f)
			{
				moon.AttachedObject.transform.localEulerAngles = new Vector3(0f, Angle, 0f);
			}
		}
	}
}
