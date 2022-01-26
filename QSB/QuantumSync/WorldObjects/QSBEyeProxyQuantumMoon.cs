namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBEyeProxyQuantumMoon : QSBQuantumObject<EyeProxyQuantumMoon>
	{
		protected override bool HostControls => true;

		public override void SendInitialState(uint to)
		{
			base.SendInitialState(to);

			// todo SendResyncInfo
		}
	}
}
