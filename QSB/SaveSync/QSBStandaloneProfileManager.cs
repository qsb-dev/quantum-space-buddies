using Newtonsoft.Json;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace QSB.SaveSync;

internal class QSBStandaloneProfileManager : IProfileManager
{
	private static QSBStandaloneProfileManager s_instance;

	private const string _saveDirectory = "/SteamSaves";
	private const string _backupDirectory = "/Backup";
	private const string _tempDirectory = "/Temp";
	private const string _gameSaveFilename = "data.owsave";
	private const string _gameSaveMultFilename = "data_mult.owsave";
	private const string _gameSettingsFilename = "player.owsett";
	private const string _gfxSettingsFilename = "graphics.owsett";
	private const string _legacyInputBindingSettingsFilename = "input.owsett";
	private const string _inputActionsSettingsFilename = "input_new.owsett";
	private const int _profileNameCharLimit = 16;

	private string _profilesPath;
	private string _profileTempPath;
	private string _profileBackupPath;
	private int _fileOpsBusyLocks;
	private GameSave _pendingGameSave;
	private SettingsSave _pendingSettingsSave;
	private GraphicSettings _pendingGfxSettingsSave;
	private string _pendingInputJSONSave = "";
	private BinaryFormatter _binaryFormatter;
	private JsonSerializer _jsonSerializer;

	public static QSBStandaloneProfileManager SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new QSBStandaloneProfileManager();
			}

			return s_instance;
		}
	}

	public GameSave currentProfileGameSave => currentProfile?.gameSave;

	public SettingsSave currentProfileGameSettings => currentProfile?.settingsSave;

	public GraphicSettings currentProfileGraphicsSettings => currentProfile?.graphicsSettings;

	public string currentProfileInputJSON => currentProfile?.inputJSON;

	public QSBProfileData currentProfile { get; private set; }

	public QSBProfileData mostRecentProfile
		=> profiles.OrderByDescending(profile => profile.lastModifiedTime).FirstOrDefault();

	public int profileNameCharacterLimit => _profileNameCharLimit;

	public List<QSBProfileData> profiles { get; private set; }

	public int numberOfProfiles => profiles.Count;

	public bool isInitialized => currentProfileGameSave != null;

	public bool isBusyWithFileOps => _fileOpsBusyLocks > 0;

	public bool hasPendingSaveOperation => _pendingGameSave != null
		|| _pendingSettingsSave != null
		|| _pendingGfxSettingsSave != null
		|| _pendingInputJSONSave != "";

	public int profileCharacterLimit => _profileNameCharLimit;

	public delegate void NoProfilesExistEvent();
	public delegate void BrokenDataExistsEvent();
	public delegate void BackupDataRestoredEvent();
	public delegate void UpdatePlayerProfilesEvent();

	public event NoProfilesExistEvent OnNoProfilesExist;
	public event BrokenDataExistsEvent OnBrokenDataExists;
	public event BackupDataRestoredEvent OnBackupDataRestored;
	public event UpdatePlayerProfilesEvent OnUpdatePlayerProfiles;
	public event ProfileSignInCompleteEvent OnProfileSignInComplete;
	public event ProfileReadDoneEvent OnProfileReadDone;
	public event ProfileDataSavedEvent OnProfileDataSaved;
	public event ProfileSignOutCompleteEvent OnProfileSignOutComplete;
	public event ProfileSignInStartEvent OnProfileSignInStart;
	public event ProfileSignOutStartEvent OnProfileSignOutStart;
	public event ControllerDisconnectedEvent OnControllerDisconnected;
	public event ControllerReconnectedEvent OnControllerReconnected;

	public void PreInitialize()
	{
		_fileOpsBusyLocks = 0;
		_pendingGameSave = null;
		_pendingSettingsSave = null;
		_pendingGfxSettingsSave = null;
		_pendingInputJSONSave = "";
	}

	public void Initialize()
	{
		_profilesPath = Application.persistentDataPath + _saveDirectory;
		_profileBackupPath = Application.persistentDataPath + _backupDirectory;
		_profileTempPath = Application.persistentDataPath + _tempDirectory;
		profiles = new List<QSBProfileData>();
		var versionDeserializationBinder = new VersionDeserializationBinder();
		_jsonSerializer = new JsonSerializer
		{
			SerializationBinder = versionDeserializationBinder
		};
		_binaryFormatter = new BinaryFormatter
		{
			Binder = versionDeserializationBinder
		};
		Achievements.Init();
		InitializeProfileData();
	}

	public void InitializeForEditor()
	{
		_profilesPath = Application.persistentDataPath + _saveDirectory;
		_profileBackupPath = Application.persistentDataPath + _backupDirectory;
		_profileTempPath = Application.persistentDataPath + _tempDirectory;
		profiles = new List<QSBProfileData>();
		var versionDeserializationBinder = new VersionDeserializationBinder();
		_jsonSerializer = new JsonSerializer
		{
			SerializationBinder = versionDeserializationBinder
		};
		_binaryFormatter = new BinaryFormatter
		{
			Binder = versionDeserializationBinder
		};
		MarkBusyWithFileOps(true);
		profiles.Clear();
		LoadProfiles();
		LoadSaveFilesFromProfiles();
		var flag = false;
		for (var i = 0; i < profiles.Count; i++)
		{
			if (profiles[i].profileName == "Debug")
			{
				currentProfile = profiles[i];
				flag = true;
				break;
			}
		}

		if (!flag)
		{
			TryCreateProfile("Debug");
		}

		MarkBusyWithFileOps(false);
		PlayerData.Init(currentProfileGameSave, currentProfileGameSettings, currentProfileGraphicsSettings, currentProfileInputJSON);
	}

	private void MarkBusyWithFileOps(bool isBusy)
	{
		if (isBusy)
		{
			_fileOpsBusyLocks++;
			return;
		}

		if (_fileOpsBusyLocks <= 0)
		{
			Debug.LogWarning("No File I/O lock to remove!");
			return;
		}

		_fileOpsBusyLocks--;
	}

	public void PerformPendingSaveOperation()
	{
		if (!isBusyWithFileOps && !LoadManager.IsBusy())
		{
			TrySaveProfile(currentProfile, _pendingGameSave, _pendingSettingsSave, _pendingGfxSettingsSave, _pendingInputJSONSave);
			_pendingGameSave = null;
			_pendingSettingsSave = null;
			_pendingGfxSettingsSave = null;
			_pendingInputJSONSave = "";
		}
	}

	public void SaveGame(GameSave gameSave, SettingsSave settSave, GraphicSettings graphicSettings, string inputBindings)
	{
		if (isBusyWithFileOps || LoadManager.IsBusy())
		{
			_pendingGameSave = gameSave;
			_pendingSettingsSave = settSave;
			_pendingGfxSettingsSave = graphicSettings;
			_pendingInputJSONSave = inputBindings;
			return;
		}

		TrySaveProfile(currentProfile, gameSave, settSave, graphicSettings, inputBindings);
	}

	private void InitializeProfileData()
	{
		LoadProfiles();
		currentProfile = mostRecentProfile;
		if (currentProfile != null)
		{
			LoadSaveFilesFromProfiles();
			return;
		}

		OnNoProfilesExist?.Invoke();
	}

	private void LoadSaveFilesFromProfiles()
	{
		MarkBusyWithFileOps(isBusy: true);
		foreach (var profile in profiles)
		{
			var path = _profilesPath + "/" + profile.profileName;
			GameSave saveData = null;
			GameSave multSaveData = null;
			SettingsSave settingsData = null;
			GraphicSettings graphicsData = null;
			var inputJSON = "";
			if (Directory.Exists(path))
			{
				Stream stream = null;
				var directoryInfo = new DirectoryInfo(path);
				profile.brokenSaveData = TryLoadSaveData(ref stream, _gameSaveFilename, directoryInfo, out saveData);
				profile.brokenMultSaveData = TryLoadSaveData(ref stream, _gameSaveMultFilename, directoryInfo, out multSaveData);
				profile.brokenSettingsData = TryLoadSaveData(ref stream, _gameSettingsFilename, directoryInfo, out settingsData);
				profile.brokenGfxSettingsData = TryLoadSaveData(ref stream, _gfxSettingsFilename, directoryInfo, out graphicsData);
				profile.brokenRebindingData = TryLoadInputBindingsSave(ref stream, directoryInfo, out inputJSON);
			}

			var profilePath = _profileBackupPath + "/" + profile.profileName;
			var savePath = profilePath + "/" + _gameSaveFilename;
			var multSavePath = profilePath + "/" + _gameSaveMultFilename;
			var settingsPath = profilePath + "/" + _gameSettingsFilename;
			var graphicsPath = profilePath + "/" + _gfxSettingsFilename;
			var inputsPath = profilePath + "/" + _inputActionsSettingsFilename;

			if (saveData == null)
			{
				profile.brokenSaveData = File.Exists(savePath);
				saveData = new GameSave();
				Debug.LogError("Could not find game save for " + profile.profileName);
			}

			if (multSaveData == null)
			{
				profile.brokenMultSaveData = File.Exists(multSavePath);
				multSaveData = new GameSave();
				Debug.LogError("Could not find multiplayer game save for " + profile.profileName);
			}

			if (settingsData == null)
			{
				profile.brokenSettingsData = File.Exists(settingsPath);
				settingsData = new SettingsSave();
				Debug.LogError("Could not find game settings for " + profile.profileName);
			}

			if (graphicsData == null)
			{
				profile.brokenGfxSettingsData = File.Exists(graphicsPath);
				graphicsData = new GraphicSettings(init: true);
				Debug.LogError("Could not find graphics settings for " + profile.profileName);
			}

			if (string.IsNullOrEmpty(inputJSON))
			{
				profile.brokenRebindingData = File.Exists(inputsPath);
				inputJSON = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
				Debug.LogError("Could not find input action settings for " + profile.profileName);
			}

			profile.gameSave = saveData;
			profile.multiplayerGameSave = multSaveData;
			profile.settingsSave = settingsData;
			profile.graphicsSettings = graphicsData;
			profile.inputJSON = inputJSON;
		}

		MarkBusyWithFileOps(isBusy: false);
		if (CurrentProfileHasBrokenData())
		{
			OnBrokenDataExists?.Invoke();
		}

		OnProfileReadDone?.Invoke();
	}

	private bool TryLoadSaveData<T>(ref Stream stream, string fileName, DirectoryInfo directoryInfo, out T saveData)
	{
		saveData = default;
		var flag = true;
		var files = directoryInfo.GetFiles(fileName);
		if (files.Length != 0)
		{
			stream = null;
			if (TryOpenFile(files[0].FullName, ref stream))
			{
				var jsonTextReader = new JsonTextReader(new StreamReader(stream));
				flag = !TryDeserializeJson<T>(jsonTextReader, out saveData);
				if (flag)
				{
					stream.Position = 0L;
					flag = !TryDeserializeBinary<T>(stream, out saveData);
				}

				jsonTextReader.Close();
			}
		}

		return flag;
	}

	private bool TryLoadInputBindingsSave(ref Stream stream, DirectoryInfo directoryInfo, out string inputJSON)
	{
		inputJSON = null;
		var result = true;
		var files = directoryInfo.GetFiles(_inputActionsSettingsFilename);
		if (files.Length != 0)
		{
			stream = null;
			if (TryOpenFile(files[0].FullName, ref stream))
			{
				result = !TryDeserializeJsonAsInputActionsData(stream, out inputJSON);
			}

			var stream2 = stream;
			if (stream2 != null)
			{
				stream2.Close();
			}
		}

		return result;
	}

	private bool TryOpenFile(string fullPath, ref Stream dataStream)
	{
		bool result;
		try
		{
			dataStream = File.Open(fullPath, FileMode.Open);
			result = true;
		}
		catch (Exception ex)
		{
			Debug.LogError("[" + ex.Message + "] Failed loading opening file " + fullPath);
			result = false;
		}

		return result;
	}

	private bool TryDeserializeBinary<T>(Stream dataStream, out T saveData)
	{
		bool result;
		try
		{
			saveData = default;
			saveData = (T)_binaryFormatter.Deserialize(dataStream);
			Debug.Log("Successfully read " + typeof(T).Name + " save data as binary");
			result = true;
		}
		catch (Exception ex)
		{
			saveData = default;
			Debug.LogError(string.Concat(new string[]
			{
				"[",
				ex.Message,
				"] Deserialization error for binary ",
				typeof(T).Name,
				" save data"
			}));
			result = false;
		}

		return result;
	}

	private bool TryDeserializeJson<T>(JsonTextReader jsonReader, out T rebindingData)
	{
		bool result;
		try
		{
			rebindingData = _jsonSerializer.Deserialize<T>(jsonReader);
			result = true;
		}
		catch (Exception)
		{
			rebindingData = default;
			Debug.LogWarning("Could not read " + typeof(T).Name + " save data as JSON, it might be in binary so giving that a try.");
			result = false;
		}

		return result;
	}

	private bool TryDeserializeJsonAsInputActionsData(Stream dataStream, out string inputJSON)
	{
		bool result;
		try
		{
			using var streamReader = new StreamReader(dataStream);
			var text = streamReader.ReadToEnd();
			inputJSON = text;
			Debug.Log("Successfully read Input Bindings save data as JSON");
			result = true;
		}
		catch (Exception ex)
		{
			inputJSON = null;
			Debug.LogError("[" + ex.Message + "] Deserialization error for Input Actions Save");
			result = false;
		}

		return result;
	}

	public bool CurrentProfileHasBrokenData()
	{
		if (currentProfile == null)
		{
			Debug.LogError("QSBStandaloneProfileManager.CurrentProfileHasBrokenData We should never get here outside of the Unity Editor");
			return false;
		}

		return currentProfile.brokenSaveData || currentProfile.brokenMultSaveData || currentProfile.brokenSettingsData || currentProfile.brokenGfxSettingsData || currentProfile.brokenRebindingData;
	}

	public bool BackupExistsForBrokenData()
	{
		var text = _profileBackupPath + "/" + currentProfile.profileName;
		var savePath = text + "/" + _gameSaveFilename;
		var multSavePath = text + "/" + _gameSaveMultFilename;
		var settingsPath = text + "/" + _gameSettingsFilename;
		var graphicsPath = text + "/" + _gfxSettingsFilename;
		var inputsPath = text + "/" + _inputActionsSettingsFilename;

		return (currentProfile.brokenSaveData && File.Exists(savePath))
			|| (currentProfile.brokenMultSaveData && File.Exists(multSavePath))
			|| (currentProfile.brokenSettingsData && File.Exists(settingsPath))
			|| (currentProfile.brokenGfxSettingsData && File.Exists(graphicsPath))
			|| (currentProfile.brokenRebindingData && File.Exists(inputsPath));
	}

	private void LoadProfiles()
	{
		MarkBusyWithFileOps(true);
		profiles.Clear();
		if (Directory.Exists(_profilesPath))
		{
			QSBProfileData profileData = null;
			Stream stream = null;
			var files = new DirectoryInfo(_profilesPath).GetFiles("*.owprofile");
			foreach (var fileInfo in files)
			{
				DebugLog.DebugWrite(fileInfo.Name);
				try
				{
					stream = null;
					stream = File.Open(fileInfo.FullName, FileMode.Open);
					var jsonTextReader = new JsonTextReader(new StreamReader(stream));
					try
					{
						profileData = _jsonSerializer.Deserialize<QSBProfileData>(jsonTextReader);
					}
					catch
					{
						stream.Position = 0L;
						profileData = (QSBProfileData)_binaryFormatter.Deserialize(stream);
					}
					finally
					{
						jsonTextReader.Close();
					}

					if (profileData == null)
					{
						DebugLog.DebugWrite("Profile at " + fileInfo.FullName + " null. Skipping.");
					}
					else
					{
						profiles.Add(profileData);
					}
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole("[" + ex.Message + "] Failed loading profile at " + fileInfo.Name, OWML.Common.MessageType.Error);
					stream?.Close();
				}
			}
		}
		else
		{
			DebugLog.DebugWrite($"{_profilesPath} does not exist");
		}

		MarkBusyWithFileOps(false);
	}

	public void RestoreCurrentProfileBackup()
	{
		MarkBusyWithFileOps(isBusy: true);
		var profilePath = _profilesPath + "/" + currentProfile.profileName;
		var savePath = profilePath + "/" + _gameSaveFilename;
		var multSavePath = profilePath + "/" + _gameSaveMultFilename;
		var settingsPath = profilePath + "/" + _gameSettingsFilename;
		var graphicsPath = profilePath + "/" + _gfxSettingsFilename;
		var inputsPath = profilePath + "/" + _inputActionsSettingsFilename;

		var profileBackupPath = _profileBackupPath + "/" + currentProfile.profileName;
		var saveBackupPath = profileBackupPath + "/" + _gameSaveFilename;
		var multSaveBackupPath = profileBackupPath + "/" + _gameSaveMultFilename;
		var settingsBackupPath = profileBackupPath + "/" + _gameSettingsFilename;
		var graphicsBackupPath = profileBackupPath + "/" + _gfxSettingsFilename;
		var inputsBackupPath = profileBackupPath + "/" + _inputActionsSettingsFilename;

		Stream stream = null;
		try
		{
			if (!Directory.Exists(_profilesPath))
			{
				Directory.CreateDirectory(_profilesPath);
			}

			if (!Directory.Exists(_profileTempPath))
			{
				Directory.CreateDirectory(_profileTempPath);
			}

			if (!Directory.Exists(_profileBackupPath))
			{
				Directory.CreateDirectory(_profileBackupPath);
			}

			if (!Directory.Exists(profilePath))
			{
				Directory.CreateDirectory(profilePath);
			}

			if (!Directory.Exists(profileBackupPath))
			{
				Directory.CreateDirectory(profileBackupPath);
			}

			var di = new DirectoryInfo(profileBackupPath);

			if (currentProfile.brokenSaveData && File.Exists(saveBackupPath))
			{
				currentProfile.gameSave = LoadAndCopyBackupSave<GameSave>(_gameSaveFilename, saveBackupPath, savePath);
			}

			if (currentProfile.brokenMultSaveData && File.Exists(multSaveBackupPath))
			{
				currentProfile.multiplayerGameSave = LoadAndCopyBackupSave<GameSave>(_gameSaveMultFilename, multSaveBackupPath, multSavePath);
			}

			if (currentProfile.brokenSettingsData && File.Exists(settingsBackupPath))
			{
				currentProfile.settingsSave = LoadAndCopyBackupSave<SettingsSave>(_gameSettingsFilename, settingsBackupPath, settingsPath);
			}

			if (currentProfile.brokenGfxSettingsData && File.Exists(graphicsBackupPath))
			{
				currentProfile.graphicsSettings = LoadAndCopyBackupSave<GraphicSettings>(_gfxSettingsFilename, graphicsBackupPath, graphicsPath);
			}

			if (currentProfile.brokenRebindingData && File.Exists(inputsBackupPath))
			{
				TryLoadInputBindingsSave(ref stream, di, out var inputJSON);
				if (inputJSON != "")
				{
					currentProfile.inputJSON = inputJSON;
					File.Copy(inputsBackupPath, inputsPath, overwrite: true);
				}
				else
				{
					Debug.LogError("Could not load backup input bindings save.");
				}

				stream?.Close();
				stream = null;
			}

			OnBackupDataRestored?.Invoke();

			T LoadAndCopyBackupSave<T>(string fileName, string backupPath, string fullPath) where T : class
			{
				TryLoadSaveData<T>(ref stream, fileName, di, out var saveData);
				if (saveData != null)
				{
					File.Copy(backupPath, fullPath, overwrite: true);
				}
				else
				{
					Debug.LogError("Could not load backup " + typeof(T).Name + " save.");
				}

				stream?.Close();
				stream = null;
				return saveData;
			}
		}
		catch (Exception ex)
		{
			stream?.Close();
			Debug.LogError("Exception during backup restore: " + ex.Message);
			MarkBusyWithFileOps(isBusy: false);
		}

		MarkBusyWithFileOps(isBusy: false);
	}

	private bool TrySaveProfile(QSBProfileData profileData, GameSave gameSave, SettingsSave settingsSave, GraphicSettings graphicsSettings, string inputJson)
	{
		MarkBusyWithFileOps(isBusy: true);
		var profilePath = _profilesPath + "/" + profileData.profileName;
		var profileManifestPath = _profilesPath + "/" + profileData.profileName + ".owprofile";
		var saveDataPath = profilePath + "/" + _gameSaveFilename;
		var multSaveDataPath = profilePath + "/" + _gameSaveMultFilename;
		var settingsPath = profilePath + "/" + _gameSettingsFilename;
		var graphicsPath = profilePath + "/" + _gfxSettingsFilename;
		var inputsPath = profilePath + "/" + _inputActionsSettingsFilename;

		var tempProfilePath = _profileTempPath + "/GameData";
		var tempProfileManifestPath = _profileTempPath + "/CurrentProfile.owprofile";
		var tempSaveDataPath = tempProfilePath + "/" + _gameSaveFilename;
		var tempMultSaveDataPath = tempProfilePath + "/" + _gameSaveMultFilename;
		var tempSettingsPath = tempProfilePath + "/" + _gameSettingsFilename;
		var tempGraphicsPath = tempProfilePath + "/" + _gfxSettingsFilename;
		var tempInputsPath = tempProfilePath + "/" + _inputActionsSettingsFilename;

		var backupProfilePath = _profileBackupPath + "/" + profileData.profileName;
		var backupSaveDataPath = backupProfilePath + "/" + _gameSaveFilename;
		var backupMultSaveDataPath = backupProfilePath + "/" + _gameSaveMultFilename;
		var backupSettingsPath = backupProfilePath + "/" + _gameSettingsFilename;
		var backupGraphicsPath = backupProfilePath + "/" + _gfxSettingsFilename;
		var backupInputsPath = backupProfilePath + "/" + _inputActionsSettingsFilename;

		Stream stream = null;
		try
		{
			// Create folders if they don't exist

			if (!Directory.Exists(_profilesPath))
			{
				Directory.CreateDirectory(_profilesPath);
			}

			if (!Directory.Exists(_profileTempPath))
			{
				Directory.CreateDirectory(_profileTempPath);
			}

			if (!Directory.Exists(_profileBackupPath))
			{
				Directory.CreateDirectory(_profileBackupPath);
			}

			if (!Directory.Exists(profilePath))
			{
				Directory.CreateDirectory(profilePath);
			}

			if (!Directory.Exists(tempProfilePath))
			{
				Directory.CreateDirectory(tempProfilePath);
			}

			if (!Directory.Exists(backupProfilePath))
			{
				Directory.CreateDirectory(backupProfilePath);
			}

			// create temp files

			SaveData(tempProfileManifestPath, profileData);
			if (gameSave != null)
			{
				if (QSBCore.IsInMultiplayer)
				{
					profileData.multiplayerGameSave = SaveData(tempMultSaveDataPath, gameSave);
				}
				else
				{
					profileData.gameSave = SaveData(tempSaveDataPath, gameSave);
				}
			}

			if (settingsSave != null)
			{
				profileData.settingsSave = SaveData(tempSettingsPath, settingsSave);
			}

			if (graphicsSettings != null)
			{
				profileData.graphicsSettings = SaveData(tempGraphicsPath, graphicsSettings);
			}

			if (inputJson != null)
			{
				File.WriteAllText(tempInputsPath, inputJson);
				profileData.inputJSON = inputJson;
			}

			// create backups of old files

			if (File.Exists(saveDataPath))
			{
				File.Copy(saveDataPath, backupSaveDataPath, overwrite: true);
			}

			if (File.Exists(multSaveDataPath))
			{
				File.Copy(multSaveDataPath, backupMultSaveDataPath, overwrite: true);
			}

			if (File.Exists(settingsPath))
			{
				File.Copy(settingsPath, backupSettingsPath, overwrite: true);
			}

			if (File.Exists(graphicsPath))
			{
				File.Copy(graphicsPath, backupGraphicsPath, overwrite: true);
			}

			if (File.Exists(inputsPath))
			{
				File.Copy(inputsPath, backupInputsPath, overwrite: true);
			}

			// delete old files and move temp files

			File.Delete(profileManifestPath);
			File.Move(tempProfileManifestPath, profileManifestPath);

			if (gameSave != null)
			{
				if (QSBCore.IsInMultiplayer)
				{
					File.Delete(multSaveDataPath);
					File.Move(tempMultSaveDataPath, multSaveDataPath);
				}
				else
				{
					File.Delete(saveDataPath);
					File.Move(tempSaveDataPath, saveDataPath);
				}
			}

			if (settingsSave != null)
			{
				File.Delete(settingsPath);
				File.Move(tempSettingsPath, settingsPath);
			}

			if (graphicsSettings != null)
			{
				File.Delete(graphicsPath);
				File.Move(tempGraphicsPath, graphicsPath);
			}

			if (inputJson != null)
			{
				File.Delete(inputsPath);
				File.Move(tempInputsPath, inputsPath);
			}

			OnProfileDataSaved?.Invoke(true);
		}
		catch (Exception ex)
		{
			if (stream != null)
			{
				stream.Close();
			}

			OnProfileDataSaved?.Invoke(false);

			Debug.LogError("[" + ex.Message + "] Error saving file for " + profileData.profileName);
			MarkBusyWithFileOps(isBusy: false);
			return false;
		}

		MarkBusyWithFileOps(isBusy: false);
		return true;

		T SaveData<T>(string filePath, T data)
		{
			stream = File.Open(filePath, FileMode.Create);
			using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
			{
				_jsonSerializer.Serialize(jsonWriter, data);
			}

			stream = null;
			return data;
		}
	}

	public bool IsValidCharacterForProfileName(char inputChar)
	{
		if (char.IsWhiteSpace(inputChar))
		{
			return false;
		}

		var invalidFileNameChars = Path.GetInvalidFileNameChars();
		for (var i = 0; i < invalidFileNameChars.Length; i++)
		{
			if (invalidFileNameChars[i] == inputChar)
			{
				return false;
			}
		}

		return inputChar != '.';
	}

	public bool ValidateProfileName(string profileName)
	{
		var result = true;
		if (profileName == "")
		{
			result = false;
		}
		else if (profileName.Length > 16)
		{
			result = false;
		}
		else if (profiles.Count > 0)
		{
			for (var i = 0; i < profiles.Count; i++)
			{
				if (profiles[i].profileName == profileName)
				{
					result = false;
				}
			}
		}

		return result;
	}

	public bool TryCreateProfile(string profileName)
	{
		var savedProfile = ValidateProfileName(profileName);
		if (savedProfile)
		{
			var noProfilesExist = profiles.Count == 0;
			var profileData = new QSBProfileData
			{
				profileName = profileName,
				lastModifiedTime = DateTime.UtcNow
			};
			var gameSave = new GameSave();
			var multGameSave = new GameSave();
			var settingsSave = new SettingsSave();
			var graphicSettings = currentProfileGraphicsSettings;
			if (graphicSettings == null)
			{
				graphicSettings = new GraphicSettings(init: true);
			}

			var text = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
			profiles.Add(profileData);
			profileData.gameSave = gameSave;
			profileData.multiplayerGameSave = multGameSave;
			profileData.settingsSave = settingsSave;
			profileData.graphicsSettings = graphicSettings;
			profileData.inputJSON = text;
			savedProfile = TrySaveProfile(profileData, gameSave, settingsSave, graphicSettings, text);
			if (savedProfile)
			{
				if (currentProfile != null && currentProfile.profileName != string.Empty)
				{
					OnProfileSignOutComplete?.Invoke();
				}

				currentProfile = profileData;
				if (noProfilesExist)
				{
					OnProfileSignInComplete?.Invoke(ProfileManagerSignInResult.COMPLETE);
					OnProfileReadDone?.Invoke();
				}
				else
				{
					OnProfileSignInComplete?.Invoke(ProfileManagerSignInResult.COMPLETE);
					OnProfileReadDone?.Invoke();
					OnUpdatePlayerProfiles?.Invoke();
				}
			}
			else
			{
				DeleteProfile(profileName);
			}
		}

		return savedProfile;
	}

	public bool SwitchProfile(string profileName)
	{
		LoadSaveFilesFromProfiles();
		var flag = false;
		for (var i = 0; i < profiles.Count; i++)
		{
			if (profileName == profiles[i].profileName)
			{
				if (currentProfile != null && currentProfile.profileName != string.Empty && OnProfileSignOutComplete != null)
				{
					OnProfileSignOutComplete();
				}

				currentProfile = profiles[i];
				flag = true;
				break;
			}
		}

		if (flag)
		{
			currentProfile.lastModifiedTime = DateTime.UtcNow;
			TrySaveProfile(currentProfile, null, null, null, null);
			OnProfileSignInComplete?.Invoke(ProfileManagerSignInResult.COMPLETE);

			if (CurrentProfileHasBrokenData() && OnBrokenDataExists != null)
			{
				OnBrokenDataExists();
				return false;
			}

			OnProfileReadDone?.Invoke();
		}

		return true;
	}

	public void DeleteProfile(string profileName)
	{
		Debug.Log("DeleteProfile");
		var flag = false;
		var profileData = new QSBProfileData
		{
			profileName = string.Empty
		};
		for (var i = 0; i < profiles.Count; i++)
		{
			if (profileName == profiles[i].profileName)
			{
				profileData = profiles[i];
				flag = true;
				break;
			}
		}

		if (!flag)
		{
			return;
		}

		MarkBusyWithFileOps(isBusy: true);
		var profileManifestPath = _profilesPath + "/" + profileData.profileName + ".owprofile";
		var profilePath = _profilesPath + "/" + profileData.profileName;
		var gameSavePath = profilePath + "/" + _gameSaveFilename;
		var multGameSavePath = profilePath + "/" + _gameSaveMultFilename;
		var settingsPath = profilePath + "/" + _gameSettingsFilename;
		var graphicsPath = profilePath + "/" + _gfxSettingsFilename;
		var oldInputsPath = profilePath + "/" + _legacyInputBindingSettingsFilename;
		var inputsPath = profilePath + "/" + _inputActionsSettingsFilename;

		var backupProfilePath = _profileBackupPath + "/" + profileData.profileName;
		var backupGameSave = backupProfilePath + "/" + _gameSaveFilename;
		var backupMultGameSave = backupProfilePath + "/" + _gameSaveMultFilename;
		var backupSettingsPath = backupProfilePath + "/" + _gameSettingsFilename;
		var backupGraphicsPath = backupProfilePath + "/" + _gfxSettingsFilename;
		var backupOldInputsPath = backupProfilePath + "/" + _legacyInputBindingSettingsFilename;
		var backupInputsPath = backupProfilePath + "/" + _inputActionsSettingsFilename;
		Stream stream = null;
		try
		{
			if (File.Exists(profileManifestPath))
			{
				File.Delete(profileManifestPath);
				Debug.Log("Delete " + profileManifestPath);
			}

			if (File.Exists(gameSavePath))
			{
				File.Delete(gameSavePath);
				Debug.Log("Delete " + gameSavePath);
			}

			if (File.Exists(multGameSavePath))
			{
				File.Delete(multGameSavePath);
				Debug.Log("Delete " + multGameSavePath);
			}

			if (File.Exists(settingsPath))
			{
				File.Delete(settingsPath);
				Debug.Log("Delete " + settingsPath);
			}

			if (File.Exists(graphicsPath))
			{
				File.Delete(graphicsPath);
				Debug.Log("Delete " + graphicsPath);
			}

			if (File.Exists(oldInputsPath))
			{
				File.Delete(oldInputsPath);
				Debug.Log("Delete " + oldInputsPath);
			}

			if (File.Exists(inputsPath))
			{
				File.Delete(inputsPath);
				Debug.Log("Delete " + inputsPath);
			}

			if (File.Exists(backupGameSave))
			{
				File.Delete(backupGameSave);
				Debug.Log("Delete " + backupGameSave);
			}

			if (File.Exists(backupMultGameSave))
			{
				File.Delete(backupMultGameSave);
				Debug.Log("Delete " + backupMultGameSave);
			}

			if (File.Exists(backupSettingsPath))
			{
				File.Delete(backupSettingsPath);
				Debug.Log("Delete " + backupSettingsPath);
			}

			if (File.Exists(backupGraphicsPath))
			{
				File.Delete(backupGraphicsPath);
				Debug.Log("Delete " + backupGraphicsPath);
			}

			if (File.Exists(backupOldInputsPath))
			{
				File.Delete(backupOldInputsPath);
				Debug.Log("Delete " + backupOldInputsPath);
			}

			if (File.Exists(backupInputsPath))
			{
				File.Delete(backupInputsPath);
				Debug.Log("Delete " + backupInputsPath);
			}

			profiles.Remove(profileData);
			var files = Directory.GetFiles(profilePath);
			var directories = Directory.GetDirectories(profilePath);
			if (files.Length == 0 && directories.Length == 0)
			{
				Directory.Delete(profilePath);
			}
			else
			{
				Debug.LogWarning(" Directory not empty. Cannot delete. ");
			}

			if (Directory.Exists(backupProfilePath))
			{
				files = Directory.GetFiles(backupProfilePath);
				directories = Directory.GetDirectories(backupProfilePath);
				if (files.Length == 0 && directories.Length == 0)
				{
					Directory.Delete(backupProfilePath);
				}
				else
				{
					Debug.LogWarning("Backup Directory not empty. Cannot delete.");
				}
			}

			OnUpdatePlayerProfiles?.Invoke();
		}
		catch (Exception ex)
		{
			stream?.Close();
			Debug.LogError("[" + ex.Message + "] Failed to delete all profile data");
			MarkBusyWithFileOps(isBusy: false);
		}

		MarkBusyWithFileOps(isBusy: false);
	}

	[Serializable]
	public class QSBProfileData
	{
		public string profileName;
		public DateTime lastModifiedTime;
		public bool brokenSaveData;
		public bool brokenMultSaveData;
		public bool brokenSettingsData;
		public bool brokenGfxSettingsData;
		public bool brokenRebindingData;
		private GameSave _gameSave;
		private GameSave _multiplayerGameSave;
		private SettingsSave _settingsSave;
		private GraphicSettings _graphicsSettings;
		private string _inputJSON;

		[JsonIgnore]
		public GameSave gameSave
		{
			get => _gameSave;
			set => _gameSave = value;
		}

		[JsonIgnore]
		public GameSave multiplayerGameSave
		{
			get => _multiplayerGameSave;
			set => _multiplayerGameSave = value;
		}

		[JsonIgnore]
		public SettingsSave settingsSave
		{
			get => _settingsSave;
			set => _settingsSave = value;
		}

		[JsonIgnore]
		public GraphicSettings graphicsSettings
		{
			get => _graphicsSettings;
			set => _graphicsSettings = value;
		}

		[JsonIgnore]
		public string inputJSON
		{
			get => _inputJSON;
			set => _inputJSON = value;
		}

		[OnDeserializing]
		private void SetDefaultValuesOnDeserializing(StreamingContext context)
		{
			brokenSaveData = false;
			brokenMultSaveData = false;
			brokenSettingsData = false;
			brokenGfxSettingsData = false;
			brokenRebindingData = false;
		}

		[OnDeserialized]
		private void SetDefaultValuesOnDeserialized(StreamingContext context)
		{
			brokenSaveData = false;
			brokenMultSaveData = false;
			brokenSettingsData = false;
			brokenGfxSettingsData = false;
			brokenRebindingData = false;
		}
	}
}
