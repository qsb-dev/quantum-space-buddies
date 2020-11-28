using QSB.Utility;
using UnityEngine;

namespace QSB.Tools
{
	public class QSBProbe : MonoBehaviour
	{
		public void SetState(bool state)
		{
			if (state)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		private void Activate()
		{
			gameObject.SetActive(true);
			gameObject.Show();
		}

		private void Deactivate()
		{
			gameObject.Hide();
		}
	}
}
