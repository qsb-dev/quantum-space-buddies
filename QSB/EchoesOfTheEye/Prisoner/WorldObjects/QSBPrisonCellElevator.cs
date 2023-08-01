using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.EchoesOfTheEye.Prisoner.Messages;
using QSB.ItemSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.WorldObjects;

public class QSBPrisonCellElevator : WorldObject<PrisonCellElevator>, IQSBDropTarget, IGhostObject
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new CellevatorCallMessage(AttachedObject._targetFloorIndex) { To = to });

	IItemDropTarget IQSBDropTarget.AttachedObject => AttachedObject;

	/*public override async UniTask Init(CancellationToken ct)
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
		lightComponent.intensity = 2;
		lightComponent.spotAngle = 50;
		lightComponent.shadows = LightShadows.Soft;
		lightComponent.shadowStrength = 1f;
		lightComponent.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
		lightComponent.shadowBias = 0.05f;
		lightComponent.shadowNormalBias = 0.4f;
		lightComponent.shadowNearPlane = 0.2f;
		Light.AddComponent<OWLight2>();

		var projectorComponent = AUTO_SLIDE_PROJECTOR.AddComponent<CustomAutoSlideProjector>();
		projectorComponent._light = Light.GetComponent<OWLight2>();

		var cellevator1 = TextureHelper.LoadTexture("Assets/cellevator1.png", TextureWrapMode.Clamp, true);
		var cellevator2 = TextureHelper.LoadTexture("Assets/cellevator2.png", TextureWrapMode.Clamp, true);
		var cellevator3 = TextureHelper.LoadTexture("Assets/cellevator3.png", TextureWrapMode.Clamp, true);

		var slideCollection = new CustomSlideCollection(3);
		slideCollection.slides[0] = new CustomSlide() { _image = cellevator1 };
		slideCollection.slides[1] = new CustomSlide() { _image = cellevator2 };
		slideCollection.slides[2] = new CustomSlide() { _image = cellevator3 };

		var slideContainer = AUTO_SLIDE_PROJECTOR.AddComponent<CustomSlideCollectionContainer>();
		slideContainer._slideCollection = slideCollection;

		projectorComponent.SetSlideCollection(slideContainer);
		projectorComponent._defaultSlideDuration = 1f;

		AUTO_SLIDE_PROJECTOR.SetActive(true);

		projectorComponent.Play(false);
	}*/
}
