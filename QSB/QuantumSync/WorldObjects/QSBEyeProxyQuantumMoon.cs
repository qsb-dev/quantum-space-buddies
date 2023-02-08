namespace QSB.QuantumSync.WorldObjects;

internal class QSBEyeProxyQuantumMoon : QSBQuantumObject<EyeProxyQuantumMoon>
{
	public override bool HostControls => true;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		// todo SendInitialState
	}
}