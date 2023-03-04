using QSB.HUD.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.ServerSettings;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QSB.HUD;

internal class MultiplayerHUDManager : MonoBehaviour, IAddComponentOnStart
{
	public static MultiplayerHUDManager Instance;

	private Transform _playerList;
	private Material _markerMaterial;

	public static Sprite UnknownSprite;
	public static Sprite DeadSprite;
	public static Sprite SpaceSprite;
	public static Sprite ShipSprite;
	public static Sprite TimberHearth;
	public static Sprite Attlerock;
	public static Sprite CaveTwin;
	public static Sprite TowerTwin;
	public static Sprite BrittleHollow;
	public static Sprite HollowsLantern;
	public static Sprite GiantsDeep;
	public static Sprite DarkBramble;
	public static Sprite Interloper;
	public static Sprite WhiteHole;

	public static ListStack<HUDIcon> HUDIconStack = new();

	private void Start()
	{
		Instance = this;

		GlobalMessenger.AddListener(OWEvents.WakeUp, OnWakeUp);

		QSBPlayerManager.OnAddPlayer += OnAddPlayer;
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;

		UnknownSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_unknown.png");
		DeadSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_dead.png");
		ShipSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_ship.png");
		CaveTwin = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_cavetwin.png");
		TowerTwin = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_towertwin.png");
		TimberHearth = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_timberhearth.png");
		Attlerock = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_attlerock.png");
		BrittleHollow = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_brittlehollow.png");
		HollowsLantern = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_hollowslantern.png");
		GiantsDeep = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_giantsdeep.png");
		DarkBramble = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_darkbramble.png");
		Interloper = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_interloper.png");
		WhiteHole = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_whitehole.png");
		SpaceSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_space.png");
	}

	private void Update()
	{
		if (!QSBWorldSync.AllObjectsReady || _playerList == null)
		{
			return;
		}

		_playerList.gameObject.SetActive(ServerSettingsManager.ShowExtraHUD);
	}

	private void OnWakeUp()
	{
		var hudController = Locator.GetPlayerCamera().transform.Find("Helmet").Find("HUDController").GetComponent<HUDCanvas>();
		var hudCamera = hudController._hudCamera;
		var hudCanvas = hudCamera.transform.parent.Find("UICanvas");

		var multiplayerGroup = Instantiate(QSBCore.HUDAssetBundle.LoadAsset<GameObject>("assets/Prefabs/multiplayergroup.prefab"));

		Delay.RunNextFrame(() =>
		{
			// no idea why this has to be next frame, but it does
			multiplayerGroup.transform.parent = hudCanvas;
			multiplayerGroup.transform.localPosition = Vector3.zero;
			var rect = multiplayerGroup.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(1, 0.5f);
			rect.anchorMax = new Vector2(1, 0.5f);
			rect.sizeDelta = new Vector2(100, 100);
			rect.anchoredPosition3D = new Vector3(-267, 0, 0);
			rect.localRotation = Quaternion.Euler(0, 55, 0);
			rect.localScale = Vector3.one;
		});

		_playerList = multiplayerGroup.transform.Find("PlayerList");

		foreach (var player in QSBPlayerManager.PlayerList)
		{
			AddBox(player);

			foreach (var item in QSBWorldSync.GetUnityObjects<Minimap>())
			{
				AddMinimapMarker(player, item);
			}
		}

		CreateTrigger("TowerTwin_Body/Sector_TowerTwin", HUDIcon.TOWER_TWIN);
		CreateTrigger("CaveTwin_Body/Sector_CaveTwin", HUDIcon.CAVE_TWIN);
		CreateTrigger("TimberHearth_Body/Sector_TH", HUDIcon.TIMBER_HEARTH);
		CreateTrigger("Moon_Body/Sector_THM", HUDIcon.ATTLEROCK);
		CreateTrigger("BrittleHollow_Body/Sector_BH", HUDIcon.BRITTLE_HOLLOW);
		CreateTrigger("VolcanicMoon_Body/Sector_VM", HUDIcon.HOLLOWS_LANTERN);
		CreateTrigger("GiantsDeep_Body/Sector_GD", HUDIcon.GIANTS_DEEP);
		CreateTrigger("DarkBramble_Body/Sector_DB", HUDIcon.DARK_BRAMBLE);
		CreateTrigger("Comet_Body/Sector_CO", HUDIcon.INTERLOPER);
		CreateTrigger("WhiteHole_Body/Sector_WhiteHole", HUDIcon.WHITE_HOLE);

		HUDIconStack.Clear();
		HUDIconStack.Push(HUDIcon.SPACE);
		HUDIconStack.Push(HUDIcon.TIMBER_HEARTH);

		new PlanetMessage(HUDIcon.TIMBER_HEARTH).Send();
	}

	public void UpdateMinimapMarkers(Minimap minimap)
	{
		var localRuleset = Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset();

		foreach (var player in QSBPlayerManager.PlayerList)
		{
			if (player.IsDead || player.IsLocalPlayer || !player.IsReady)
			{
				continue;
			}

			if (player.RulesetDetector == null)
			{
				if (player.Body != null)
				{
					DebugLog.ToConsole($"Error - {player.PlayerId}'s RulesetDetector is null.", OWML.Common.MessageType.Error);
				}

				continue;
			}

			if (player.RulesetDetector.GetPlanetoidRuleset() == null
				|| player.RulesetDetector.GetPlanetoidRuleset() != localRuleset)
			{
				continue;
			}

			if (player.MinimapPlayerMarker == null)
			{
				continue;
			}

			if (ServerSettingsManager.ShowExtraHUD)
			{
				player.MinimapPlayerMarker.localPosition = GetLocalMapPosition(player, minimap);
				player.MinimapPlayerMarker.LookAt(minimap._globeMeshTransform, minimap._globeMeshTransform.up);
			}
			else
			{
				player.MinimapPlayerMarker.localPosition = Vector3.zero;
				player.MinimapPlayerMarker.localRotation = Quaternion.identity;
			}	
		}
	}

	private void AddMinimapMarker(PlayerInfo player, Minimap minimap)
	{
		player.MinimapPlayerMarker = Instantiate(minimap._probeMarkerTransform);
		player.MinimapPlayerMarker.parent = minimap._probeMarkerTransform.parent;
		player.MinimapPlayerMarker.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		player.MinimapPlayerMarker.localPosition = Vector3.zero;
		player.MinimapPlayerMarker.localRotation = Quaternion.identity;

		if (_markerMaterial == null)
		{
			var playerMinimap = QSBWorldSync.GetUnityObjects<Minimap>().First(x => x.name == "Minimap_Root");
			_markerMaterial = Instantiate(playerMinimap._probeMarkerTransform.GetComponent<MeshRenderer>().material);
			_markerMaterial.color = new Color32(218, 115, 255, 255);
		}

		player.MinimapPlayerMarker.GetComponent<MeshRenderer>().material = _markerMaterial;
	}

	private void AddBox(PlayerInfo player)
	{
		var box = Instantiate(QSBCore.HUDAssetBundle.LoadAsset<GameObject>("assets/Prefabs/playerbox.prefab"));
		box.transform.parent = _playerList;
		box.transform.localScale = new Vector3(1, 1, 1);
		box.transform.localPosition = Vector3.zero;
		box.transform.localRotation = Quaternion.identity;

		var boxScript = box.GetComponent<PlayerBox>();
		boxScript.AssignPlayer(player);
	}

	private Vector3 GetLocalMapPosition(PlayerInfo player, Minimap minimap)
	{
		return Vector3.Scale(
			player.RulesetDetector.GetPlanetoidRuleset().transform.InverseTransformPoint(player.Body.transform.position).normalized * 0.51f,
			minimap._globeMeshTransform.localScale);
	}

	private void OnAddPlayer(PlayerInfo player)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		AddBox(player);

		foreach (var item in QSBWorldSync.GetUnityObjects<Minimap>())
		{
			AddMinimapMarker(player, item);
		}
	}

	private void OnRemovePlayer(PlayerInfo player)
	{
		Destroy(player.HUDBox.gameObject);
	}

	private PlanetTrigger CreateTrigger(string parentPath, HUDIcon icon)
		=> CreateTrigger(Find(parentPath), icon);

	private PlanetTrigger CreateTrigger(GameObject parent, HUDIcon icon)
	{
		if (parent == null)
		{
			return null;
		}

		var triggerGO = parent.FindChild("HUD_PLANET_TRIGGER");
		if (triggerGO != null)
		{
			var trigger = triggerGO.GetAddComponent<PlanetTrigger>();
			trigger.Icon = icon;
			return trigger;
		}
		else
		{
			triggerGO = new GameObject("HUD_PLANET_TRIGGER");
			triggerGO.transform.SetParent(parent.transform, false);
			triggerGO.SetActive(false);
			var trigger = triggerGO.AddComponent<PlanetTrigger>();
			trigger.Icon = icon;
			triggerGO.SetActive(true);
			return trigger;
		}
	}

	public static GameObject Find(string path)
	{
		var go = GameObject.Find(path);

		if (go == null)
		{
			// find inactive use root + transform.find
			var names = path.Split('/');
			var rootName = names[0];
			var root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(x => x.name == rootName);
			if (root == null)
			{
				return null;
			}

			var childPath = string.Join("/", names.Skip(1));
			go = root.FindChild(childPath);
		}

		return go;
	}
}