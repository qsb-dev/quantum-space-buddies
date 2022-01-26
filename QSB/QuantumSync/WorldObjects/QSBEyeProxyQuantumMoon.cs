namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBEyeProxyQuantumMoon : QSBQuantumObject<EyeProxyQuantumMoon>
	{
		protected override bool HostControls => true;

		public override void SendResyncInfo(uint to)
		{
			base.SendResyncInfo(to);

			// todo SendResyncInfo
		}
	}
}
