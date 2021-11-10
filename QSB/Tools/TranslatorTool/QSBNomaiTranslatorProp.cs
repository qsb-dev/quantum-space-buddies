using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.Tools.TranslatorTool
{
	class QSBNomaiTranslatorProp : MonoBehaviour
	{
		public GameObject TranslatorProp;

		private TranslatorTargetBeam _targetBeam;
		private TranslatorScanBeam[] _scanBeams;
		private bool _isTranslating;

		private void Awake()
		{
			DebugLog.DebugWrite($"Awake.");
			_targetBeam = transform.GetComponentInChildren<TranslatorTargetBeam>();
			_scanBeams = transform.GetComponentsInChildren<TranslatorScanBeam>();
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].enabled = false;
			}
			TranslatorProp.SetActive(false);
		}

		private void Start()
		{
			DebugLog.DebugWrite($"Start.");
			enabled = false;
		}

		public void OnEquipTool()
		{
			DebugLog.DebugWrite($"OnEquipTool.");
			enabled = true;
			if (_targetBeam)
			{
				_targetBeam.Activate();
			}
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].enabled = true;
			}
			TranslatorProp.SetActive(true);
		}

		public void OnUnequipTool()
		{
			DebugLog.DebugWrite($"On unequip tool.");
			enabled = false;
			StopTranslating();
		}

		public void OnFinishUnequipAnimation()
		{
			DebugLog.DebugWrite($"On finish unequip animation.");

			if (_targetBeam)
			{
				_targetBeam.Deactivate();
			}

			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].enabled = false;
			}
			TranslatorProp.SetActive(false);
		}

		public void SetTooCloseToTarget(bool value)
		{
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetTooCloseToTarget(value);
			}
		}

		public void SetNomaiTextLine(NomaiTextLine line)
		{
			DebugLog.DebugWrite($"Set Nomai Text Line.");

			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiTextLine(line);
				_scanBeams[i].SetNomaiComputerRing(null);
				_scanBeams[i].SetNomaiVesselComputerRing(null);
			}
		}

		public void ClearNomaiTextLine()
		{
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiTextLine(null);
			}
		}

		public void SetNomaiComputerRing(NomaiComputerRing ring)
		{
			DebugLog.DebugWrite($"Set nomai computer ring.");
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiTextLine(null);
				_scanBeams[i].SetNomaiComputerRing(ring);
				_scanBeams[i].SetNomaiVesselComputerRing(null);
			}
		}

		public void ClearNomaiComputerRing()
		{
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiComputerRing(null);
			}
		}

		public void SetNomaiVesselComputerRing(NomaiVesselComputerRing ring)
		{
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiTextLine(null);
				_scanBeams[i].SetNomaiComputerRing(null);
				_scanBeams[i].SetNomaiVesselComputerRing(ring);
			}
		}

		public void ClearNomaiVesselComputerRing()
		{
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].SetNomaiVesselComputerRing(null);
			}
		}

		private void StopTranslating()
		{
			DebugLog.DebugWrite($"Stop Translating.");
			if (_isTranslating)
			{
				_isTranslating = false;
			}
		}
	}
}
