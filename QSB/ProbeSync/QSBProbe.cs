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
			_detectorObj = GetComponentInChildren<ForceDetector>().gameObject;
			_rulesetDetector = _detectorObj.GetComponent<RulesetDetector>();
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