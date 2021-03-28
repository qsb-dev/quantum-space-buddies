namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBEyeProxyQuantumMoon : QSBQuantumObject<EyeProxyQuantumMoon>
	{
		public override void Init(EyeProxyQuantumMoon moonObject, int id)
		{
			ObjectId = id;
			AttachedObject = moonObject;
			ControllingPlayer = 1u;
			base.Init(moonObject, id);
		}
	}
}
