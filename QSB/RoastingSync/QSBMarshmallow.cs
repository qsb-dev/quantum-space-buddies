using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.RoastingSync;

[UsedInUnityProject]
public class QSBMarshmallow : MonoBehaviour
{
	public const float RAW_TOASTED_FRACTION = 0.2f;
	public const float PERFECT_TOASTED_FRACTION = 0.7f;
	public const float HEAT_CAPACITY = 300f;
	public const float BURN_THRESHOLD = 25f;
	public const float BURN_DURATION = 5f;
	public MeshRenderer _fireRenderer;
	public ParticleSystem _smokeParticles;
	public MeshRenderer _mallowRenderer;
	public Color _rawColor;
	public Color _toastedColor;
	public Color _burntColor;
	private Marshmallow.MallowState _mallowState;
	private ParticleSystem.MainModule _smokeParticlesSettings;
	private float _heatPerSecond;
	private float _toastedFraction;
	private float _initBurnTime;
	private float _initShrivelTime;
	private PlayerInfo _attachedPlayer;

	private void Awake()
	{
		_smokeParticlesSettings = _smokeParticles.main;
		_fireRenderer.enabled = false;
		_smokeParticles.Stop();
		_attachedPlayer = QSBPlayerManager.PlayerList.First(x => x.Marshmallow == this);
	}

	private void Update()
	{
		if (_attachedPlayer.Campfire != null)
		{
			UpdateRoast(_attachedPlayer.Campfire.AttachedObject);
		}
	}

	public void SpawnMallow()
	{
		transform.localPosition = new Vector3(0f, 0f, 0.3f);
		transform.localScale = Vector3.one;
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		_toastedFraction = 0f;
		_mallowRenderer.enabled = true;
		_mallowState = Marshmallow.MallowState.Default;
		enabled = true;

		_attachedPlayer.AudioController.PlayOneShot(AudioType.ToolMarshmallowReplace);
	}

	public void Disable()
	{
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		enabled = false;
	}

	public void Extinguish()
	{
		if (_mallowState == Marshmallow.MallowState.Burning)
		{
			_fireRenderer.enabled = false;
			_mallowState = Marshmallow.MallowState.Default;

			_attachedPlayer.AudioController.PlayOneShot(AudioType.ToolMarshmallowBlowOut);
		}
	}

	public void UpdateRoast(Campfire campfire)
	{
		UpdateHeatExposure(campfire);
		UpdateVisuals();
		transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime);
		if (_mallowState != Marshmallow.MallowState.Default)
		{
			if (_mallowState == Marshmallow.MallowState.Shriveling)
			{
				var shrivelFraction = Mathf.Clamp01((Time.time - _initShrivelTime) / 2f);
				if (shrivelFraction >= 0.5 && _fireRenderer.enabled)
				{
					_fireRenderer.enabled = false;
				}

				transform.localScale = Vector3.one * (1f - shrivelFraction);
			}
		}

		_heatPerSecond = 0f;
	}

	private void UpdateHeatExposure(Campfire campfire)
	{
		_heatPerSecond = campfire.GetHeatAtPosition(transform.position);
		_toastedFraction += _heatPerSecond * Time.deltaTime / HEAT_CAPACITY;
		_toastedFraction = Mathf.Clamp01(_toastedFraction);
	}

	private void UpdateVisuals()
	{
		var num = _heatPerSecond / BURN_THRESHOLD;
		if (num > 0f)
		{
			if (!_smokeParticles.isEmitting)
			{
				_smokeParticles.Play();
			}

			var smokeColor = new Color(1f, 1f, 1f, num);
			_smokeParticlesSettings.startColor = smokeColor;
		}
		else if (_smokeParticles.isEmitting)
		{
			_smokeParticles.Stop();
		}

		Color newColor;
		if (_toastedFraction < PERFECT_TOASTED_FRACTION)
		{
			var fractionTowardsPerfect = _toastedFraction / PERFECT_TOASTED_FRACTION;
			newColor = Color.Lerp(_rawColor, _toastedColor, fractionTowardsPerfect);
		}
		else
		{
			var fractionTowardsBurnt = (_toastedFraction - PERFECT_TOASTED_FRACTION) / 0.3f;
			newColor = Color.Lerp(_toastedColor, _burntColor, fractionTowardsBurnt);
		}

		_mallowRenderer.material.color = Color.Lerp(_mallowRenderer.material.color, newColor, RAW_TOASTED_FRACTION);
		_smokeParticles.transform.forward = Locator.GetPlayerTransform().up;
	}

	public void Burn()
	{
		if (_mallowState == Marshmallow.MallowState.Default)
		{
			_fireRenderer.enabled = true;
			_toastedFraction = 1f;
			_initBurnTime = Time.time;
			_mallowState = Marshmallow.MallowState.Burning;

			_attachedPlayer.AudioController.PlayOneShot(AudioType.ToolMarshmallowIgnite);
		}
	}

	public void Shrivel()
	{
		if (_mallowState == Marshmallow.MallowState.Burning)
		{
			_initShrivelTime = Time.time;
			_mallowState = Marshmallow.MallowState.Shriveling;
		}
	}

	public void RemoveMallow()
	{
		_smokeParticles.Stop();
		_fireRenderer.enabled = false;
		_mallowRenderer.enabled = false;
		_mallowState = Marshmallow.MallowState.Gone;
		enabled = false;
	}
}