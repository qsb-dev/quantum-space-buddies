using QSB.Animation;
using QSB.Player.Instruments;
using QSB.Player.Markers;
using QSB.TransformSync;
using UnityEngine;

namespace QSB.Player.TransformSyncs
{
	public class PlayerTransformSync : SyncedTransform
	{
		public static PlayerTransformSync LocalInstance { get; private set; }

		static PlayerTransformSync() => AnimControllerPatch.Init();

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Player.HudMarker?.Remove();
			PlayerManager.RemovePlayer(PlayerId);
		}

		private Transform GetPlayerModel() =>
			Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");

		protected override Transform InitLocalTransform()
		{
			var body = GetPlayerModel();

			GetComponent<AnimationSync>().InitLocal(body);
			GetComponent<InstrumentsManager>().InitLocal(body);

			Player.Body = body.gameObject;

			return body;
		}

		protected override Transform InitRemoteTransform()
		{
			var body = Instantiate(GetPlayerModel());

			GetComponent<AnimationSync>().InitRemote(body);
			GetComponent<InstrumentsManager>().InitRemote(body);

			var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
			marker.Init(Player);

			body.gameObject.AddComponent<PlayerMapMarker>().PlayerName = Player.Name;

			Player.Body = body.gameObject;

			return body;
		}

		private void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !Player.IsReady || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}
			Popcron.Gizmos.Cube(Player.Body.transform.position, Player.Body.transform.rotation, new Vector3(1, 2, 1));
			Popcron.Gizmos.Line(ReferenceSector.Position, Player.Body.transform.position, Color.blue, true);
			Popcron.Gizmos.Sphere(ReferenceSector.Position, 5f, Color.cyan);
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& PlayerManager.PlayerExists(Player.PlayerId)
			&& Player.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}