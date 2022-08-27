using QSB.Utility;
using UnityEngine;

namespace QSB.Audio;

[UsedInUnityProject]
public class QSBPlayerAudioController : MonoBehaviour
{
	public OWAudioSource _oneShotExternalSource;
	public OWAudioSource _repairToolSource;

	public void PlayEquipTool()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolTranslatorEquip);

	public void PlayUnequipTool()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolTranslatorUnequip);

	public void PlayTurnOnFlashlight()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolFlashlightOn);

	public void PlayTurnOffFlashlight()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.ToolFlashlightOff);

	public void PlayWearSuit()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.PlayerSuitWearSuit);

	public void PlayRemoveSuit()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.PlayerSuitRemoveSuit);

	public void PlayWearHelmet()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.PlayerSuitWearHelmet);

	public void PlayRemoveHelmet()
		=> _oneShotExternalSource?.PlayOneShot(AudioType.PlayerSuitRemoveHelmet);
}