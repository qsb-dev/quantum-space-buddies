using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace QSB.Utility.VariableSync;

public class WorldObjectVariableSyncer<T> : BaseVariableSyncer<T>, IWorldObjectVariableSyncer
{
	private static List<BaseVariableSyncer<T>> _instances = new();

	public IWorldObject AttachedWorldObject { get; private set; }

	public static List<U> GetSpecificSyncers<U>()
		=> _instances.OfType<U>().ToList();

	public void Init(IWorldObject worldObject)
	{
		AttachedWorldObject = worldObject;
	}

	public override void OnStartClient()
	{
		_instances.Add(this);
		DontDestroyOnLoad(this);
		base.OnStartClient();
	}

	public override void OnStopClient()
	{
		_instances.Remove(this);
		// DontDestroyOnLoad simply moves a GO to a scene called "DontDestroyOnLoad"
		// so to undo it we can just move it back
		SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
		base.OnStopClient();
	}
}
