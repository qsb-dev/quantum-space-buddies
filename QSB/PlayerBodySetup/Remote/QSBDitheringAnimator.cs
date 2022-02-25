using UnityEngine;
using UnityEngine.Rendering;

namespace QSB.PlayerBodySetup.Remote;

public class QSBDitheringAnimator : MonoBehaviour
{
	[SerializeField]
	private bool _toggleShadowCasting = true;
	public bool _visible { get; private set; } = true;
	public float _visibleFraction { get; private set; } = 1f;
	private float _fadeRate = 1f;
	public OWRenderer[] _renderers { get; private set; }
	private bool[] _ignoreToggleShadows;

	private void Awake()
	{
		var componentsInChildren = GetComponentsInChildren<Renderer>(true);
		_renderers = new OWRenderer[componentsInChildren.Length];
		_ignoreToggleShadows = new bool[componentsInChildren.Length];
		for (var i = 0; i < _renderers.Length; i++)
		{
			_renderers[i] = componentsInChildren[i].GetComponent<OWRenderer>();
			if (_renderers[i] == null)
			{
				_renderers[i] = componentsInChildren[i].gameObject.AddComponent<OWRenderer>();
			}

			_ignoreToggleShadows[i] = componentsInChildren[i].shadowCastingMode == ShadowCastingMode.Off;
		}
	}

	private void Start() => enabled = false;

	public void SetVisibleImmediate(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_visibleFraction = _visible ? 1f : 0f;
			UpdateDithering();
			UpdateShadowCasting();
		}
	}

	public void SetVisible(bool visible, float fadeRate)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_fadeRate = fadeRate;
			if (!_visible)
			{
				UpdateShadowCasting();
			}

			enabled = true;
		}
	}

	private void Update()
	{
		var target = _visible ? 1f : 0f;
		_visibleFraction = Mathf.MoveTowards(_visibleFraction, target, _fadeRate * Time.deltaTime);
		if (OWMath.ApproxEquals(_visibleFraction, target))
		{
			_visibleFraction = target;
			enabled = false;
			if (_visible)
			{
				UpdateShadowCasting();
			}
		}

		UpdateDithering();
	}

	private void UpdateDithering()
	{
		foreach (var renderer in _renderers)
		{
			if (renderer != null)
			{
				renderer.SetDitherFade(1f - _visibleFraction);
			}
		}
	}

	private void UpdateShadowCasting()
	{
		if (!_toggleShadowCasting)
		{
			return;
		}

		for (var i = 0; i < _renderers.Length; i++)
		{
			if (_ignoreToggleShadows[i])
			{
				continue;
			}

			_renderers[i].GetRenderer().shadowCastingMode = _visible ? ShadowCastingMode.On : ShadowCastingMode.Off;
		}
	}
}