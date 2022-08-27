using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.TranslatorTool;

[UsedInUnityProject]
internal class QSBTranslatorScanBeam : MonoBehaviour
{
	public Renderer _projectorRenderer;
	public Renderer _lightVolumeRenderer;
	private const float _focusedBeamWidth = 0.25f;
	private const float _maxBeamWidth = 1f;
	private const float _maxBeamLength = 10f;
	private const float _scanOffset = 0f;
	private const float _switchLength = 0.33f;
	private const float _fadeLength = 0.66f;

	public float _scanSpeed = 1f;
	public readonly Color _baseProjectorColor = new(0.3545942f, 2.206932f, 4.594794f, 1f);
	public readonly Color _baseLightColor = new(0.1301365f, 0.2158605f, 0.6239606f, 1f);

	private  Quaternion _baseRotation;
	private bool _tooCloseToTarget;
	private NomaiTextLine _nomaiTextLine;
	private NomaiComputerRing _nomaiComputerRing;
	private NomaiVesselComputerRing _nomaiVesselComputerRing;
	private float _scanTime;
	private float _switchTime;
	private Quaternion _prevRotation;
	private Vector3 _prevScale;
	private float _fade;

	private void Awake()
	{
		_tooCloseToTarget = false;
		_baseRotation = transform.localRotation;
		_prevRotation = Quaternion.identity;
		_prevScale = new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength);
		_fade = 0f;

		if (_projectorRenderer != null)
		{
			_projectorRenderer.material.SetAlpha(0f);
			_projectorRenderer.enabled = false;
		}

		if (_lightVolumeRenderer != null)
		{
			_lightVolumeRenderer.material.SetAlpha(0f);
			_lightVolumeRenderer.enabled = false;
		}
	}

	private void OnDisable()
	{
		_tooCloseToTarget = false;
		_nomaiTextLine = null;
		_prevRotation = Quaternion.identity;
		_prevScale = new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength);
		_fade = 0f;
		if (_projectorRenderer != null)
		{
			_projectorRenderer.material.SetAlpha(0f);
			_projectorRenderer.enabled = false;
		}

		if (_lightVolumeRenderer != null)
		{
			_lightVolumeRenderer.material.SetAlpha(0f);
			_lightVolumeRenderer.enabled = false;
		}

		transform.localRotation = _baseRotation;
		transform.localScale = _prevScale;
	}

	public bool IsSwitching()
		=> _switchTime < 1f;

	private void OnRenderObject()
	{
		if (!QSBCore.DebugSettings.DrawLines || !QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		Popcron.Gizmos.Line(transform.position, transform.position + transform.forward);
	}

	private void Update()
	{
		if (_nomaiTextLine != null && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			var smoothedSwitchTime = Mathf.SmoothStep(0f, 1f, _switchTime);
			_scanTime += Time.deltaTime * _scanSpeed;
			var num = (Mathf.Cos(_scanTime + _scanOffset) * 0.5f) + 0.5f;
			var pointAlongLine = _nomaiTextLine.GetPointAlongLine(num);
			var rhs = _nomaiTextLine.GetPointAlongLine(num + 0.1f) - _nomaiTextLine.GetPointAlongLine(num - 0.1f);
			var vector = pointAlongLine - transform.position;
			var upwards = Vector3.Cross(vector, rhs);
			var distance = Vector3.Distance(transform.position, pointAlongLine);
			var q = Quaternion.LookRotation(vector, upwards);
			var scanningRotation = transform.parent.InverseTransformRotation(q);
			transform.localRotation = Quaternion.Lerp(_prevRotation, scanningRotation, smoothedSwitchTime);
			transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + distance), smoothedSwitchTime);
		}
		else if (_nomaiComputerRing != null && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			var t2 = Mathf.SmoothStep(0f, 1f, _switchTime);
			_scanTime += Time.deltaTime * _scanSpeed;
			var t3 = (Mathf.Cos(_scanTime + _scanOffset) * 0.5f) + 0.5f;
			t3 = Mathf.Lerp(0.25f, 0.75f, t3);
			var pointOnRing = _nomaiComputerRing.GetPointOnRing(t3, transform.position);
			var forward = pointOnRing - transform.position;
			var up = _nomaiComputerRing.transform.up;
			var num3 = Vector3.Distance(transform.position, pointOnRing);
			var q2 = Quaternion.LookRotation(forward, up);
			var b2 = transform.parent.InverseTransformRotation(q2);
			transform.localRotation = Quaternion.Lerp(_prevRotation, b2, t2);
			transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + num3), t2);
		}
		else if (_nomaiVesselComputerRing != null && !_tooCloseToTarget)
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _switchLength);
			var smoothedSwitchTime = Mathf.SmoothStep(0f, 1f, _switchTime);

			_scanTime += Time.deltaTime * _scanSpeed;
			var t5 = (Mathf.Cos(_scanTime + _scanOffset) * 0.5f) + 0.5f;
			t5 = Mathf.Lerp(0.25f, 0.75f, t5);
			var pointOnRing2 = _nomaiVesselComputerRing.GetPointOnRing(t5, transform.position);
			var forward2 = pointOnRing2 - transform.position;
			var up2 = _nomaiVesselComputerRing.transform.up;
			var num4 = Vector3.Distance(transform.position, pointOnRing2);
			var q3 = Quaternion.LookRotation(forward2, up2);
			var b3 = transform.parent.InverseTransformRotation(q3);
			transform.localRotation = Quaternion.Lerp(_prevRotation, b3, smoothedSwitchTime);
			transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_focusedBeamWidth, _focusedBeamWidth, 1f + num4), smoothedSwitchTime);
		}
		else
		{
			_switchTime = Mathf.MoveTowards(_switchTime, 1f, Time.deltaTime / _fadeLength);
			var t6 = Mathf.SmoothStep(0f, 1f, _switchTime * (2f - _switchTime));
			transform.localRotation = Quaternion.Lerp(_prevRotation, _baseRotation, _switchTime);
			transform.localScale = Vector3.Lerp(_prevScale, new Vector3(_maxBeamWidth, _maxBeamWidth, _maxBeamLength), t6);
		}

		var flag = !_tooCloseToTarget && (_nomaiTextLine != null || _nomaiComputerRing != null || _nomaiVesselComputerRing != null);

		_fade = Mathf.MoveTowards(
			_fade,
			flag
				? 1f
				: 0f,
			Time.deltaTime / _fadeLength * (_tooCloseToTarget
				? 3f
				: 1f));

		if (_projectorRenderer != null)
		{
			var shouldBeOn = _fade > 0f;
			if (_projectorRenderer.enabled != shouldBeOn)
			{
				_projectorRenderer.enabled = shouldBeOn;
			}

			if (_projectorRenderer.enabled)
			{
				_projectorRenderer.material.SetAlpha(_fade * _fade * _baseProjectorColor.a);
			}
		}

		if (_lightVolumeRenderer != null)
		{
			var shouldBeOn = _fade > 0f;
			if (_lightVolumeRenderer.enabled != shouldBeOn)
			{
				_lightVolumeRenderer.enabled = shouldBeOn;
			}

			if (_lightVolumeRenderer.enabled)
			{
				_lightVolumeRenderer.material.SetAlpha(_fade * _fade * _baseLightColor.a);
			}
		}
	}

	public void SetTooCloseToTarget(bool tooClose)
	{
		if (_tooCloseToTarget != tooClose)
		{
			_tooCloseToTarget = tooClose;
			_switchTime = 0f;
			_prevRotation = transform.localRotation;
			_prevScale = transform.localScale;
		}
	}

	public void SetNomaiTextLine(NomaiTextLine line)
	{
		if (_nomaiTextLine != line)
		{
			_switchTime = 0f;
			_nomaiTextLine = line;
			_prevRotation = transform.localRotation;
			_prevScale = transform.localScale;
		}
	}

	public void SetNomaiComputerRing(NomaiComputerRing ring)
	{
		if (_nomaiComputerRing != ring)
		{
			_switchTime = 0f;
			_nomaiComputerRing = ring;
			_prevRotation = transform.localRotation;
			_prevScale = transform.localScale;
		}
	}

	public void SetNomaiVesselComputerRing(NomaiVesselComputerRing ring)
	{
		if (_nomaiVesselComputerRing != ring)
		{
			_switchTime = 0f;
			_nomaiVesselComputerRing = ring;
			_prevRotation = transform.localRotation;
			_prevScale = transform.localScale;
		}
	}
}