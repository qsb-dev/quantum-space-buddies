using UnityEngine;

namespace QSB.Tools.TranslatorTool
{
	class QSBNomaiTranslatorProp : MonoBehaviour
	{
		private static MaterialPropertyBlock s_matPropBlock;
		private static int s_propID_EmissionColor;

		public GameObject TranslatorProp;
		public MeshRenderer _leftPageArrowRenderer;
		public MeshRenderer _rightPageArrowRenderer;
		public Color _baseEmissionColor;

		private TranslatorTargetBeam _targetBeam;
		private QSBTranslatorScanBeam[] _scanBeams;
		private bool _isTranslating;

		private void Awake()
		{
			_targetBeam = transform.GetComponentInChildren<TranslatorTargetBeam>();

			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
			}

			TurnOffArrowEmission();

			TranslatorProp.SetActive(false);
		}

		private void Start()
		{
			_scanBeams = transform.GetComponentsInChildren<QSBTranslatorScanBeam>();
			for (var i = 0; i < _scanBeams.Length; i++)
			{
				_scanBeams[i].enabled = false;
			}

			enabled = false;
		}

		private void TurnOffArrowEmission()
		{
			if (_leftPageArrowRenderer != null)
			{
				SetMaterialEmissionEnabled(_leftPageArrowRenderer, false);
			}

			if (_rightPageArrowRenderer != null)
			{
				SetMaterialEmissionEnabled(_rightPageArrowRenderer, false);
			}
		}

		private void SetMaterialEmissionEnabled(MeshRenderer emissiveRenderer, bool emissionEnabled)
		{
			if (emissionEnabled)
			{
				s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 1f);
				emissiveRenderer.SetPropertyBlock(s_matPropBlock);
				return;
			}

			s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 0f);
			emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}

		public void OnEquipTool()
		{
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
			enabled = false;
			StopTranslating();
			TurnOffArrowEmission();
		}

		public void OnFinishUnequipAnimation()
		{
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
			if (_isTranslating)
			{
				_isTranslating = false;
			}
		}
	}
}
