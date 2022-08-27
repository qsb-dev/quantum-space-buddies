using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Animation.Player;

[UsedInUnityProject]
public class PlayerHeadRotationSync : MonoBehaviour
{
	private Animator _attachedAnimator;
	private Transform _lookBase;
	private bool _isSetUp;

	public void Init(Transform lookBase)
	{
		_attachedAnimator = GetComponent<Animator>();
		_lookBase = lookBase;
		_isSetUp = true;
	}

	private void LateUpdate()
	{
		if (!_isSetUp)
		{
			return;
		}

		if (_attachedAnimator == null)
		{
			DebugLog.ToConsole($"Error - _attachedAnimator is null!", MessageType.Error);
			return;
		}

		if (_lookBase == null)
		{
			DebugLog.ToConsole($"Error - _lookBase is null!", MessageType.Error);
			return;
		}

		var bone = _attachedAnimator.GetBoneTransform(HumanBodyBones.Head);
		// Get the camera's local rotation with respect to the player body
		var lookLocalRotation = Quaternion.Inverse(_attachedAnimator.transform.rotation) * _lookBase.rotation;
		// no idea why this rotation is like this, but this is the only way it looks right
		bone.localRotation = Quaternion.Euler(-lookLocalRotation.eulerAngles.y, -lookLocalRotation.eulerAngles.z, lookLocalRotation.eulerAngles.x);
	}
}