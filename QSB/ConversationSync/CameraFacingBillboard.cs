using QSB.Utility;
using UnityEngine;

namespace QSB.ConversationSync;

[UsedInUnityProject]
public class CameraFacingBillboard : MonoBehaviour
{
	private OWCamera _activeCam;

	private void Awake()
		=> GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);

	private void Start()
	{
		_activeCam = Locator.GetActiveCamera();
		UpdateRotation();
	}

	private void OnDestroy()
		=> GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);

	private void OnSwitchActiveCamera(OWCamera activeCamera)
	{
		_activeCam = activeCamera;
		UpdateRotation();
	}

	private void LateUpdate()
		=> UpdateRotation();

	private void UpdateRotation()
		=> transform.LookAt(transform.position + (_activeCam.transform.rotation * Vector3.forward), _activeCam.transform.rotation * Vector3.up);
}