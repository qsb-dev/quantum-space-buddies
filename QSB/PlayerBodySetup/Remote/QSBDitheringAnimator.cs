using QSB.Utility;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace QSB.PlayerBodySetup.Remote;

[UsedInUnityProject]
public class QSBDitheringAnimator : MonoBehaviour
{
	public bool FullyVisible => !enabled && OWMath.ApproxEquals(_visibleFraction, 1);
	public bool FullyInvisible => !enabled && OWMath.ApproxEquals(_visibleFraction, 0);

	private float _visibleTarget = 1;
	private float _visibleFraction = 1;
	private float _fadeRate;
	private (OWRenderer Renderer, bool UpdateShadows)[] _renderers;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>(true)
			.Select(x => (x.gameObject.GetAddComponent<OWRenderer>(), x.shadowCastingMode != ShadowCastingMode.Off))
			.ToArray();
		enabled = false;
	}

	public void SetVisible(bool visible, float seconds = 0)
	{
		var visibleTarget = visible ? 1 : 0;
		if (OWMath.ApproxEquals(visibleTarget, _visibleTarget))
		{
			return;
		}

		_visibleTarget = visibleTarget;
		if (seconds != 0)
		{
			_fadeRate = 1 / seconds;
			enabled = true;
		}
		else
		{
			_visibleFraction = _visibleTarget;
			UpdateRenderers();
		}
	}

	private void Update()
	{
		_visibleFraction = Mathf.MoveTowards(_visibleFraction, _visibleTarget, _fadeRate * Time.deltaTime);
		if (OWMath.ApproxEquals(_visibleFraction, _visibleTarget))
		{
			_visibleFraction = _visibleTarget;
			enabled = false;
		}

		UpdateRenderers();
	}

	private void UpdateRenderers()
	{
		foreach (var (renderer, updateShadows) in _renderers)
		{
			if (renderer == null)
			{
				continue;
			}

			renderer.SetDitherFade(1 - _visibleFraction);
			if (updateShadows)
			{
				renderer._renderer.shadowCastingMode = FullyVisible ? ShadowCastingMode.On : ShadowCastingMode.Off;
			}
		}
	}
}