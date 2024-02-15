using QSB.Messaging;
using QSB.Utility;
using System;

namespace QSB.WorldSync.Messages;

/// <summary>
/// sent to the host to get initial object states.
/// <para/>
/// world objects will be ready on both sides at this point
/// </summary>
public class RequestInitialStatesMessage : QSBMessage
{
	public RequestInitialStatesMessage() => To = 0;

	public override void OnReceiveRemote() =>
		Delay.RunWhen(() => QSBWorldSync.AllObjectsReady,
			() => SendInitialStates(From));

	private static void SendInitialStates(uint to)
	{
		SendInitialState?.SafeInvoke(to);
		DebugLog.DebugWrite($"sent initial states to {to}");
	}

	/// <summary>
	/// called on the host.
	/// use this to send initial states to whoever is asking for it.
	/// </summary>
	public static event Action<uint> SendInitialState;
}
