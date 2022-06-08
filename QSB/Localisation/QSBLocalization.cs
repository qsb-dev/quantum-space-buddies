using QSB.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QSB.Localisation;

public static class QSBLocalization
{
	private readonly static List<Translation> _translations = new();
	public static Translation Current;

	public static Action LanguageChanged;

	public static void Init()
	{
		// get all translation files
		var directory = new DirectoryInfo(Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, "Translations\\"));
		var files = directory.GetFiles("*.json");
		foreach (var file in files)
		{
			var translation = QSBCore.Helper.Storage.Load<Translation>($"Translations\\{file.Name}", false);
			_translations.Add(translation);
			DebugLog.DebugWrite($"- Added translation for language {translation.Language}");
		}

		if (_translations.Count == 0)
		{
			DebugLog.ToConsole($"FATAL - No translation files found!", OWML.Common.MessageType.Fatal);
			return;
		}

		// hack to stop things from breaking
		Current = _translations[0];

		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
	}

	private static void OnLanguageChanged()
	{
		var language = TextTranslation.Get().GetLanguage();
		DebugLog.DebugWrite($"Language changed to {language}");
		var newTranslation = _translations.FirstOrDefault(x => x.Language == language);

		if (newTranslation == default)
		{
			DebugLog.ToConsole($"Error - Could not find translation for language {language}! Defaulting to English.");
			newTranslation = _translations.First(x => x.Language == TextTranslation.Language.ENGLISH);
		}

		Current = newTranslation;
		LanguageChanged?.Invoke();
	}
}
