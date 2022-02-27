using UnityEngine;

namespace QSB.Audio
{
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
	}
}