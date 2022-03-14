using QSB.WorldSync;
using UnityEngine.SceneManagement;

namespace QSB.Utility.VariableSync;

public class WorldObjectVariableSyncer<TSyncType, TWorldObjectType> : BaseVariableSyncer<TSyncType>, IWorldObjectVariableSyncer
	where TWorldObjectType : IWorldObject
{
	public TWorldObjectType AttachedWorldObject { get; private set; }

	public void Init(IWorldObject worldObject)
		=> AttachedWorldObject = (TWorldObjectType)worldObject;

	public override void OnStartClient()
	{
		VariableSyncStorage._instances.Add(this);
		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		VariableSyncStorage._instances.Remove(this);
	}
}
