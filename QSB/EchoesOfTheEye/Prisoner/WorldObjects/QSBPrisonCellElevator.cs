using Cysharp.Threading.Tasks;
using QSB.Audio;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.ItemSync.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.WorldObjects;

internal class QSBPrisonCellElevator : WorldObject<PrisonCellElevator>, IQSBDropTarget, IGhostObject
{
	public override void SendInitialState(uint to)
	{
		// todo : implement this
	}

	private CustomAutoSlideProjector projector;
	private OWLight2 light;

	IItemDropTarget IQSBDropTarget.AttachedObject => AttachedObject;

	public override void DisplayLines()
	{
		Popcron.Gizmos.Sphere(projector.transform.position, 0.5f, Color.white);
		Popcron.Gizmos.Line(QSBPlayerManager.LocalPlayer.Body.transform.position, light.transform.position);
		Popcron.Gizmos.Cone(light.transform.position, light.transform.rotation, light.range, light.GetLight().spotAngle, Color.yellow);
	}

	public override async UniTask Init(CancellationToken ct)
	{
		DebugLog.DebugWrite($"INIT {AttachedObject.name}");

		var Interactibles_PrisonCell = AttachedObject.GetAttachedOWRigidbody().GetOrigParent().parent;
		DebugLog.DebugWrite(Interactibles_PrisonCell.name);

		var AUTO_SLIDE_PROJECTOR = new GameObject("AUTO SLIDE PROJECTOR");
		AUTO_SLIDE_PROJECTOR.transform.parent = Interactibles_PrisonCell;
		AUTO_SLIDE_PROJECTOR.transform.localPosition = new Vector3(-1.8f, 6.4f, 11.33f);
		AUTO_SLIDE_PROJECTOR.transform.localRotation = Quaternion.identity;
		AUTO_SLIDE_PROJECTOR.transform.localScale = Vector3.one;

		AUTO_SLIDE_PROJECTOR.SetActive(false);

		var Light = new GameObject("Light");
		Light.transform.parent = AUTO_SLIDE_PROJECTOR.transform;
		Light.transform.localPosition = Vector3.zero;
		Light.transform.localRotation = Quaternion.Euler(32f, 90f, 0f);
		var lightComponent = Light.AddComponent<Light>();
		lightComponent.type = LightType.Spot;
		lightComponent.range = 10;
		lightComponent.spotAngle = 60;
		lightComponent.shadows = LightShadows.Soft;
		lightComponent.shadowStrength = 1f;
		lightComponent.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
		lightComponent.shadowBias = 0.05f;
		lightComponent.shadowNormalBias = 0.4f;
		lightComponent.shadowNearPlane = 0.2f;
		light = Light.AddComponent<OWLight2>();

		var projectorComponent = AUTO_SLIDE_PROJECTOR.AddComponent<CustomAutoSlideProjector>();
		projectorComponent._light = Light.GetComponent<OWLight2>();

		var cellevator1 = QSBCore.Helper.Assets.GetTexture("cellevator1.png");
		cellevator1.wrapMode = TextureWrapMode.Clamp;
		var cellevator2 = QSBCore.Helper.Assets.GetTexture("cellevator2.png");
		cellevator2.wrapMode = TextureWrapMode.Clamp;
		var cellevator3 = QSBCore.Helper.Assets.GetTexture("cellevator3.png");
		cellevator3.wrapMode = TextureWrapMode.Clamp;

		var slideCollection = new CustomSlideCollection(3);
		slideCollection.slides[0] = new CustomSlide() { _image = cellevator1 };
		slideCollection.slides[1] = new CustomSlide() { _image = cellevator2 };
		slideCollection.slides[2] = new CustomSlide() { _image = cellevator3 };

		var slideContainer = AUTO_SLIDE_PROJECTOR.AddComponent<CustomSlideCollectionContainer>();
		slideContainer._slideCollection = slideCollection;

		projectorComponent.SetSlideCollection(slideContainer);
		projectorComponent._defaultSlideDuration = 1f;

		AUTO_SLIDE_PROJECTOR.SetActive(true);

		projector = projectorComponent;

		projectorComponent.Play(false);
	}
}
