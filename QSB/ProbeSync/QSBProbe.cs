using QSB.Utility;
using UnityEngine;

namespace QSB.ProbeSync
{
	public class QSBProbe : MonoBehaviour
	{
		public delegate void SurveyorProbeEvent();
		public delegate void RetrieveEvent(float retrieveLength);

		public event SurveyorProbeEvent OnLaunchProbe;
		public event SurveyorProbeEvent OnAnchorProbe;
		public event SurveyorProbeEvent OnUnanchorProbe;
		public event SurveyorProbeEvent OnRetrieveProbe;
		public event SurveyorProbeEvent OnProbeDestroyed;
		public event RetrieveEvent OnStartRetrieveProbe;

		private GameObject _detectorObj;
		private RulesetDetector _rulesetDetector;
		private SingularityWarpEffect _warpEffect;
		private bool _isRetrieving;

		public RulesetDetector GetRulesetDetector()
			=> _rulesetDetector;

		private void Awake()
		{
			_detectorObj = GetComponentInChildren<RulesetDetector>().gameObject;
			_rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
			_warpEffect = GetComponentInChildren<SingularityWarpEffect>();
			_warpEffect.OnWarpComplete += OnWarpComplete;
			_isRetrieving = false;
		}

		private void OnDestroy()
		{
			_warpEffect.OnWarpComplete -= OnWarpComplete;
		}

		private void OnWarpComplete()
		{
			DebugLog.DebugWrite($"OnWarpComplete");
			//gameObject.SetActive(false);
			transform.localScale = Vector3.one;
			_isRetrieving = false;
		}

		public bool IsRetrieving()
		{
			return IsLaunched() && _isRetrieving;
		}

		public bool IsLaunched()
		{
			return gameObject.activeSelf;
		}

		public void HandleEvent(ProbeEvent probeEvent)
		{
			switch (probeEvent)
			{
				case ProbeEvent.Launch:
					if (OnLaunchProbe == null)
					{
						DebugLog.ToConsole($"Warning - OnLaunchProbe is null!", OWML.Common.MessageType.Warning);
						break;
					}

					OnLaunchProbe();
					break;
				case ProbeEvent.Anchor:
					if (OnAnchorProbe == null)
					{
						DebugLog.ToConsole($"Warning - OnAnchorProbe is null!", OWML.Common.MessageType.Warning);
						break;
					}

					OnAnchorProbe();
					break;
				case ProbeEvent.Unanchor:
					DebugLog.DebugWrite($"OnUnanchorProbe");
					OnUnanchorProbe();
					break;
				case ProbeEvent.Retrieve:
					if (OnRetrieveProbe == null)
					{
						DebugLog.ToConsole($"Warning - OnRetrieveProbe is null!", OWML.Common.MessageType.Warning);
						break;
					}

					OnRetrieveProbe();
					break;
				case ProbeEvent.Destroy:
					if (OnProbeDestroyed == null)
					{
						DebugLog.ToConsole($"Warning - OnProbeDestroyed is null!", OWML.Common.MessageType.Warning);
						break;
					}

					OnProbeDestroyed();
					break;
				case ProbeEvent.Invalid:
				default:
					DebugLog.DebugWrite($"Warning - Unknown/Invalid probe event.", OWML.Common.MessageType.Warning);
					break;
			}
		}

		public void OnStartRetrieve(float duration)
		{
			DebugLog.DebugWrite($"OnStartRetrieving");
			if (!_isRetrieving)
			{
				_isRetrieving = true;
				DebugLog.DebugWrite($"start warp out");
				_warpEffect.WarpObjectOut(duration);

				if (_warpEffect.gameObject.activeInHierarchy == false)
				{
					DebugLog.DebugWrite($"warp effect GO is not active!");
				}
			}
		}

		public void SetState(bool state)
		{
			if (state)
			{
				gameObject.SetActive(true);
				gameObject.Show();
				return;
			}

			gameObject.Hide();
		}
	}
}