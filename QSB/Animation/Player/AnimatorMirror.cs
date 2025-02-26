using Mirror;
using OWML.Common;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.Player;

public class AnimatorMirror : MonoBehaviour
{
	private const float SmoothTime = 0.05f;

	private Animator _from;
	private Animator _to;
	private NetworkAnimator _networkAnimator;

	private readonly Dictionary<int, AnimFloatParam> _floatParams = new();

	/// <summary>
	/// Initializes the Animator Mirror
	/// </summary>
	/// <param name="from">The Animator to take the values from.</param>
	/// <param name="to">The Animator to set the values on to.</param>
	/// <param name="netAnimator">The NetworkAnimator to sync triggers through. Set only if you have auth over "<paramref name="from"/>", otherwise set to null.</param>
	public void Init(Animator from, Animator to, NetworkAnimator netAnimator)
	{
		if (from == null)
		{
			DebugLog.ToConsole($"Error - Trying to init AnimatorMirror with null \"from\".", MessageType.Error);
		}

		if (to == null)
		{
			DebugLog.ToConsole($"Error - Trying to init AnimatorMirror with null \"to\".", MessageType.Error);
		}

		if (to == null || from == null)
		{
			// Doing the return this way so you can see if one or both are null
			return;
		}

		_from = from;
		_to = to;
		if (_from.runtimeAnimatorController == null)
		{
			_from.runtimeAnimatorController = _to.runtimeAnimatorController;
		}
		else if (_to.runtimeAnimatorController == null)
		{
			_to.runtimeAnimatorController = _from.runtimeAnimatorController;
		}

		_networkAnimator = netAnimator;

		RebuildFloatParams();
	}

	public void Update()
	{
		if (_to == null || _from == null)
		{
			return;
		}

		if (_to.runtimeAnimatorController != _from.runtimeAnimatorController)
		{
			_to.runtimeAnimatorController = _from.runtimeAnimatorController;
			RebuildFloatParams();
		}

		SyncParams();
		SyncLayerWeights();
		SmoothFloats();
	}

	private void SyncParams()
	{
		foreach (var fromParam in _from.parameters)
		{
			switch (fromParam.type)
			{
				case AnimatorControllerParameterType.Float:
					_floatParams[fromParam.nameHash].Target = _from.GetFloat(fromParam.nameHash);
					break;

				case AnimatorControllerParameterType.Int:
					_to.SetInteger(fromParam.nameHash, _from.GetInteger(fromParam.nameHash));
					break;

				case AnimatorControllerParameterType.Bool:
					_to.SetBool(fromParam.nameHash, _from.GetBool(fromParam.nameHash));
					break;

				case AnimatorControllerParameterType.Trigger:
					if (_from.GetBool(fromParam.nameHash) && !_to.GetBool(fromParam.nameHash))
					{
						if (_networkAnimator != null)
						{
							// DebugLog.DebugWrite($"Set {fromParam.name} on netanim");
							_networkAnimator.SetTrigger(fromParam.nameHash);
						}

						_to.SetTrigger(fromParam.nameHash);
					}

					if (!_from.GetBool(fromParam.nameHash) && _to.GetBool(fromParam.nameHash))
					{
						if (_networkAnimator != null)
						{
							// DebugLog.DebugWrite($"Reset {fromParam.name} on netanim");
							_networkAnimator.ResetTrigger(fromParam.nameHash);
						}

						_to.ResetTrigger(fromParam.nameHash);
					}

					break;
			}
		}
	}

	private void SyncLayerWeights()
	{
		for (var i = 0; i < _from.layerCount; i++)
		{
			var weight = _from.GetLayerWeight(i);
			_to.SetLayerWeight(i, weight);
		}
	}

	private void SmoothFloats()
	{
		foreach (var floatParam in _floatParams)
		{
			var current = floatParam.Value.Smooth(SmoothTime);
			_to.SetFloat(floatParam.Key, current);
		}
	}

	public void RebuildFloatParams()
	{
		_floatParams.Clear();
		foreach (var param in _from.parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
		{
			_floatParams.Add(param.nameHash, new AnimFloatParam());
		}
	}
}
