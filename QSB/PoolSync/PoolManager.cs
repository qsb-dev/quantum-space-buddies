using QSB.WorldSync;

namespace QSB.PoolSync
{
	internal class PoolManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void RebuildWorldObjects(OWScene scene)
		{
			foreach (var streaming in QSBWorldSync.GetUnityObjects<NomaiRemoteCameraStreaming>())
			{
				streaming.gameObject.AddComponent<CustomNomaiRemoteCameraStreaming>();
			}

			foreach (var camera in QSBWorldSync.GetUnityObjects<NomaiRemoteCamera>())
			{
				camera.gameObject.AddComponent<CustomNomaiRemoteCamera>();
			}

			foreach (var platform in QSBWorldSync.GetUnityObjects<NomaiRemoteCameraPlatform>())
			{
				platform.gameObject.AddComponent<CustomNomaiRemoteCameraPlatform>();
			}
		}
	}
}
