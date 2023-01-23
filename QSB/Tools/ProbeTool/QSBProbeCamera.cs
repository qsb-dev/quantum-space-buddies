using QSB.Player;
using UnityEngine;

namespace QSB.Tools.ProbeTool;

[RequireComponent(typeof(OWCamera))]
public class QSBProbeCamera : MonoBehaviour
{
	[SerializeField]
	private ProbeCamera.ID _id;

	private OWCamera _camera;
	private RenderTexture _snapshotTexture;
	private NoiseImageEffect _noiseEffect;
	private static OWCamera _lastSnapshotCamera;
	private Quaternion _origLocalRotation;
	private Quaternion _origParentLocalRotation;
	private Vector2 _cameraRotation = Vector2.zero;
	private SandLevelController _sandLevelController;
	private PlayerInfo owner;

	public event ProbeCamera.RotateCameraEvent OnRotateCamera;

	private void Awake()
	{
		_camera = this.GetRequiredComponent<OWCamera>();
		_camera.enabled = false;
		_noiseEffect = GetComponent<NoiseImageEffect>();
		//this._snapshotTexture = ProbeCamera.GetSharedSnapshotTexture();
	}

	private void OnDestroy()
		=> _snapshotTexture = null;

	private void Start()
	{
		var probe = GetComponentInParent<QSBSurveyorProbe>();
		owner = probe.GetOwner();
	}

	public static OWCamera GetLastSnapshotCamera() =>
		_lastSnapshotCamera;

	public OWCamera GetOWCamera() 
		=> _camera;

	public ProbeCamera.ID GetID()
		=> _id;

	public void SetSandLevelController(SandLevelController sandLevelController)
		=> _sandLevelController = sandLevelController;

	public bool HasInterference() =>
		(_id != ProbeCamera.ID.PreLaunch && owner.IsInMoon != owner.Probe.InsideQuantumMoon)
		|| (_sandLevelController != null && _sandLevelController.IsPointBuried(transform.position))
		|| (Locator.GetCloakFieldController() != null && owner.IsInCloak != owner.Probe.InsideCloak);

	public RenderTexture TakeSnapshot()
	{
		_lastSnapshotCamera = _camera;
		if (_noiseEffect != null)
		{
			_noiseEffect.enabled = HasInterference();
		}

		_camera.targetTexture = _snapshotTexture;
		_camera.Render();
		return _snapshotTexture;
	}

	public void RotateHorizontal(float cameraRotationX)
	{
		_cameraRotation.x = cameraRotationX;
		transform.parent.localRotation = _origParentLocalRotation * Quaternion.AngleAxis(_cameraRotation.x, Vector3.up);
		OnRotateCamera?.Invoke(_cameraRotation);
	}

	public void RotateVertical(float cameraRotationY)
	{
		_cameraRotation.y = cameraRotationY;
		transform.localRotation = _origLocalRotation * Quaternion.AngleAxis(_cameraRotation.y, Vector3.right);
		OnRotateCamera?.Invoke(_cameraRotation);
	}

	public void ResetRotation()
	{
		_cameraRotation = Vector2.zero;
		transform.localRotation = _origLocalRotation;
		transform.parent.localRotation = _origParentLocalRotation;
	}
}
