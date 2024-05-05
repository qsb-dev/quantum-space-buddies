using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QSB.QuantumSync.WorldObjects;

public class QSBMultiStateQuantumObject : QSBQuantumObject<MultiStateQuantumObject>
{
	public List<QSBQuantumState> QuantumStates { get; private set; }
	public int CurrentState => AttachedObject._stateIndex;

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);

		QuantumStates = AttachedObject._states.Select(QSBWorldSync.GetWorldObject<QSBQuantumState>).ToList();

		if (QuantumStates.Any(x => x == null))
		{
			DebugLog.ToConsole($"Error - {AttachedObject.name} has one or more null QSBQuantumStates assigned!", OWML.Common.MessageType.Error);
		}
	}

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}StateIndex:{AttachedObject._stateIndex}";

	public void ChangeState(int newStateIndex)
	{
		if (CurrentState != -1)
		{
			QuantumStates[CurrentState].SetVisible(false);
		}

		QuantumStates[newStateIndex].SetVisible(true);
		AttachedObject._stateIndex = newStateIndex;
	}
}