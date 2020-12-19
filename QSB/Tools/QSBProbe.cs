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
				gameObject.SetActive(true);
				gameObject.Show();
				return;
			}
			gameObject.Hide();
		}
	}
}