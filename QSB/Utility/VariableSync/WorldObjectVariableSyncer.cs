using QSB.WorldSync;
using UnityEngine.SceneManagement;

namespace QSB.Utility.VariableSync;

public class WorldObjectVariableSyncer<T> : BaseVariableSyncer<T>, IWorldObjectVariableSyncer
{
	public IWorldObject AttachedWorldObject { get; private set; }

	public void Init(IWorldObject worldObject)
		=> AttachedWorldObject = worldObject;

	public override void OnStartClient()
	{
		VariableSyncStorage._instances.Add(this);
		DontDestroyOnLoad(this);
		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		VariableSyncStorage._instances.Remove(this);
		// DontDestroyOnLoad moves GOs to the scene "DontDestroyOnLoad"
		// so to undo it we can just move them back
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
		base.OnStopClient();
	}
}
