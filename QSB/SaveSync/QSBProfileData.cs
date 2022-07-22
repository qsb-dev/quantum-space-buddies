using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace QSB.SaveSync;

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
