using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Player;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers;

public class CodeControllerRemoteUpdater : MonoBehaviour
{
	private QSBEclipseCodeController _attachedWorldObject;
	private EclipseCodeController4 _codeController => _attachedWorldObject.AttachedObject;

	private void Start()
	{
		_attachedWorldObject = GetComponent<EclipseCodeController4>().GetWorldObject<QSBEclipseCodeController>();
	}

	private void Update()
	{
		if (_attachedWorldObject.PlayerInControl == QSBPlayerManager.LocalPlayer
			|| (_attachedWorldObject.PlayerInControl == null && !_codeController._movingSelector))
		{
			return;
		}

		if (_codeController._movingSelector)
		{
			_codeController._currentSelectorPosY = Mathf.MoveTowards(_codeController._currentSelectorPosY, _codeController._targetSelectorPosY, Time.deltaTime * 1.5f);
			if (OWMath.ApproxEquals(_codeController._currentSelectorPosY, _codeController._targetSelectorPosY, 0.001f))
			{
				_codeController._currentSelectorPosY = _codeController._targetSelectorPosY;
				_codeController._movingSelector = false;
			}

			for (var i = 0; i < _codeController._selectors.Length; i++)
			{
				_codeController._selectors[i].SetLocalPositionY(_codeController._currentSelectorPosY);
			}
		}

		if (_codeController._codeCheckDirty)
		{
			_codeController.CheckForCode();
			_codeController._codeCheckDirty = false;
		}
	}
}
