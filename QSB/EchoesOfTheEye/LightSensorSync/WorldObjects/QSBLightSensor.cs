using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects
{
	// will be implemented when eote
	internal class QSBLightSensor : WorldObject<SingleLightSensor>
	{
		internal bool _illuminatedByLocal;

		public event Action OnDetectLocalLight;
		public event Action OnDetectLocalDarkness;

		public override async UniTask Init(CancellationToken ct)
		{
			AttachedObject.OnDetectLight += OnDetectLight;
			AttachedObject.OnDetectDarkness += OnDetectDarkness;
		}

		public override void OnRemoval()
		{
			AttachedObject.OnDetectLight -= OnDetectLight;
			AttachedObject.OnDetectDarkness -= OnDetectDarkness;
		}

		private void OnDetectLight()
		{
			if (_illuminatedByLocal)
			{
				OnDetectLocalLight?.Invoke();
			}
		}

		private void OnDetectDarkness()
		{
			if (_illuminatedByLocal)
			{
				OnDetectLocalDarkness?.Invoke();
			}
		}

		public override void SendInitialState(uint to) { }
	}
}