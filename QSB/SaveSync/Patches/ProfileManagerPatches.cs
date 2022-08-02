using HarmonyLib;
using Newtonsoft.Json;
using QSB.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace QSB.SaveSync.Patches;

/// <summary>
/// all these patches are to use QSBProfileData instead of the existing profile in order to support a separate multiplayer save 
/// </summary>
internal class ProfileManagerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override PatchVendor PatchVendor => PatchVendor.Steam | PatchVendor.Epic;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.LoadSaveFilesFromProfiles))]
	public static bool LoadSaveFilesFromProfiles(StandaloneProfileManager __instance)
	{
		__instance.MarkBusyWithFileOps(isBusy: true);
		foreach (var profile in QSBProfileManager._profiles)
		{
			var path = __instance._profilesPath + "/" + profile.profileName;
			GameSave saveData = null;
			GameSave multSaveData = null;
			SettingsSave settingsData = null;
			GraphicSettings graphicsData = null;
			var inputJSON = "";
			if (Directory.Exists(path))
			{
				Stream stream = null;
				var directoryInfo = new DirectoryInfo(path);
				profile.brokenSaveData = __instance.TryLoadSaveData(null, ref stream, "data.owsave", directoryInfo, out saveData);
				profile.brokenMultSaveData = __instance.TryLoadSaveData(null, ref stream, "dataMult.owsave", directoryInfo, out multSaveData);
				profile.brokenSettingsData = __instance.TryLoadSaveData(null, ref stream, "player.owsett", directoryInfo, out settingsData);
				profile.brokenGfxSettingsData = __instance.TryLoadSaveData(null, ref stream, "graphics.owsett", directoryInfo, out graphicsData);
				profile.brokenRebindingData = __instance.TryLoadInputBindingsSave(null, ref stream, directoryInfo, out inputJSON);
			}

			var profilePath = __instance._profileBackupPath + "/" + profile.profileName;
			var savePath = profilePath + "/data.owsave";
			var multSavePath = profilePath + "/dataMult.owsave";
			var settingsPath = profilePath + "/player.owsett";
			var graphicsPath = profilePath + "/graphics.owsett";
			var inputsPath = profilePath + "/input_new.owsett";

			if (saveData == null)
			{
				profile.brokenSaveData = File.Exists(savePath);
				saveData = new GameSave();
				UnityEngine.Debug.LogError("Could not find game save for " + profile.profileName);
			}

			if (multSaveData == null)
			{
				profile.brokenMultSaveData = File.Exists(multSavePath);
				multSaveData = new GameSave();
				UnityEngine.Debug.LogError("Could not find multiplayer game save for " + profile.profileName);
			}

			if (settingsData == null)
			{
				profile.brokenSettingsData = File.Exists(settingsPath);
				settingsData = new SettingsSave();
				UnityEngine.Debug.LogError("Could not find game settings for " + profile.profileName);
			}

			if (graphicsData == null)
			{
				profile.brokenGfxSettingsData = File.Exists(graphicsPath);
				graphicsData = new GraphicSettings(init: true);
				UnityEngine.Debug.LogError("Could not find graphics settings for " + profile.profileName);
			}

			if (inputJSON == "")
			{
				profile.brokenRebindingData = File.Exists(inputsPath);
				inputJSON = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
				UnityEngine.Debug.LogError("Could not find input action settings for " + profile.profileName);
			}

			profile.gameSave = saveData;
			profile.multiplayerGameSave = multSaveData;
			profile.settingsSave = settingsData;
			profile.graphicsSettings = graphicsData;
			profile.inputJSON = inputJSON;
		}

		__instance.MarkBusyWithFileOps(isBusy: false);
		if (__instance.CurrentProfileHasBrokenData())
		{
			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnBrokenDataExists));
		}

		__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileReadDone));

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.LoadProfiles))]
	public static bool LoadProfiles(StandaloneProfileManager __instance)
	{
		__instance.MarkBusyWithFileOps(true);
		QSBProfileManager._profiles.Clear();
		if (Directory.Exists(__instance._profilesPath))
		{
			QSBProfileData profileData = null;
			Stream stream = null;
			var files = new DirectoryInfo(__instance._profilesPath).GetFiles("*.owprofile");
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
						profileData = __instance._jsonSerializer.Deserialize<QSBProfileData>(jsonTextReader);
					}
					catch
					{
						stream.Position = 0L;
						profileData = (QSBProfileData)__instance._binaryFormatter.Deserialize(stream);
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
						QSBProfileManager._profiles.Add(profileData);
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
			DebugLog.DebugWrite($"{__instance._profilesPath} does not exist");
		}

		__instance.MarkBusyWithFileOps(false);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.CurrentProfileHasBrokenData))]
	public static bool CurrentProfileHasBrokenData(StandaloneProfileManager __instance, out bool __result)
	{
		var currentProfile = QSBProfileManager._currentProfile;

		if (currentProfile == null)
		{
			Debug.LogError("QSBProfileManager.CurrentProfileHasBrokenData We should never get here outside of the Unity Editor");
			__result = false;
			return false;
		}

		__result = currentProfile.brokenSaveData
			|| currentProfile.brokenMultSaveData
			|| currentProfile.brokenSettingsData
			|| currentProfile.brokenGfxSettingsData
			|| currentProfile.brokenRebindingData;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.BackupExistsForBrokenData))]
	public static bool BackupExistsForBrokenData(StandaloneProfileManager __instance, out bool __result)
	{
		var text = __instance._profileBackupPath + "/" + QSBProfileManager._currentProfile.profileName;
		var savePath = text + "/data.owsave";
		var multSavePath = text + "/dataMult.owsave";
		var settingsPath = text + "/player.owsett";
		var graphicsPath = text + "/graphics.owsett";
		var inputsPath = text + "/input_new.owsett";

		if (QSBProfileManager._currentProfile.brokenSaveData && File.Exists(savePath))
		{
			__result = true;
			return false;
		}

		if (QSBProfileManager._currentProfile.brokenMultSaveData && File.Exists(multSavePath))
		{
			__result = true;
			return false;
		}

		if (QSBProfileManager._currentProfile.brokenSettingsData && File.Exists(settingsPath))
		{
			__result = true;
			return false;
		}

		if (QSBProfileManager._currentProfile.brokenGfxSettingsData && File.Exists(graphicsPath))
		{
			__result = true;
			return false;
		}

		if (QSBProfileManager._currentProfile.brokenRebindingData && File.Exists(inputsPath))
		{
			__result = true;
			return false;
		}

		__result = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.RestoreCurrentProfileBackup))]
	public static bool RestoreCurrentProfileBackup(StandaloneProfileManager __instance)
	{
		__instance.MarkBusyWithFileOps(isBusy: true);
		var profilePath = __instance._profilesPath + "/" + QSBProfileManager._currentProfile.profileName;
		var savePath = profilePath + "/data.owsave";
		var multSavePath = profilePath + "/dataMult.owsave";
		var settingsPath = profilePath + "/player.owsett";
		var graphicsPath = profilePath + "/graphics.owsett";
		var inputsPath = profilePath + "/input_new.owsett";

		var profileBackupPath = __instance._profileBackupPath + "/" + QSBProfileManager._currentProfile.profileName;
		var saveBackupPath = profileBackupPath + "/data.owsave";
		var multSaveBackupPath = profileBackupPath + "/dataMult.owsave";
		var settingsBackupPath = profileBackupPath + "/player.owsett";
		var graphicsBackupPath = profileBackupPath + "/graphics.owsett";
		var inputsBackupPath = profileBackupPath + "/input_new.owsett";

		Stream stream = null;
		try
		{
			if (!Directory.Exists(__instance._profilesPath))
			{
				Directory.CreateDirectory(__instance._profilesPath);
			}

			if (!Directory.Exists(__instance._profileTempPath))
			{
				Directory.CreateDirectory(__instance._profileTempPath);
			}

			if (!Directory.Exists(__instance._profileBackupPath))
			{
				Directory.CreateDirectory(__instance._profileBackupPath);
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

			if (QSBProfileManager._currentProfile.brokenSaveData && File.Exists(saveBackupPath))
			{
				QSBProfileManager._currentProfile.gameSave = LoadAndCopyBackupSave<GameSave>("data.owsave", saveBackupPath, savePath);
			}

			if (QSBProfileManager._currentProfile.brokenMultSaveData && File.Exists(multSaveBackupPath))
			{
				QSBProfileManager._currentProfile.multiplayerGameSave = LoadAndCopyBackupSave<GameSave>("dataMult.owsave", multSaveBackupPath, multSavePath);
			}

			if (QSBProfileManager._currentProfile.brokenSettingsData && File.Exists(settingsBackupPath))
			{
				QSBProfileManager._currentProfile.settingsSave = LoadAndCopyBackupSave<SettingsSave>("player.owsett", settingsBackupPath, settingsPath);
			}

			if (QSBProfileManager._currentProfile.brokenGfxSettingsData && File.Exists(graphicsBackupPath))
			{
				QSBProfileManager._currentProfile.graphicsSettings = LoadAndCopyBackupSave<GraphicSettings>("graphics.owsett", graphicsBackupPath, graphicsPath);
			}

			if (QSBProfileManager._currentProfile.brokenRebindingData && File.Exists(inputsBackupPath))
			{
				__instance.TryLoadInputBindingsSave(null, ref stream, di, out var inputJSON);
				if (inputJSON != "")
				{
					QSBProfileManager._currentProfile.inputJSON = inputJSON;
					File.Copy(inputsBackupPath, inputsPath, overwrite: true);
				}
				else
				{
					UnityEngine.Debug.LogError("Could not load backup input bindings save.");
				}

				stream?.Close();
				stream = null;
			}

			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnBackupDataRestored));

			T LoadAndCopyBackupSave<T>(string fileName, string backupPath, string fullPath) where T : class
			{
				__instance.TryLoadSaveData<T>(null, ref stream, fileName, di, out var saveData);
				if (saveData != null)
				{
					File.Copy(backupPath, fullPath, overwrite: true);
				}
				else
				{
					UnityEngine.Debug.LogError("Could not load backup " + typeof(T).Name + " save.");
				}

				stream?.Close();
				stream = null;
				return saveData;
			}
		}
		catch (Exception ex)
		{
			stream?.Close();
			UnityEngine.Debug.LogError("Exception during backup restore: " + ex.Message);
			__instance.MarkBusyWithFileOps(isBusy: false);
		}

		__instance.MarkBusyWithFileOps(isBusy: false);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.TrySaveProfile))]
	public static bool TrySaveProfile()
	{
		DebugLog.DebugWrite($"Error - StandaloneProfileManager.TrySaveProfile should not be used anymore." +
			$"{Environment.NewLine}Called by : {Environment.NewLine}{Environment.StackTrace}", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.SaveGame))]
	public static bool SaveGame(StandaloneProfileManager __instance, GameSave gameSave, SettingsSave settSave, GraphicSettings graphicSettings, string inputBindings)
	{
		if (__instance.isBusyWithFileOps || LoadManager.IsBusy())
		{
			__instance._pendingGameSave = gameSave;
			__instance._pendingSettingsSave = settSave;
			__instance._pendingGfxSettingsSave = graphicSettings;
			__instance._pendingInputJSONSave = inputBindings;
		}
		else
		{
			TrySaveProfile(QSBProfileManager._currentProfile, gameSave, settSave, graphicSettings, inputBindings);
		}

		return false;
	}

	private static bool TrySaveProfile(QSBProfileData profileData, GameSave gameSave, SettingsSave settingsSave, GraphicSettings graphicsSettings, string inputJson)
	{
		var profileManager = StandaloneProfileManager.SharedInstance;

		profileManager.MarkBusyWithFileOps(isBusy: true);
		var profilePath = profileManager._profilesPath + "/" + profileData.profileName;
		var profileManifestPath = profileManager._profilesPath + "/" + profileData.profileName + ".owprofile";
		var saveDataPath = profilePath + "/data.owsave";
		var multSaveDataPath = profilePath + "/dataMult.owsave";
		var settingsPath = profilePath + "/player.owsett";
		var graphicsPath = profilePath + "/graphics.owsett";
		var inputsPath = profilePath + "/input_new.owsett";

		var tempProfilePath = profileManager._profileTempPath + "/GameData";
		var tempProfileManifestPath = profileManager._profileTempPath + "/CurrentProfile.owprofile";
		var tempSaveDataPath = tempProfilePath + "/data.owsave";
		var tempMultSaveDataPath = tempProfilePath + "/dataMult.owsave";
		var tempSettingsPath = tempProfilePath + "/player.owsett";
		var tempGraphicsPath = tempProfilePath + "/graphics.owsett";
		var tempInputsPath = tempProfilePath + "/input_new.owsett";

		var backupProfilePath = profileManager._profileBackupPath + "/" + profileData.profileName;
		var backupSaveDataPath = backupProfilePath + "/data.owsave";
		var backupMultSaveDataPath = backupProfilePath + "/dataMult.owsave";
		var backupSettingsPath = backupProfilePath + "/player.owsett";
		var backupGraphicsPath = backupProfilePath + "/graphics.owsett";
		var backupInputsPath = backupProfilePath + "/input_new.owsett";

		Stream stream = null;
		try
		{
			// Create folders if they don't exist

			if (!Directory.Exists(profileManager._profilesPath))
			{
				Directory.CreateDirectory(profileManager._profilesPath);
			}

			if (!Directory.Exists(profileManager._profileTempPath))
			{
				Directory.CreateDirectory(profileManager._profileTempPath);
			}

			if (!Directory.Exists(profileManager._profileBackupPath))
			{
				Directory.CreateDirectory(profileManager._profileBackupPath);
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

			Debug.Log("Wrote save data to file for " + profileData.profileName);
			profileManager.RaiseEvent(nameof(StandaloneProfileManager.OnProfileDataSaved), true);
		}
		catch (Exception ex)
		{
			if (stream != null)
			{
				stream.Close();
			}

			profileManager.RaiseEvent(nameof(StandaloneProfileManager.OnProfileDataSaved), false);

			Debug.LogError("[" + ex.Message + "] Error saving file for " + profileData.profileName);
			profileManager.MarkBusyWithFileOps(isBusy: false);
			return false;
		}

		profileManager.MarkBusyWithFileOps(isBusy: false);
		return true;

		T SaveData<T>(string filePath, T data)
		{
			stream = File.Open(filePath, FileMode.Create);
			using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
			{
				profileManager._jsonSerializer.Serialize(jsonWriter, data);
			}

			stream = null;
			return data;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.InitializeProfileData))]
	public static bool InitializeProfileData(StandaloneProfileManager __instance)
	{
		__instance.LoadProfiles();
		QSBProfileManager._currentProfile = QSBProfileManager.mostRecentProfile;
		if (QSBProfileManager._currentProfile == null)
		{
			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnNoProfilesExist));
		}
		else
		{
			__instance.LoadSaveFilesFromProfiles();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.currentProfileGameSave), MethodType.Getter)]
	public static bool CurrentProfileGameSave(out GameSave __result)
	{
		__result = QSBProfileManager._currentProfile?.gameSave;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.currentProfileGameSettings), MethodType.Getter)]
	public static bool CurrentProfileGameSettings(out SettingsSave __result)
	{
		__result = QSBProfileManager._currentProfile?.settingsSave;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.currentProfileGraphicsSettings), MethodType.Getter)]
	public static bool CurrentProfileGraphicsSettings(out GraphicSettings __result)
	{
		__result = QSBProfileManager._currentProfile?.graphicsSettings;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.currentProfileInputJSON), MethodType.Getter)]
	public static bool CurrentProfileInputJSON(out string __result)
	{
		__result = QSBProfileManager._currentProfile?.inputJSON;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), "get_currentProfile")]
	public static bool CurrentProfile(out StandaloneProfileManager.ProfileData __result)
	{
		DebugLog.DebugWrite($"Error - StandaloneProfileManager.currentProfile should not be used anymore." +
			$"{Environment.NewLine}Called by : {Environment.NewLine}{Environment.StackTrace}", OWML.Common.MessageType.Error);
		__result = null;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), "get_mostRecentProfile")]
	public static bool MostRecentProfile(out StandaloneProfileManager.ProfileData __result)
	{
		DebugLog.DebugWrite($"Error - StandaloneProfileManager.mostRecentProfile should not be used anymore." +
			$"{Environment.NewLine}Called by : {Environment.NewLine}{Environment.StackTrace}", OWML.Common.MessageType.Error);
		__result = null;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), "get_profiles")]
	public static bool Profiles(out List<StandaloneProfileManager.ProfileData> __result)
	{
		DebugLog.DebugWrite($"Error - StandaloneProfileManager.profiles should not be used anymore." +
			$"{Environment.NewLine}Called by : {Environment.NewLine}{Environment.StackTrace}", OWML.Common.MessageType.Error);
		__result = null;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.TryCreateProfile))]
	public static bool TryCreateProfile(StandaloneProfileManager __instance, string profileName, out bool __result)
	{
		bool savedProfile = __instance.ValidateProfileName(profileName);
		if (savedProfile)
		{
			bool noProfilesExist = QSBProfileManager._profiles.Count == 0;
			QSBProfileData profileData = new QSBProfileData();
			profileData.profileName = profileName;
			profileData.lastModifiedTime = DateTime.UtcNow;
			GameSave gameSave = new GameSave();
			GameSave multGameSave = new GameSave();
			SettingsSave settingsSave = new SettingsSave();
			GraphicSettings graphicSettings = __instance.currentProfileGraphicsSettings;
			if (graphicSettings == null)
			{
				graphicSettings = new GraphicSettings(init: true);
			}
			string text = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
			QSBProfileManager._profiles.Add(profileData);
			profileData.gameSave = gameSave;
			profileData.multiplayerGameSave = multGameSave;
			profileData.settingsSave = settingsSave;
			profileData.graphicsSettings = graphicSettings;
			profileData.inputJSON = text;
			savedProfile = TrySaveProfile(profileData, gameSave, settingsSave, graphicSettings, text);
			if (savedProfile)
			{
				if (QSBProfileManager._currentProfile != null && QSBProfileManager._currentProfile.profileName != string.Empty)
				{
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileSignOutComplete));
				}

				QSBProfileManager._currentProfile = profileData;
				if (noProfilesExist)
				{
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileSignInComplete), ProfileManagerSignInResult.COMPLETE);
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileReadDone));
				}
				else
				{
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileSignInComplete), ProfileManagerSignInResult.COMPLETE);
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileReadDone));
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnUpdatePlayerProfiles));
				}
			}
			else
			{
				__instance.DeleteProfile(profileName);
			}
		}

		__result = savedProfile;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.DeleteProfile))]
	public static bool DeleteProfile(StandaloneProfileManager __instance, string profileName)
	{
		Debug.Log("DeleteProfile");
		bool flag = false;
		QSBProfileData profileData = new QSBProfileData();
		profileData.profileName = string.Empty;
		for (int i = 0; i < QSBProfileManager._profiles.Count; i++)
		{
			if (profileName == QSBProfileManager._profiles[i].profileName)
			{
				profileData = QSBProfileManager._profiles[i];
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		__instance.MarkBusyWithFileOps(isBusy: true);
		string profileManifestPath = __instance._profilesPath + "/" + profileData.profileName + ".owprofile";
		string profilePath = __instance._profilesPath + "/" + profileData.profileName;
		string gameSavePath = profilePath + "/data.owsave";
		string multGameSavePath = profilePath + "/dataMult.owsave";
		string settingsPath = profilePath + "/player.owsett";
		string graphicsPath = profilePath + "/graphics.owsett";
		string oldInputsPath = profilePath + "/input.owsett";
		string inputsPath = profilePath + "/input_new.owsett";

		string backupProfilePath = __instance._profileBackupPath + "/" + profileData.profileName;
		string backupGameSave = backupProfilePath + "/data.owsave";
		string backupMultGameSave = backupProfilePath + "/dataMult.owsave";
		string backupSettingsPath = backupProfilePath + "/player.owsett";
		string backupGraphicsPath = backupProfilePath + "/graphics.owsett";
		string backupOldInputsPath = backupProfilePath + "/input.owsett";
		string backupInputsPath = backupProfilePath + "/input_new.owsett";
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

			QSBProfileManager._profiles.Remove(profileData);
			string[] files = Directory.GetFiles(profilePath);
			string[] directories = Directory.GetDirectories(profilePath);
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

			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnUpdatePlayerProfiles));
		}
		catch (Exception ex)
		{
			stream?.Close();
			Debug.LogError("[" + ex.Message + "] Failed to delete all profile data");
			__instance.MarkBusyWithFileOps(isBusy: false);
		}

		__instance.MarkBusyWithFileOps(isBusy: false);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.SwitchProfile))]
	public static bool SwitchProfile(StandaloneProfileManager __instance, string profileName, out bool __result)
	{
		__instance.LoadSaveFilesFromProfiles();
		bool flag = false;
		for (int i = 0; i < QSBProfileManager._profiles.Count; i++)
		{
			if (profileName == QSBProfileManager._profiles[i].profileName)
			{
				if (QSBProfileManager._currentProfile != null && QSBProfileManager._currentProfile.profileName != string.Empty)
				{
					__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileSignOutComplete));
				}

				QSBProfileManager._currentProfile = QSBProfileManager._profiles[i];
				flag = true;
				break;
			}
		}

		if (flag)
		{
			QSBProfileManager._currentProfile.lastModifiedTime = DateTime.UtcNow;
			TrySaveProfile(QSBProfileManager._currentProfile, null, null, null, null);
			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileSignInComplete), ProfileManagerSignInResult.COMPLETE);

			if (__instance.CurrentProfileHasBrokenData())
			{
				__instance.RaiseEvent(nameof(StandaloneProfileManager.OnBrokenDataExists));
				__result = false;
				return false;
			}

			__instance.RaiseEvent(nameof(StandaloneProfileManager.OnProfileReadDone));
		}

		__result = true;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.ValidateProfileName))]
	public static bool ValidateProfileName(StandaloneProfileManager __instance, string profileName, out bool __result)
	{
		bool result = true;
		if (profileName == "")
		{
			result = false;
		}
		else if (profileName.Length > 16)
		{
			result = false;
		}
		else if (QSBProfileManager._profiles.Count > 0)
		{
			for (int i = 0; i < QSBProfileManager._profiles.Count; i++)
			{
				if (QSBProfileManager._profiles[i].profileName == profileName)
				{
					result = false;
				}
			}
		}

		__result = result;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(StandaloneProfileManager), nameof(StandaloneProfileManager.PerformPendingSaveOperation))]
	public static bool PerformPendingSaveOperation(StandaloneProfileManager __instance)
	{
		if (!__instance.isBusyWithFileOps && !LoadManager.IsBusy())
		{
			TrySaveProfile(QSBProfileManager._currentProfile, __instance._pendingGameSave, __instance._pendingSettingsSave, __instance._pendingGfxSettingsSave, __instance._pendingInputJSONSave);
			__instance._pendingGameSave = null;
			__instance._pendingSettingsSave = null;
			__instance._pendingGfxSettingsSave = null;
			__instance._pendingInputJSONSave = "";
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProfileMenuManager), nameof(ProfileMenuManager.PopulateProfiles))]
	public static bool PopulateProfiles(ProfileMenuManager __instance)
	{
		if (__instance._listProfileElements == null)
		{
			__instance._listProfileElements = new List<GameObject>();
		}
		else
		{
			for (var i = 0; i < __instance._listProfileElements.Count; i++)
			{
				var requiredComponent = __instance._listProfileElements[i].GetRequiredComponent<TwoButtonActionElement>();
				__instance.ClearProfileElementListeners(requiredComponent);
				UnityEngine.Object.Destroy(__instance._listProfileElements[i]);
			}

			__instance._listProfileElements.Clear();
		}

		if (__instance._listProfileUIElementLookup == null)
		{
			__instance._listProfileUIElementLookup = new List<ProfileMenuManager.ProfileElementLookup>();
		}
		else
		{
			__instance._listProfileUIElementLookup.Clear();
		}

		var array = QSBProfileManager._profiles.ToArray();
		var profileName = QSBProfileManager._currentProfile.profileName;
		var num = 0;
		Selectable selectable = null;
		for (var j = 0; j < array.Length; j++)
		{
			if (!(array[j].profileName == profileName))
			{
				var gameObject = UnityEngine.Object.Instantiate(__instance._profileItemTemplate);
				gameObject.gameObject.SetActive(value: true);
				gameObject.transform.SetParent(__instance._profileListRoot.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				var componentsInChildren = gameObject.gameObject.GetComponentsInChildren<Text>();
				for (var k = 0; k < componentsInChildren.Length; k++)
				{
					__instance._fontController.AddTextElement(componentsInChildren[k]);
				}

				num++;
				var requiredComponent2 = gameObject.GetRequiredComponent<TwoButtonActionElement>();
				var requiredComponent3 = requiredComponent2.GetRequiredComponent<Selectable>();
				__instance.SetUpProfileElementListeners(requiredComponent2);
				requiredComponent2.SetLabelText(array[j].profileName);
				var component = requiredComponent2.GetButtonOne().GetComponent<Text>();
				if (component != null)
				{
					__instance._fontController.AddTextElement(component);
				}

				component = requiredComponent2.GetButtonTwo().GetComponent<Text>();
				if (component != null)
				{
					__instance._fontController.AddTextElement(component);
				}

				if (num == 1)
				{
					var navigation = __instance._createProfileButton.navigation;
					navigation.selectOnDown = gameObject.GetRequiredComponent<Selectable>();
					__instance._createProfileButton.navigation = navigation;
					var navigation2 = requiredComponent3.navigation;
					navigation2.selectOnUp = __instance._createProfileButton;
					requiredComponent3.navigation = navigation2;
				}
				else
				{
					var navigation3 = requiredComponent3.navigation;
					var navigation4 = selectable.navigation;
					navigation3.selectOnUp = selectable;
					navigation3.selectOnDown = null;
					navigation4.selectOnDown = requiredComponent3;
					requiredComponent3.navigation = navigation3;
					selectable.navigation = navigation4;
				}

				__instance._listProfileElements.Add(gameObject);
				selectable = requiredComponent3;
				var profileElementLookup = new ProfileMenuManager.ProfileElementLookup
				{
					profileName = array[j].profileName,
					lastModifiedTime = array[j].lastModifiedTime,
					confirmSwitchAction = requiredComponent2.GetSubmitActionOne() as SubmitActionConfirm,
					confirmDeleteAction = requiredComponent2.GetSubmitActionTwo() as SubmitActionConfirm
				};
				__instance._listProfileUIElementLookup.Add(profileElementLookup);
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProfileMenuManager), nameof(ProfileMenuManager.SetCurrentProfileLabel))]
	public static bool SetCurrentProfileName(ProfileMenuManager __instance)
	{
		__instance._currenProfileLabel.text = UITextLibrary.GetString(UITextType.MenuProfile) + " " + QSBProfileManager._currentProfile.profileName;
		return false;
	}
}
