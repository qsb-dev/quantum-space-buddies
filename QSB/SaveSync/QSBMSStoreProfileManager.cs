using Microsoft.Xbox;
using Newtonsoft.Json;
using QSB.Utility;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QSB.SaveSync;

public class QSBMSStoreProfileManager : IProfileManager
{
	private const string OW_SAVE_CONTAINER_NAME = "GameSave";
	private const string OW_GAME_SAVE_BLOB_NAME = "Outer Wilds Converted";
	private const string OW_GAME_SETTINGS_BLOB_NAME = "PCGameSettings";

	private static QSBMSStoreProfileManager _sharedInstance;
	private QSBX1SaveData _saveData;
	private const string c_containerName = "OuterWildsConnectedStorage";
	private GameSave _pendingGameSave;
	private SettingsSave _pendingSettingsSave;
	private GraphicSettings _pendingGfxSettingsSave;
	private string _pendingInputActionsSave = "";
	private JsonSerializer _jsonSerializer;
	private int _fileOpsBusyLocks;
	private bool _preInitialized;
	private bool _isLoadingGameBlob;
	private bool _isLoadingSettingsBlob;

	public static QSBMSStoreProfileManager SharedInstance
	{
		get
		{
			if (_sharedInstance == null)
			{
				_sharedInstance = new QSBMSStoreProfileManager();
			}

			return _sharedInstance;
		}
	}

	public GameSave currentProfileGameSave => _saveData.gameSave;
	public GameSave currentProfileMultiplayerGameSave => _saveData.gameMultSave;
	public SettingsSave currentProfileGameSettings => _saveData.settings;
	public GraphicSettings currentProfileGraphicsSettings => _saveData.gfxSettings;
	public string currentProfileInputJSON => _saveData.inputActionsJson;
	public bool isInitialized { get; private set; }
	public bool isBusyWithFileOps => _fileOpsBusyLocks > 0;
	public bool hasPendingSaveOperation => _pendingGameSave != null || _pendingSettingsSave != null || _pendingGfxSettingsSave != null || _pendingInputActionsSave != null;
	public bool saveSystemAvailable { get; private set; }
	public string userDisplayName => Gdk.Helpers.currentGamertag;

	public delegate void BrokenDataExistsEvent();

	public event BrokenDataExistsEvent OnBrokenDataExists;
	public event ProfileDataSavedEvent OnProfileDataSaved;
	public event ProfileReadDoneEvent OnProfileReadDone;
	public event ProfileSignInCompleteEvent OnProfileSignInComplete;
	public event ProfileSignInStartEvent OnProfileSignInStart;
	public event ProfileSignOutCompleteEvent OnProfileSignOutComplete;
	public event ProfileSignOutStartEvent OnProfileSignOutStart;

	public void Initialize()
	{
		if (!isInitialized)
		{
			Gdk.Helpers.SignIn();
			SpinnerUI.Show();
			Debug.Log("MSStoreProfileManager.Initialize");
			Gdk.Helpers.OnGameSaveSucceeded += OnGameSaveComplete;
			Gdk.Helpers.OnGameSaveFailed += OnGameSaveFailed;
			Gdk.Helpers.OnGameSaveLoaded += OnGameSaveLoaded;
			Gdk.Helpers.OnGameSaveLoadFailed += OnGameSaveLoadFailed;
			Achievements.Init();
			var serializationBinder = new VersionDeserializationBinder();
			_jsonSerializer = new JsonSerializer
			{
				SerializationBinder = serializationBinder
			};
			isInitialized = true;
			return;
		}

		OnProfileSignInComplete?.Invoke(ProfileManagerSignInResult.COMPLETE);
		OnProfileReadDone?.Invoke();

		DebugLog.DebugWrite("INITIALIZED");
	}

	public void PreInitialize()
	{
		if (_preInitialized)
		{
			return;
		}

		_fileOpsBusyLocks = 0;
		_pendingGameSave = null;
		_pendingSettingsSave = null;
		_pendingGfxSettingsSave = null;
		_pendingInputActionsSave = null;
		_preInitialized = true;
	}

	public void InvokeProfileSignInComplete() =>
		OnProfileSignInComplete?.Invoke(ProfileManagerSignInResult.COMPLETE);

	public void InvokeSaveSetupComplete()
	{
		saveSystemAvailable = true;
		_isLoadingGameBlob = true;
		_saveData = new QSBX1SaveData();
		LoadGame(OW_GAME_SAVE_BLOB_NAME);
	}

	public void InitializeForEditor() { }

	public void SaveGame(GameSave gameSave, SettingsSave settSave, GraphicSettings gfxSettings, string inputJSON)
	{
		DebugLog.DebugWrite("MSStoreProfileManager.SaveGame");
		if (isBusyWithFileOps || LoadManager.IsBusy())
		{
			_pendingGameSave = gameSave;
			_pendingSettingsSave = settSave;
			_pendingGfxSettingsSave = gfxSettings;
			_pendingInputActionsSave = inputJSON;
			return;
		}

		var gameSaveData = new QSBX1SaveData();
		var settingsSaveData = new QSBX1SaveData();
		var saveGameSave = false;
		if (gameSave != null)
		{
			saveGameSave = true;

			if (QSBCore.IsInMultiplayer)
			{
				_saveData.gameMultSave = gameSave;
				gameSaveData.gameMultSave = gameSave;
			}
			else
			{
				_saveData.gameSave = gameSave;
				gameSaveData.gameSave = gameSave;
			}
		}

		var saveGameSettings = false;
		if (settSave != null)
		{
			saveGameSettings = true;
			_saveData.settings = settSave;
			settingsSaveData.settings = settSave;
		}
		else
		{
			settingsSaveData.settings = _saveData.settings;
		}

		if (gfxSettings != null)
		{
			saveGameSettings = true;
			_saveData.gfxSettings = gfxSettings;
			settingsSaveData.gfxSettings = gfxSettings;
		}
		else
		{
			settingsSaveData.gfxSettings = _saveData.gfxSettings;
		}

		if (!string.IsNullOrEmpty(inputJSON))
		{
			saveGameSettings = true;
			_saveData.inputActionsJson = inputJSON;
			settingsSaveData.inputActionsJson = inputJSON;
		}
		else if (!string.IsNullOrEmpty(_saveData.inputActionsJson))
		{
			settingsSaveData.inputActionsJson = _saveData.inputActionsJson;
		}
		else
		{
			settingsSaveData.inputActionsJson = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
		}

		if (saveGameSave)
		{
			WriteSaveToStorage(gameSaveData, OW_GAME_SAVE_BLOB_NAME);
		}

		if (saveGameSettings)
		{
			WriteSaveToStorage(settingsSaveData, OW_GAME_SETTINGS_BLOB_NAME);
		}
	}

	private void LoadGame(string blobName)
	{
		DebugLog.DebugWrite($"LoadGame blobName:{blobName}");
		_fileOpsBusyLocks++;
		Gdk.Helpers.LoadSaveData(blobName);
	}

	private void WriteSaveToStorage(QSBX1SaveData saveData, string blobName)
	{
		DebugLog.DebugWrite("Saving to storage: " + blobName);
		_fileOpsBusyLocks++;
		var memoryStream = new MemoryStream();
		using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(memoryStream)))
		{
			_jsonSerializer.Serialize(jsonWriter, saveData);
		}

		var buffer = memoryStream.GetBuffer();
		Gdk.Helpers.Save(buffer, blobName);
	}

	public void PerformPendingSaveOperation()
	{
		if (!isBusyWithFileOps && !LoadManager.IsBusy())
		{
			SaveGame(_pendingGameSave, _pendingSettingsSave, _pendingGfxSettingsSave, _pendingInputActionsSave);
			_pendingGameSave = null;
			_pendingSettingsSave = null;
			_pendingGfxSettingsSave = null;
			_pendingInputActionsSave = null;
		}
	}

	private void OnGameSaveComplete(object sender, string blobName)
	{
		_fileOpsBusyLocks--;
		DebugLog.DebugWrite("[GDK] save to blob " + blobName + " complete");
	}

	private void OnGameSaveFailed(object sender, string blobName)
	{
		_fileOpsBusyLocks--;
		DebugLog.DebugWrite("[GDK] save to blob " + blobName + " failed");
	}

	private void OnGameSaveLoaded(object sender, string blobName, GameSaveLoadedArgs saveData)
	{
		_fileOpsBusyLocks--;
		DebugLog.DebugWrite("[GDK] save file load complete! blob name: " + blobName);
		var memoryStream = new MemoryStream(saveData.Data);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		using (var jsonTextReader = new JsonTextReader(new StreamReader(memoryStream)))
		{
			var tempSaveData = _jsonSerializer.Deserialize<QSBX1SaveData>(jsonTextReader);
			if (_isLoadingGameBlob)
			{
				if (tempSaveData != null)
				{
					if (tempSaveData.gameSave == null)
					{
						DebugLog.DebugWrite("[GDK] tempSaveData.gameSave is null (oh no)");
					}

					if (tempSaveData.gameMultSave == null)
					{
						DebugLog.DebugWrite("[GDK] tempSaveData.gameMultSave is null (oh no)");
					}

					_saveData.gameSave = tempSaveData.gameSave ?? new GameSave();
					_saveData.gameMultSave = tempSaveData.gameMultSave ?? new GameSave();
				}
				else
				{
					DebugLog.DebugWrite("[GDK] tempSaveData is null (oh no)");
					_saveData.gameSave = new GameSave();
					_saveData.gameMultSave = new GameSave();
				}
			}
			else
			{
				if (tempSaveData != null)
				{
					_saveData.gfxSettings = tempSaveData.gfxSettings ?? new GraphicSettings(true);
					_saveData.settings = tempSaveData.settings ?? new SettingsSave();
					_saveData.inputActionsJson = tempSaveData.inputActionsJson ?? ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
				}
				else
				{
					_saveData.gfxSettings = new GraphicSettings(true);
					_saveData.settings = new SettingsSave();
					_saveData.inputActionsJson = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
				}

				DebugLog.DebugWrite(string.Format("after settings load, _saveData.gameSave is null: {0}", _saveData.gameSave == null));
				DebugLog.DebugWrite(string.Format("_saveData loopCount: {0}", _saveData.gameSave.loopCount));
			}
		}

		if (_isLoadingGameBlob)
		{
			_isLoadingGameBlob = false;
			LoadGame(OW_GAME_SETTINGS_BLOB_NAME);
			_isLoadingSettingsBlob = true;
			return;
		}

		if (_isLoadingSettingsBlob)
		{
			_isLoadingSettingsBlob = false;
			OnProfileReadDone?.Invoke();
			DebugLog.DebugWrite("LOADED SETTINGS BLOB");
		}
	}

	private void OnGameSaveLoadFailed(object sender, string blobName)
	{
		DebugLog.DebugWrite("OnGameSaveLoadFailed");
		_fileOpsBusyLocks--;
		if (_isLoadingGameBlob)
		{
			_saveData.gameSave = new GameSave();
			SaveGame(_saveData.gameSave, null, null, null);
			_isLoadingGameBlob = false;
			LoadGame(OW_GAME_SETTINGS_BLOB_NAME);
			_isLoadingSettingsBlob = true;
			return;
		}

		if (_isLoadingSettingsBlob)
		{
			_saveData.settings = new SettingsSave();
			_saveData.gfxSettings = new GraphicSettings(true);
			_saveData.inputActionsJson = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
			SaveGame(null, _saveData.settings, _saveData.gfxSettings, _saveData.inputActionsJson);
			_isLoadingSettingsBlob = false;
			OnProfileReadDone?.Invoke();
			DebugLog.DebugWrite("LOADING SETTINGS BLOB - FROM FAILED GAME LOAD");
		}
	}

	[Serializable]
	public class QSBX1SaveData
	{
		[XmlElement("gameSave")]
		public GameSave gameSave;

		[XmlElement("gameMultSave")]
		[OptionalField(VersionAdded = 5)]
		public GameSave gameMultSave;

		[XmlElement("settings")]
		public SettingsSave settings;

		[XmlElement("gfxSettings")]
		[OptionalField(VersionAdded = 2)]
		public GraphicSettings gfxSettings;

		[OptionalField(VersionAdded = 3)]
		[NonSerialized]
		public InputRebindableData bindingSettings;

		[OptionalField(VersionAdded = 4)]
		public string inputActionsPacked;

		private InputActionAsset _inputActionsSave;

		[JsonIgnore]
		public string inputActionsJson
		{
			get => inputActionsPacked;
			set
			{
				inputActionsPacked = value;
				if (!string.IsNullOrEmpty(inputActionsPacked))
				{
					_inputActionsSave = InputActionAsset.FromJson(inputActionsPacked);
					return;
				}

				_inputActionsSave = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions;
			}
		}

		[JsonIgnore]
		public InputActionAsset inputActionsSave
		{
			get
			{
				if (_inputActionsSave == null && !string.IsNullOrEmpty(inputActionsPacked))
				{
					try
					{
						_inputActionsSave = InputActionAsset.FromJson(inputActionsPacked);
					}
					catch (Exception)
					{
						_inputActionsSave = null;
					}
				}

				return _inputActionsSave;
			}
		}

		[OnDeserializing]
		private void SetDefaultValuesOnDeserializing(StreamingContext context)
		{
			gfxSettings = null;
			bindingSettings = null;
			inputActionsPacked = null;
		}
	}
}
