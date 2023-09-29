using UnityEngine;

namespace Popcron
{
	public class FrustumDrawer : Drawer
	{
		public FrustumDrawer()
		{

		}

		public override int Draw(ref Vector3[] buffer, params object[] values)
		{
			var camera = (OWCamera)values[0];

			//bottom left
			buffer[0] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
			buffer[1] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane / 2));
			//bottom right
			buffer[2] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.nearClipPlane));
			buffer[3] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane / 2));
			//top left
			buffer[4] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.nearClipPlane));
			buffer[5] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane / 2));
			//top right
			buffer[6] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane));
			buffer[7] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane / 2));
			//bottom left to bottom right
			buffer[8] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane / 2));
			buffer[9] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane / 2));
			//bottom right to top right
			buffer[10] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.farClipPlane / 2));
			buffer[11] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane / 2));
			//top right to top left
			buffer[12] = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.farClipPlane / 2));
			buffer[13] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane / 2));
			//top left to bottom left
			buffer[14] = camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, camera.farClipPlane / 2));
			buffer[15] = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.farClipPlane / 2));
			return 16;
		}
	}
}