using OWML.Common;
using QSB.PlayerBodySetup.Remote;
using QSB.Utility;
using UnityEngine;

namespace QSB.Animation.Player;

[UsedInUnityProject]
public class HelmetAnimator : MonoBehaviour
{
    public Transform FakeHelmet;
    public Transform FakeHead;
    public GameObject SuitGroup;

    private QSBDitheringAnimator _fakeHelmetDitheringAnimator;

    private const float ANIM_TIME = 0.5f;
    private bool _isPuttingOnHelmet;
    private bool _isTakingOffHelmet;

    public void Start()
    {
        _fakeHelmetDitheringAnimator = FakeHelmet.GetComponent<QSBDitheringAnimator>();

        FakeHead.gameObject.SetActive(false);
    }

    public void RemoveHelmet()
    {
        if (!SuitGroup.activeSelf)
        {
            DebugLog.DebugWrite($"Trying to remove helmet when player is not wearing suit!", MessageType.Error);
            return;
        }

        _fakeHelmetDitheringAnimator.SetVisible(true);
        FakeHelmet.gameObject.SetActive(true);
        FakeHead.gameObject.SetActive(true);
        _fakeHelmetDitheringAnimator.SetVisible(false, ANIM_TIME);
        _isTakingOffHelmet = true;
    }

    public void PutOnHelmet()
    {
        if (!SuitGroup.activeSelf)
        {
            DebugLog.DebugWrite($"Trying to put on helmet when player is not wearing suit!", MessageType.Error);
            return;
        }

        _fakeHelmetDitheringAnimator.SetVisible(false);
		FakeHead.gameObject.SetActive(true);
        FakeHelmet.gameObject.SetActive(true);
		_fakeHelmetDitheringAnimator.SetVisible(true, ANIM_TIME);
		_isPuttingOnHelmet = true;
    }

    public void SetHelmetInstant(bool helmetOn)
    {
	    if (helmetOn)
	    {
		    FakeHelmet.gameObject.SetActive(true);
		    _fakeHelmetDitheringAnimator.SetVisible(true);
			FakeHead.gameObject.SetActive(false);
		}
	    else
	    {
		    _fakeHelmetDitheringAnimator.SetVisible(false);
			FakeHelmet.gameObject.SetActive(false);
		    if (!SuitGroup.activeSelf)
		    {
			    FakeHead.gameObject.SetActive(false);
		    }
		}
    }

    private void Update()
    {
	    if (_isPuttingOnHelmet && _fakeHelmetDitheringAnimator.FullyVisible)
	    {
		    _isPuttingOnHelmet = false;
		    FakeHead.gameObject.SetActive(false);
		}

	    if (_isTakingOffHelmet && _fakeHelmetDitheringAnimator.FullyInvisible)
	    {
		    FakeHelmet.gameObject.SetActive(false);

		    if (!SuitGroup.activeSelf)
		    {
			    FakeHead.gameObject.SetActive(false);
			}
		}
    }
}
