using OWML.Common;
using QSB.Animation;
using QSB.EventsCore;
using QSB.Instruments.QSBCamera;
using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Instruments
{
	public class InstrumentsManager : PlayerSyncObject
	{
		private Transform rootObj;
		private AnimationType _savedType;
		private GameObject ChertDrum;

		public void InitLocal(Transform root)
		{
			rootObj = root;
			gameObject.AddComponent<CameraManager>();

			QSBInputManager.ChertTaunt += () => StartInstrument(AnimationType.Chert);
			QSBInputManager.EskerTaunt += () => StartInstrument(AnimationType.Esker);
			QSBInputManager.FeldsparTaunt += () => StartInstrument(AnimationType.Feldspar);
			QSBInputManager.GabbroTaunt += () => StartInstrument(AnimationType.Gabbro);
			QSBInputManager.RiebeckTaunt += () => StartInstrument(AnimationType.Riebeck);
			QSBInputManager.ExitTaunt += () => ReturnToPlayer();

			QSB.Helper.Events.Unity.RunWhen(() => Locator.GetPlayerBody() != null, SetupInstruments);

			QSBPlayerManager.PlayerSyncObjects.Add(this);
		}

		public void InitRemote(Transform root)
		{
			rootObj = root;
			QSB.Helper.Events.Unity.RunWhen(() => Locator.GetPlayerBody() != null, SetupInstruments);

			QSBPlayerManager.PlayerSyncObjects.Add(this);
		}

		private void OnDestroy()
		{
			if (!IsLocalPlayer)
			{
				return;
			}
			DebugLog.DebugWrite($"OnDestroy {PlayerId}");
			QSBInputManager.ChertTaunt -= () => StartInstrument(AnimationType.Chert);
			QSBInputManager.EskerTaunt -= () => StartInstrument(AnimationType.Esker);
			QSBInputManager.FeldsparTaunt -= () => StartInstrument(AnimationType.Feldspar);
			QSBInputManager.GabbroTaunt -= () => StartInstrument(AnimationType.Gabbro);
			QSBInputManager.RiebeckTaunt -= () => StartInstrument(AnimationType.Riebeck);
			QSBInputManager.ExitTaunt -= () => ReturnToPlayer();
		}

		private void SetupInstruments()
		{
			var bundle = QSB.InstrumentAssetBundle;
			ChertDrum = MakeChertDrum(bundle);
		}

		// EyeCompatibility : Need to find right object.
		private GameObject MakeChertDrum(AssetBundle bundle)
		{
			var drum = new GameObject();
			var mf = drum.AddComponent<MeshFilter>();
			mf.sharedMesh = bundle.LoadAsset("assets/Chert/hourglasstwinsmeshescharacters2.asset") as Mesh;
			var mr = drum.AddComponent<MeshRenderer>();
			mr.sharedMaterial = GameObject.Find("NewDrum:polySurface2").GetComponent<MeshRenderer>().material;
			drum.transform.parent = rootObj;
			drum.transform.rotation = rootObj.rotation;
			drum.transform.localPosition = Vector3.zero;
			drum.transform.localScale = new Vector3(16.0f, 16.5f, 16.0f);
			drum.SetActive(false);

			return drum;
		}

		public void StartInstrument(AnimationType type)
		{
			if (!IsLocalPlayer)
			{
				DebugLog.DebugWrite("Error - Tried to start instrument on non-local player!", MessageType.Error);
				return;
			}
			if (Player.PlayingInstrument || !Locator.GetPlayerController().IsGrounded())
			{
				return;
			}
			_savedType = Player.AnimationSync.CurrentType;
			CameraManager.Instance.SwitchTo3rdPerson();
			SwitchToType(type);
		}

		public void ReturnToPlayer()
		{
			if (!Player.PlayingInstrument)
			{
				return;
			}
			CameraManager.Instance.SwitchTo1stPerson();
			SwitchToType(_savedType);
		}

		public void SwitchToType(AnimationType type)
		{
			DebugLog.DebugWrite($"switch to type {type} player {PlayerId}");
			GlobalMessenger<uint, AnimationType>.FireEvent(EventNames.QSBChangeAnimType, QSBPlayerManager.LocalPlayerId, type);
			QSBPlayerManager.LocalPlayer.AnimationSync.SetAnimationType(type);
			CheckInstrumentProps(type);
		}

		public void CheckInstrumentProps(AnimationType type)
		{
			switch (type)
			{
				case AnimationType.Chert:
					ChertDrum.SetActive(true);
					break;
				case AnimationType.PlayerSuited:
				case AnimationType.PlayerUnsuited:
					ChertDrum.SetActive(false);
					break;
			}
		}
	}
}
