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

	private readonly Dictionary<string, AnimFloatParam> _floatParams = new();

	public void Init(Animator from, Animator to)
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
		SmoothFloats();
	}

	private void SyncParams()
	{
		foreach (var fromParam in _from.parameters)
		{
			switch (fromParam.type)
			{
				case AnimatorControllerParameterType.Float:
					_floatParams[fromParam.name].Target = _from.GetFloat(fromParam.name);
					break;

				case AnimatorControllerParameterType.Bool:
					_to.SetBool(fromParam.name, _from.GetBool(fromParam.name));
					break;

				case AnimatorControllerParameterType.Int:
					_to.SetInteger(fromParam.name, _from.GetInteger(fromParam.name));
					break;

				case AnimatorControllerParameterType.Trigger:
					if (_from.GetBool(fromParam.name) && !_to.GetBool(fromParam.name))
					{
						_to.SetTrigger(fromParam.name);
					}
					if (!_from.GetBool(fromParam.name) && _to.GetBool(fromParam.name))
					{
						_to.ResetTrigger(fromParam.name);
					}
					break;
			}
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
			_floatParams.Add(param.name, new AnimFloatParam());
		}
	}
}
