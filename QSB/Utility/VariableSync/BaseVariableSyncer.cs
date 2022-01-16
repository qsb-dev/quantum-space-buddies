namespace QSB.Utility.VariableSync
{
	public abstract class BaseVariableSyncer : QSBNetworkBehaviour
	{
		protected override float SendInterval => 0.1f;
	}
}
