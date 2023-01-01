using UnityEngine;

namespace QSB.Tools.ProbeTool;

[RequireComponent(typeof(Animator))]
internal class QSBProbeAnimatorController : MonoBehaviour
{
	private Animator _animator;
	private QSBSurveyorProbe _probe;

	[SerializeField]
	private Transform _centerBone;

	private Quaternion _startCenterBoneRotation = Quaternion.identity;
	private Quaternion _targetCenterBoneRotation = Quaternion.identity;
	private Quaternion _currentCenterBoneRotation = Quaternion.identity;

	private float _rotationT;

	private void Awake()
	{
		this._animator = base.GetComponent<Animator>();
		this._probe = transform.parent.parent.parent.GetRequiredComponent<QSBSurveyorProbe>();
		this._probe.OnLaunchProbe += this.OnProbeFire;
		this._probe.OnAnchorProbe += this.OnProbeAnchor;
		this._probe.OnRetrieveProbe += this.OnProbeReset;
	}

	private void Start()
	{
		this._probe.GetRotatingCamera().OnRotateCamera += this.OnRotateCamera;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		this._probe.OnLaunchProbe -= this.OnProbeFire;
		this._probe.OnAnchorProbe -= this.OnProbeAnchor;
		this._probe.OnRetrieveProbe -= this.OnProbeReset;
	}

	public Quaternion GetCenterBoneLocalRotation()
	{
		return this._centerBone.localRotation;
	}

	private void OnProbeFire()
	{
		this._animator.SetTrigger("Fire");
	}

	private void OnProbeAnchor()
	{
		this._animator.SetTrigger("Impact");
	}

	private void OnProbeReset()
	{
		this._animator.SetTrigger("Reset");
		this._centerBone.localRotation = Quaternion.identity;
		this._startCenterBoneRotation = Quaternion.identity;
		this._targetCenterBoneRotation = Quaternion.identity;
		this._currentCenterBoneRotation = Quaternion.identity;
		this._rotationT = 0f;
		base.enabled = false;
	}

	private void OnRotateCamera(Vector2 rotation)
	{
		this._startCenterBoneRotation = this._currentCenterBoneRotation;
		this._targetCenterBoneRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
		this._rotationT = 0f;
		base.enabled = true;
	}

	private void LateUpdate()
	{
		this._rotationT += 10f * Time.deltaTime;
		if (this._rotationT >= 1f)
		{
			this._rotationT = 1f;
			this._currentCenterBoneRotation = this._targetCenterBoneRotation;
			base.enabled = false;
		}
		else
		{
			var t = Mathf.Sqrt(Mathf.SmoothStep(0f, 1f, this._rotationT));
			this._currentCenterBoneRotation = Quaternion.Lerp(this._startCenterBoneRotation, this._targetCenterBoneRotation, t);
		}
		this._centerBone.localRotation = this._currentCenterBoneRotation;
	}
}
