using Mirror;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using UnityEngine;

namespace QSB.QuantumSync.Messages;

public class EyeProxyMoonStateChangeMessage : QSBWorldObjectMessage<QSBEyeProxyQuantumMoon>
{
	private bool Active;
	private float Angle;

	public EyeProxyMoonStateChangeMessage(bool active, float angle)
	{
		Active = active;
		Angle = angle;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(Active);
		writer.Write(Angle);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		Active = reader.Read<bool>();
		Angle = reader.Read<float>();
	}

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._moonStateRoot.SetActive(Active);
		if (Angle != -1f)
		{
			WorldObject.AttachedObject.transform.localEulerAngles = new Vector3(0f, Angle, 0f);
		}
	}
}