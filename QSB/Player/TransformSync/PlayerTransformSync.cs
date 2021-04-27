using QSB.Animation.Player;
using QSB.Instruments;
using QSB.TransformSync;
using UnityEngine;

namespace QSB.Player.TransformSync
{
	public class PlayerTransformSync : QSBNetworkTransform
	{
		public static PlayerTransformSync LocalInstance { get; private set; }

		static PlayerTransformSync() => AnimControllerPatch.Init();

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		public override void Start()
		{
			base.Start();
			Player.TransformSync = this;
		}

		protected override void OnDestroy()
		{
			QSBPlayerManager.OnRemovePlayer?.Invoke(PlayerId);
			base.OnDestroy();
			if (QSBPlayerManager.PlayerExists(PlayerId))
			{
				Player.HudMarker?.Remove();
				QSBPlayerManager.RemovePlayer(PlayerId);
			}
		}

		private Transform GetPlayerModel() =>
			Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");

		protected override GameObject InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetPlayerSectorDetector());
			var body = GetPlayerModel();

			GetComponent<AnimationSync>().InitLocal(body);
			GetComponent<InstrumentsManager>().InitLocal(body);

			Player.Body = body.gameObject;

			return body.gameObject;
		}

		protected override GameObject InitRemoteTransform()
		{
			var body = Instantiate(GetPlayerModel());

			GetComponent<AnimationSync>().InitRemote(body);
			GetComponent<InstrumentsManager>().InitRemote(body);

			var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
			marker.Init(Player);

			body.gameObject.AddComponent<PlayerMapMarker>().PlayerName = Player.Name;

			Player.Body = body.gameObject;

			return body.gameObject;
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& Player.PlayerStates.IsReady
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}