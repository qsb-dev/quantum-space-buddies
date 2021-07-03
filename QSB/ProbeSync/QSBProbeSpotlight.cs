using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.ProbeSync
{
	class QSBProbeSpotlight : MonoBehaviour
	{
		public ProbeCamera.ID _id;
		public float _fadeInLength = 1f;

		private QSBProbe _probe;
		private OWLight2 _light;
		private bool _inFlight;
		private float _intensity;
		private float _timer;

		private void Awake()
		{
			_probe = this.GetRequiredComponentInChildren<QSBProbe>();
			_light = GetComponent<OWLight2>();
			_intensity = _light.GetLight().intensity;
			_light.GetLight().enabled = false;
			enabled = false;
			_probe.OnLaunchProbe += OnLaunch;
			_probe.OnAnchorProbe += OnAnchorOrRetrieve;
			_probe.OnRetrieveProbe += OnAnchorOrRetrieve;
		}

		private void OnDestroy()
		{
			_probe.OnLaunchProbe -= OnLaunch;
			_probe.OnAnchorProbe -= OnAnchorOrRetrieve;
			_probe.OnRetrieveProbe -= OnAnchorOrRetrieve;
			GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", new Callback<ProbeCamera>(OnTakeSnapshot));
		}

		private void Update()
		{
			_timer += Time.deltaTime;
			var num = Mathf.Clamp01(_timer / _fadeInLength);
			var intensityScale = (2f - num) * num * _intensity;
			_light.SetIntensityScale(intensityScale);
		}

		private void StartFadeIn()
		{
			if (!enabled)
			{
				_light.GetLight().enabled = true;
				_light.SetIntensityScale(0f);
				_timer = 0f;
				enabled = true;
			}
		}

		private void OnLaunch()
		{
			if (_id == ProbeCamera.ID.Forward)
			{
				StartFadeIn();
			}

			_inFlight = true;
		}

		private void OnAnchorOrRetrieve()
		{
			_light.GetLight().enabled = false;
			enabled = false;
			_inFlight = false;
		}
	}
}
