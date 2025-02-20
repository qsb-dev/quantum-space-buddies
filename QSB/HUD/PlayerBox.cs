using OWML.Common;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Utility;
using QSB.ServerSettings;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.HUD;

[UsedInUnityProject]
public class PlayerBox : MonoBehaviour
{
	public Text PlayerName;
	public Image InfoImage;

	private PlayerInfo _player;

	public string CurrentPlanet { get; private set; }

	public void AssignPlayer(PlayerInfo player)
	{
		_player = player;
		_player.HUDBox = this;

		if (player.Name != null)
		{
			Delay.RunWhen(
				() => player.Name != null,
				() => PlayerName.text = player.Name.ToUpper());
		}
		
		InfoImage.sprite = MultiplayerHUDManager.UnknownSprite;

		new RequestStateResyncMessage().Send();
	}

	private void SetSprite(Sprite sprite)
	{
		if (InfoImage.sprite != sprite)
		{
			InfoImage.sprite = sprite;
		}
	}

	void Update()
	{
		var isDead = _player.IsDead;
		var inShip = _player.IsInShip;
		var currentSprite = InfoImage.sprite;

		if (isDead)
		{
			SetSprite(MultiplayerHUDManager.DeadSprite);
			return;
		}

		if (inShip)
		{
			SetSprite(MultiplayerHUDManager.ShipSprite);
			return;
		}

		var isUnknown = IsUnknown();

		if (isUnknown && !ServerSettingsManager.ServerAlwaysShowPlanetIcons)
		{
			SetSprite(MultiplayerHUDManager.UnknownSprite);
		}
		else
		{
			SetSprite(GetSprite(CurrentPlanet));
		}
	}

	private bool IsUnknown()
	{
		if (ServerSettingsManager.AlwaysShowPlanetIcons)
		{
			return false;
		}

		if (CurrentPlanet is "__UNKNOWN__" or "RingWorld" or "QuantumMoon")
		{
			return true;
		}

		// TODO : Get NH interference volumes / map restrictions working here

		return false;
	}

	public void UpdateIcon(string planet)
	{
		CurrentPlanet = planet;
	}

	private Sprite GetSprite(string planetName)
	{
		if (planetName == null)
		{
			return MultiplayerHUDManager.UnknownSprite;
		}

		if (MultiplayerHUDManager.Instance.PlanetToSprite.TryGetValue(planetName, out var sprite))
		{
			if (sprite != null)
			{
				return sprite;
			}
			else
			{
				DebugLog.DebugWrite($"Sprite for {planetName} is null.", MessageType.Warning);
			}
		}

		DebugLog.DebugWrite($"No sprite found for {planetName}", MessageType.Warning);
		return MultiplayerHUDManager.UnknownSprite;
	}
}
