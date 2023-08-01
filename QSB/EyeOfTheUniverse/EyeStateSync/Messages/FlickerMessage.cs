using Mirror;
using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Messages;

public class FlickerMessage : QSBMessage
{
	public static bool IgnoreNextMessage;

	static FlickerMessage() => GlobalMessenger<float, float>.AddListener(OWEvents.FlickerOffAndOn, Handler);

	private static void Handler(float offDuration, float onDuration)
	{
		if (IgnoreNextMessage)
		{
			IgnoreNextMessage = false;
			return;
		}

		if (PlayerTransformSync.LocalInstance)
		{
			new FlickerMessage(offDuration, onDuration).Send();
		}
	}

	private float _offDuration;
	private float _onDuration;

	private FlickerMessage(float offDuration, float onDuration)
	{
		_offDuration = offDuration;
		_onDuration = onDuration;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(_offDuration);
		writer.Write(_onDuration);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_offDuration = reader.Read<float>();
		_onDuration = reader.Read<float>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		// manually fire callbacks
		var eventTable = GlobalMessenger<float, float>.eventTable;
		lock (eventTable)
		{
			var eventData = eventTable[OWEvents.FlickerOffAndOn];
			if (eventData.isInvoking)
			{
				throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
			}

			eventData.isInvoking = true;
			eventData.temp.AddRange(eventData.callbacks);
			foreach (var callback in eventData.temp)
			{
				// ignore callback for this message to prevent infinite loop
				if (callback == Handler)
				{
					continue;
				}

				try
				{
					callback(_offDuration, _onDuration);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}

			eventData.temp.Clear();
			eventData.isInvoking = false;
		}
	}
}