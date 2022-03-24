using QSB.CampfireSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.RoastingSync;
using QSB.Tools;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeLauncherTool;
using QSB.Tools.ProbeTool;
using UnityEngine;

namespace QSB.Player;

public partial class PlayerInfo
{
	public GameObject ProbeBody { get; set; }
	public QSBProbe Probe { get; set; }
	public QSBFlashlight FlashLight => CameraBody == null ? null : CameraBody.GetComponentInChildren<QSBFlashlight>();
	public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
	public QSBTool Translator => GetToolByType(ToolType.Translator);
	public QSBProbeLauncherTool ProbeLauncher => (QSBProbeLauncherTool)GetToolByType(ToolType.ProbeLauncher);
	private Transform _handPivot;
	public Transform HandPivot
	{
		get
		{
			if (_handPivot == null)
			{
				_handPivot = Body.transform.Find(
					// TODO : kill me for my sins
					"REMOTE_Traveller_HEA_Player_v2/" +
					"Traveller_Rig_v01:Traveller_Trajectory_Jnt/" +
					"Traveller_Rig_v01:Traveller_ROOT_Jnt/" +
					"Traveller_Rig_v01:Traveller_Spine_01_Jnt/" +
					"Traveller_Rig_v01:Traveller_Spine_02_Jnt/" +
					"Traveller_Rig_v01:Traveller_Spine_Top_Jnt/" +
					"Traveller_Rig_v01:Traveller_RT_Arm_Clavicle_Jnt/" +
					"Traveller_Rig_v01:Traveller_RT_Arm_Shoulder_Jnt/" +
					"Traveller_Rig_v01:Traveller_RT_Arm_Elbow_Jnt/" +
					"Traveller_Rig_v01:Traveller_RT_Arm_Wrist_Jnt/" +
					"REMOTE_ItemCarryTool/" +
					"HandPivot"
				);
			}

			return _handPivot;
		}
	}
	public Transform ItemSocket => HandPivot.Find("REMOTE_ItemSocket");
	public Transform ScrollSocket => HandPivot.Find("REMOTE_ScrollSocket");
	public Transform SharedStoneSocket => HandPivot.Find("REMOTE_SharedStoneSocket");
	public Transform WarpCoreSocket => HandPivot.Find("REMOTE_WarpCoreSocket");
	public Transform VesselCoreSocket => HandPivot.Find("REMOTE_VesselCoreSocket");
	public Transform SimpleLanternSocket => HandPivot.Find("REMOTE_SimpleLanternSocket");
	public Transform DreamLanternSocket => HandPivot.Find("REMOTE_DreamLanternSocket");
	public Transform SlideReelSocket => HandPivot.Find("REMOTE_SlideReelSocket");
	public Transform VisionTorchSocket => HandPivot.Find("REMOTE_VisionTorchSocket");
	public QSBMarshmallow Marshmallow { get; set; }
	public QSBCampfire Campfire { get; set; }
	public IQSBItem HeldItem { get; set; }
	public bool FlashlightActive { get; set; }
	public bool SuitedUp { get; set; }
	public bool ProbeLauncherEquipped { get; set; }
	public bool SignalscopeEquipped { get; set; }
	public bool TranslatorEquipped { get; set; }
	public bool ProbeActive { get; set; }
	public GameObject RoastingStick { get; set; }
}
