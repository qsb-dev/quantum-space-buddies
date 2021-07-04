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

		public RulesetDetector GetRulesetDetector() 
			=> _rulesetDetector;

		private void Awake()
		{
			_detectorObj = GetComponentInChildren<RulesetDetector>().gameObject;
			_rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
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

					DebugLog.DebugWrite($"OnLaunchProbe");
					OnLaunchProbe();
					break;
				case ProbeEvent.Anchor:
					if (OnAnchorProbe == null)
					{
						DebugLog.ToConsole($"Warning - OnAnchorProbe is null!", OWML.Common.MessageType.Warning);
						break;
					}

					DebugLog.DebugWrite($"OnAnchorProbe");
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

					DebugLog.DebugWrite($"OnRetrieveProbe");
					OnRetrieveProbe();
					break;
				case ProbeEvent.Destroy:
					DebugLog.DebugWrite($"OnDestroyProbe");
					OnProbeDestroyed();
					break;
				case ProbeEvent.Invalid:
				default:
					DebugLog.DebugWrite($"Warning - Unknown/Invalid probe event.", OWML.Common.MessageType.Warning);
					break;
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