using QSB.Utility;
using UnityEngine;

namespace Popcron
{
	class FrustumDrawer : Drawer
	{
		public FrustumDrawer()
		{

		}

		public override int Draw(ref Vector3[] buffer, params object[] values)
		{
			var camera = (OWCamera)values[0];

			if (camera == null)
			{
				DebugLog.DebugWrite("camera null");
			}

			//bottom left
			buffer[0] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
			buffer[1] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane));
			//bottom right
			buffer[2] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.nearClipPlane));
			buffer[3] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane));
			//top left
			buffer[4] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.nearClipPlane));
			buffer[5] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane));
			//top right
			buffer[6] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane));
			buffer[7] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane));
			//bottom left to bottom right
			buffer[8] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane));
			buffer[9] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane));
			//bottom right to top right
			buffer[10] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane));
			buffer[11] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane));
			//top right to top left
			buffer[12] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane));
			buffer[13] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane));
			//top left to bottom left
			buffer[14] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane));
			buffer[15] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane));
			return 16;
		}
	}
}
