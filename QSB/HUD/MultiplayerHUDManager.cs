using OWML.Common;
using QSB.HUD.Messages;
using QSB.Localization;
using QSB.Messaging;
using QSB.Player;
using QSB.ServerSettings;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QSB.HUD;

public class MultiplayerHUDManager : MonoBehaviour, IAddComponentOnStart
{
	public static MultiplayerHUDManager Instance;

	private Transform _playerList;
	private Transform _textChat;
	private InputField _inputField;
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
	public static Sprite Ringworld;
	public static Sprite QuantumMoon;

	public static readonly ListStack<string> HUDIconStack = new(true);

	public class ChatEvent : UnityEvent<string, uint> { }
	public static readonly ChatEvent OnChatMessageEvent = new();

	public Dictionary<string, Sprite> PlanetToSprite = null;

	private void Start()
	{
		Instance = this;

		GlobalMessenger.AddListener(OWEvents.WakeUp, OnWakeUp);

		QSBPlayerManager.OnAddPlayer += OnAddPlayer;
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;

		UnknownSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_unknown.png");
		DeadSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_dead.png");
		ShipSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_ship.png");
		SpaceSprite = QSBCore.HUDAssetBundle.LoadAsset<Sprite>("Assets/MULTIPLAYER_UI/playerbox_space.png");
	}

	private const int LINE_COUNT = 10;
	private const int CHAR_COUNT = 33;
	private const float FADE_DELAY = 5f;
	private const float FADE_TIME = 2f;

	private bool _writingMessage;
	private readonly (string msg, Color color)[] _lines = new (string msg, Color color)[LINE_COUNT];
	// this should really be a deque, but eh
	private readonly ListStack<(string msg, Color color)> _messages = new(false);
	private float _lastMessageTime;

	// this just exists so i can patch this in my tts addon
	// perks of being a qsb dev :-)
	public void WriteSystemMessage(string message, Color color)
	{
		WriteMessage($"QSB: {message}", color);
		OnChatMessageEvent.Invoke(message, uint.MaxValue);
	}

	public void WriteMessage(string message, Color color)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		/* Tricky problem to solve.
		 * - 11 available lines for text to fit onto
		 * - Each line can be max 41 characters
		 * - Newest messages appear at the bottom, and get pushed up by newer messages.
		 * - Messages can use several lines.
		 * 
		 * From newest to oldest message, work out how many lines it needs
		 * and set the lines correctly bottom-up.
		 */

		_lastMessageTime = Time.time;

		_messages.Push((message, color));

		if (_messages.Count > LINE_COUNT)
		{
			_messages.PopFromBack();
		}

		var currentLineIndex = LINE_COUNT - 1;

		foreach (var msg in _messages.Reverse())
		{
			var characterCount = msg.msg.Length;
			var linesNeeded = Mathf.CeilToInt((float)characterCount / CHAR_COUNT);
			var chunk = 0;
			for (var i = linesNeeded - 1; i >= 0; i--)
			{
				if (currentLineIndex - i < 0)
				{
					chunk++;
					continue;
				}

				var chunkString = string.Concat(msg.msg.Skip(CHAR_COUNT * chunk).Take(CHAR_COUNT));
				_lines[currentLineIndex - i] = (chunkString, msg.color);
				chunk++;
			}

			currentLineIndex -= linesNeeded;

			if (currentLineIndex < 0)
			{
				break;
			}
		}

		var finalText = "";
		foreach (var line in _lines)
		{
			var msgColor = ColorUtility.ToHtmlStringRGBA(line.color);
			var msg = $"<color=#{msgColor}>{line.msg}</color>";

			if (line == default)
			{
				finalText += Environment.NewLine;
			}
			else if (line.msg.Length == CHAR_COUNT + 1)
			{
				finalText += msg;
			}
			else
			{
				finalText += $"{msg}{Environment.NewLine}";
			}
		}

		_textChat.Find("Messages").Find("Message").GetComponent<Text>().text = finalText;

		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			var audioController = Locator.GetPlayerAudioController();
			audioController.PlayNotificationTextScrolling();
			Delay.RunFramesLater(10, () => audioController.StopNotificationTextScrolling());
		}

		_textChat.GetComponent<CanvasGroup>().alpha = 1;
	}

	ListStack<string> previousMessages = new(true);

	private void Update()
	{
		if (!QSBWorldSync.AllObjectsReady || _playerList == null)
		{
			return;
		}

		_playerList.gameObject.SetActive(ServerSettingsManager.ShowExtraHUD);

		var inSuit = Locator.GetPlayerSuit().IsWearingHelmet();

		if ((OWInput.IsNewlyPressed(InputLibrary.enter, InputMode.Character) || (Keyboard.current[Key.Slash].wasPressedThisFrame && OWInput.IsInputMode(InputMode.Character)))
			&& !_writingMessage && inSuit && QSBCore.TextChatInput)
		{
			OWInput.ChangeInputMode(InputMode.KeyboardInput);
			_writingMessage = true;
			_inputField.ActivateInputField();
			_textChat.GetComponent<CanvasGroup>().alpha = 1;

			if (Keyboard.current[Key.Slash].wasPressedThisFrame)
			{
				Delay.RunNextFrame(() => _inputField.text = "/");
			}
		}

		if (Keyboard.current[Key.UpArrow].wasPressedThisFrame && _writingMessage)
		{
			var currentText = _inputField.text;

			if (previousMessages.Contains(currentText))
			{
				var index = previousMessages.IndexOf(currentText);

				if (index == 0)
				{
					return;
				}

				_inputField.text = previousMessages[index - 1];
			}
			else
			{
				_inputField.text = previousMessages.Last();
			}
		}

		if (OWInput.IsNewlyPressed(InputLibrary.enter, InputMode.KeyboardInput) && _writingMessage)
		{
			OWInput.RestorePreviousInputs();
			_writingMessage = false;
			_inputField.DeactivateInputField();

			var message = _inputField.text;
			_inputField.text = "";
			message = message.Replace("\n", "").Replace("\r", "");

			previousMessages.Push(message);

			if (QSBCore.DebugSettings.DebugMode && CommandInterpreter.InterpretCommand(message))
			{
				return;
			}
			
			message = $"{QSBPlayerManager.LocalPlayer.Name}: {message}";
			new ChatMessage(message, Color.white).Send();
		}

		if (OWInput.IsNewlyPressed(InputLibrary.escape, InputMode.KeyboardInput) && _writingMessage)
		{
			OWInput.RestorePreviousInputs();
			_writingMessage = false;
		}

		if (_writingMessage)
		{
			_lastMessageTime = Time.time;
		}

		if (!_writingMessage
			&& Time.time > _lastMessageTime + FADE_DELAY
			&& Time.time < _lastMessageTime + FADE_DELAY + FADE_TIME + 1)
		{
			var difference = Time.time - (_lastMessageTime + FADE_DELAY);
			var alpha = Mathf.Lerp(1, 0, difference / FADE_TIME);
			_textChat.GetComponent<CanvasGroup>().alpha = alpha;
		}
	}

	public void CreatePlanetToSprite()
	{
		PlanetToSprite = new Dictionary<string, Sprite>()
		{
			{ "__SHIP__", MultiplayerHUDManager.ShipSprite },
			{ "__DEAD__", MultiplayerHUDManager.DeadSprite },
			{ "__SPACE__", MultiplayerHUDManager.SpaceSprite },
			{ "__UNKNOWN__", MultiplayerHUDManager.UnknownSprite },
			{ nameof(AstroObject.Name.CaveTwin), MultiplayerHUDManager.CaveTwin },
			{ nameof(AstroObject.Name.TowerTwin), MultiplayerHUDManager.TowerTwin },
			{ nameof(AstroObject.Name.TimberHearth), MultiplayerHUDManager.TimberHearth },
			{ nameof(AstroObject.Name.TimberMoon), MultiplayerHUDManager.Attlerock },
			{ nameof(AstroObject.Name.BrittleHollow), MultiplayerHUDManager.BrittleHollow },
			{ nameof(AstroObject.Name.VolcanicMoon), MultiplayerHUDManager.HollowsLantern },
			{ nameof(AstroObject.Name.GiantsDeep), MultiplayerHUDManager.GiantsDeep },
			{ nameof(AstroObject.Name.DarkBramble), MultiplayerHUDManager.DarkBramble },
			{ nameof(AstroObject.Name.Comet), MultiplayerHUDManager.Interloper },
			{ nameof(AstroObject.Name.WhiteHole), MultiplayerHUDManager.WhiteHole },
			{ nameof(AstroObject.Name.RingWorld), MultiplayerHUDManager.Ringworld },
			{ nameof(AstroObject.Name.QuantumMoon), MultiplayerHUDManager.QuantumMoon },
		};
	}

	private string AstroObjectNameToStringID(AstroObject.Name name) => name switch
	{
		AstroObject.Name.TimberHearth => "TIMBER_HEARTH",
		AstroObject.Name.DreamWorld => "DREAMWORLD",
		AstroObject.Name.RingWorld => "RINGWORLD",
		AstroObject.Name.GiantsDeep => "GIANTS_DEEP",
		AstroObject.Name.VolcanicMoon => "VOLCANIC_MOON",
		AstroObject.Name.BrittleHollow => "BRITTLE_HOLLOW",
		AstroObject.Name.WhiteHole => "WHITE_HOLE",
		AstroObject.Name.ProbeCannon => "ORBITAL_PROBE_CANNON",
		AstroObject.Name.TimberMoon => "TIMBER_MOON",
		AstroObject.Name.QuantumMoon => "QUANTUM_MOON",
		AstroObject.Name.Eye => "EYE_OF_THE_UNIVERSE",
		AstroObject.Name.SunStation => "SUN_STATION",
		AstroObject.Name.Sun => "SUN",
		AstroObject.Name.DarkBramble => "DARK_BRAMBLE",
		AstroObject.Name.WhiteHoleTarget => "WHITE_HOLE_TARGET",
		AstroObject.Name.Comet => "COMET",
		AstroObject.Name.TowerTwin => "TOWER_TWIN",
		AstroObject.Name.CaveTwin => "CAVE_TWIN",
		_ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
	};

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
			rect.localRotation = Quaternion.identity;
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

		var shipLogMapMode = Resources.FindObjectsOfTypeAll<ShipLogMapMode>()[0];
		var allShipLogAstroObjects = new List<ShipLogAstroObject>();
		allShipLogAstroObjects.AddRange(shipLogMapMode._topRow);
		allShipLogAstroObjects.AddRange(shipLogMapMode._midRow);
		allShipLogAstroObjects.AddRange(shipLogMapMode._bottomRow);

		Sprite GetSprite(AstroObject.Name name)
		{
			var stringID = AstroObjectNameToStringID(name);
			var shipLogAstroObject = allShipLogAstroObjects.FirstOrDefault(x => x._id == stringID);

			if (shipLogAstroObject == null)
			{
				DebugLog.DebugWrite($"Couldn't find ShipLogAstroObject for {name} ({stringID})", MessageType.Error);
				return null;
			}

			return shipLogAstroObject._image.sprite;
		}

		CaveTwin = GetSprite(AstroObject.Name.CaveTwin);
		TowerTwin = GetSprite(AstroObject.Name.TowerTwin);
		TimberHearth = GetSprite(AstroObject.Name.TimberHearth);
		Attlerock = GetSprite(AstroObject.Name.TimberMoon);
		BrittleHollow = GetSprite(AstroObject.Name.BrittleHollow);
		HollowsLantern = GetSprite(AstroObject.Name.VolcanicMoon);
		GiantsDeep = GetSprite(AstroObject.Name.GiantsDeep);
		DarkBramble = GetSprite(AstroObject.Name.DarkBramble);
		Interloper = GetSprite(AstroObject.Name.Comet);
		WhiteHole = GetSprite(AstroObject.Name.WhiteHole);
		Ringworld = GetSprite(AstroObject.Name.RingWorld);
		QuantumMoon = GetSprite(AstroObject.Name.QuantumMoon);

		CreatePlanetToSprite();

		CreateTrigger("TowerTwin_Body/Sector_TowerTwin", AstroObject.Name.TowerTwin);
		CreateTrigger("CaveTwin_Body/Sector_CaveTwin", AstroObject.Name.CaveTwin);
		CreateTrigger("TimberHearth_Body/Sector_TH", AstroObject.Name.TimberHearth);
		CreateTrigger("Moon_Body/Sector_THM", AstroObject.Name.TimberMoon);
		CreateTrigger("BrittleHollow_Body/Sector_BH", AstroObject.Name.BrittleHollow);
		CreateTrigger("VolcanicMoon_Body/Sector_VM", AstroObject.Name.VolcanicMoon);
		CreateTrigger("GiantsDeep_Body/Sector_GD", AstroObject.Name.GiantsDeep);
		CreateTrigger("DarkBramble_Body/Sector_DB", AstroObject.Name.DarkBramble);
		CreateTrigger("Comet_Body/Sector_CO", AstroObject.Name.Comet);
		CreateTrigger("WhiteHole_Body/Sector_WhiteHole", AstroObject.Name.WhiteHole);
		CreateTrigger("RingWorld_Body/Sector_RingWorld", AstroObject.Name.RingWorld); // TODO : this doesnt work????
		CreateTrigger("QuantumMoon_Body/Sector_QuantumMoon", AstroObject.Name.QuantumMoon);

		HUDIconStack.Clear();
		HUDIconStack.Push("__SPACE__");

		HUDIconStack.Push("TimberHearth");
		new PlanetMessage("TimberHearth").Send();

		_textChat = multiplayerGroup.transform.Find("TextChat");
		var inputFieldGO = _textChat.Find("InputField");
		_inputField = inputFieldGO.GetComponent<InputField>();
		_inputField.text = "";
		_inputField.characterLimit = 256;
		_textChat.Find("Messages").Find("Message").GetComponent<Text>().text = "";
		_lines.Clear();
		_messages.Clear();
		_textChat.GetComponent<CanvasGroup>().alpha = 0;
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
					DebugLog.ToConsole($"Error - {player.PlayerId}'s RulesetDetector is null.", MessageType.Error);
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
				player.MinimapPlayerMarker.GetComponent<MeshRenderer>().enabled = true;
			}
			else
			{
				player.MinimapPlayerMarker.localPosition = Vector3.zero;
				player.MinimapPlayerMarker.localRotation = Quaternion.identity;
				player.MinimapPlayerMarker.GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}

	public void HideMinimap(Minimap minimap)
	{
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			if (player.MinimapPlayerMarker == null)
			{
				continue;
			}

			player.MinimapPlayerMarker.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	public void ShowMinimap(Minimap minimap)
	{
		foreach (var player in QSBPlayerManager.PlayerList)
		{
			if (player.MinimapPlayerMarker == null)
			{
				continue;
			}

			player.MinimapPlayerMarker.GetComponent<MeshRenderer>().enabled = true;
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
		Destroy(player.HUDBox?.gameObject);
		Destroy(player.MinimapPlayerMarker);

		WriteSystemMessage(string.Format(QSBLocalization.Current.PlayerLeftTheGame, player.Name), Color.yellow);
	}

	public static PlanetTrigger CreateTrigger(string parentPath, AstroObject.Name name)
		=> CreateTrigger(Find(parentPath), Enum.GetName(typeof(AstroObject.Name), name));

	public static PlanetTrigger CreateTrigger(GameObject parent, string name)
	{
		if (parent == null)
		{
			return null;
		}

		var triggerGO = parent.FindChild("HUD_PLANET_TRIGGER");
		if (triggerGO != null)
		{
			var trigger = triggerGO.GetAddComponent<PlanetTrigger>();
			trigger.PlanetID = name;
			return trigger;
		}
		else
		{
			triggerGO = new GameObject("HUD_PLANET_TRIGGER");
			triggerGO.transform.SetParent(parent.transform, false);
			triggerGO.SetActive(false);
			var trigger = triggerGO.AddComponent<PlanetTrigger>();
			trigger.PlanetID = name;
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
