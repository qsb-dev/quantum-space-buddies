using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        private float _fadeInDuration = 2f;
        private AnimationCurve _fadeInCurve;
        private AnimationCurve _fadeOutCurve;
        private OWEmissiveRenderer _emissiveRenderer;

        private OWLight2 _light;
        private float _originalRange;
        private float _fadeFraction;
        private float _targetFade;
        private float _startFade;
        private float _startFadeTime;
        private float _fadeDuration;

        private GameObject _detectorObj;
        private RulesetDetector _rulesetDetector;

        // This whole file is hacked together bits of SurveyorProbe and ProbeLantern.
        // I hate it too.

        private void Awake()
        {
            _detectorObj = GetComponentInChildren<ForceDetector>().gameObject;
            _rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
        }

        public void Init(SurveyorProbe oldProbe, ProbeLantern oldLantern)
        {
            _fadeInCurve = oldLantern.GetValue<AnimationCurve>("_fadeInCurve");
            _fadeOutCurve = oldLantern.GetValue<AnimationCurve>("_fadeOutCurve");
            _fadeInDuration = oldLantern.GetValue<float>("_fadeInDuration");
        }

        private void Start()
        {
            base.gameObject.SetActive(false);
            if (_emissiveRenderer != null)
            {
                _emissiveRenderer.SetEmissiveScale(0f);
            }
            _light.GetLight().enabled = false;
            _originalRange = _light.range;
            base.enabled = false;
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            var animationCurve = (_targetFade <= 0f) ? _fadeOutCurve : _fadeInCurve;
            var num = Mathf.InverseLerp(_startFadeTime, _startFadeTime + _fadeDuration, Time.time);
            _fadeFraction = Mathf.Lerp(_startFade, _targetFade, animationCurve.Evaluate(num));
            var probeRuleSet = _rulesetDetector.GetProbeRuleSet();
            float lightRange = (!(probeRuleSet != null) || !probeRuleSet.GetOverrideLanternRange()) ? _originalRange : probeRuleSet.GetLanternRange();
            _light.range = lightRange * _fadeFraction;
            if (_emissiveRenderer != null)
            {
                _emissiveRenderer.SetEmissiveScale(_fadeFraction);
            }
            if (num >= 1f)
            {
                enabled = false;
            }
        }

        private void FadeTo(float fade, float duration)
        {
            _startFadeTime = Time.time;
            _fadeDuration = duration;
            _startFade = _fadeFraction;
            _targetFade = fade;
            base.enabled = true;
        }

        public void OnProbeAnchorToSurface()
        {
            _light.GetLight().enabled = true;
            _light.range = 0f;
            FadeTo(1f, _fadeInDuration);
        }
    }
}
