using QSB.Player;
using QSB.Utility;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QSB.HUD;

public class PlayerBox : MonoBehaviour
{
	public Text PlayerName;
	public Image InfoImage;

	private PlayerInfo _player;
	private bool _planetIconOverride;

	public HUDIcon PlanetIcon { get; private set; }

	public void AssignPlayer(PlayerInfo player)
	{
		_player = player;
		_player.HUDBox = this;

		if (player.Name != null)
		{
			PlayerName.text = player.Name.ToUpper();
		}
		
		InfoImage.sprite = MultiplayerHUDManager.UnknownSprite;
	}

	public void OnDeath()
	{
		InfoImage.sprite = MultiplayerHUDManager.DeadSprite;
		_planetIconOverride = true;
	}

	public void OnRespawn()
	{
		InfoImage.sprite = MultiplayerHUDManager.ShipSprite;
		_planetIconOverride = true; // still in ship
	}

	public void OnEnterShip()
	{
		InfoImage.sprite = MultiplayerHUDManager.ShipSprite;
		_planetIconOverride = true;
	}

	public void OnExitShip()
	{
		_planetIconOverride = false;
		InfoImage.sprite = SpriteFromEnum(PlanetIcon);
	}

	public void UpdateIcon(HUDIcon icon)
	{
		PlanetIcon = icon;

		if (!_planetIconOverride)
		{
			InfoImage.sprite = SpriteFromEnum(PlanetIcon);
		}
	}

	public Sprite SpriteFromEnum(HUDIcon icon) => icon switch
	{
		HUDIcon.SHIP => MultiplayerHUDManager.ShipSprite,
		HUDIcon.DEAD => MultiplayerHUDManager.DeadSprite,
		HUDIcon.SPACE => MultiplayerHUDManager.SpaceSprite,
		HUDIcon.CAVE_TWIN => MultiplayerHUDManager.CaveTwin,
		HUDIcon.TOWER_TWIN => MultiplayerHUDManager.TowerTwin,
		HUDIcon.TIMBER_HEARTH => MultiplayerHUDManager.TimberHearth,
		HUDIcon.ATTLEROCK => MultiplayerHUDManager.Attlerock,
		HUDIcon.BRITTLE_HOLLOW => MultiplayerHUDManager.BrittleHollow,
		HUDIcon.HOLLOWS_LANTERN => MultiplayerHUDManager.HollowsLantern,
		HUDIcon.GIANTS_DEEP => MultiplayerHUDManager.GiantsDeep,
		HUDIcon.DARK_BRAMBLE => MultiplayerHUDManager.DarkBramble,
		HUDIcon.INTERLOPER => MultiplayerHUDManager.Interloper,
		HUDIcon.WHITE_HOLE => MultiplayerHUDManager.WhiteHole,
		_ => MultiplayerHUDManager.UnknownSprite,
	};
}
