using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QSB.Localization;

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
			var filePath = Path.Combine(QSBCore.Helper.Manifest.ModFolderPath, $"Translations\\{file.Name}");

			if (translation == null)
			{
				DebugLog.ToConsole($"Error - could not load translation at {filePath}", MessageType.Error);
				continue;
			}

			FixMissingEntries(translation);

			_translations.Add(translation);
			DebugLog.DebugWrite($"- Added translation for language {translation.Language}");
		}

		if (_translations.Count == 0)
		{
			DebugLog.ToConsole("FATAL - No translation files found!", MessageType.Fatal);
			return;
		}

		// gotta call this manually because InitializeLanguage happens before we listen to OnLanguageChanged
		OnLanguageChanged();

		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
	}

	private static void FixMissingEntries(Translation translation)
	{
		var publicFields = typeof(Translation).GetFields(BindingFlags.Public | BindingFlags.Instance);

		var stringFields = publicFields.Where(x => x.FieldType == typeof(string));

		foreach (var stringField in stringFields)
		{
			var value = (string)stringField.GetValue(translation);
			if (string.IsNullOrEmpty(value))
			{
				DebugLog.DebugWrite($"Warning - Language {translation.Language} has missing field of name {stringField.Name}", MessageType.Warning);
				stringField.SetValue(translation, stringField.Name);
			}
		}
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

	public static CultureInfo CultureInfo
		=> Current.Language switch
		{
			/*
			 * Language tags from BCP-47 standard, implemented by windows
			 * https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
			 * I have no fucking idea if this will work on linux. ¯\_(ツ)_/¯
			 */

			TextTranslation.Language.ENGLISH => new CultureInfo("en"),
			TextTranslation.Language.SPANISH_LA => new CultureInfo("es-419"),
			TextTranslation.Language.GERMAN => new CultureInfo("de"),
			TextTranslation.Language.FRENCH => new CultureInfo("fr"),
			TextTranslation.Language.ITALIAN => new CultureInfo("it"),
			TextTranslation.Language.POLISH => new CultureInfo("pl"),
			TextTranslation.Language.PORTUGUESE_BR => new CultureInfo("pt-BR"),
			TextTranslation.Language.JAPANESE => new CultureInfo("ja"),
			TextTranslation.Language.RUSSIAN => new CultureInfo("ru"),
			TextTranslation.Language.CHINESE_SIMPLE => new CultureInfo("zh-Hans"),
			TextTranslation.Language.KOREAN => new CultureInfo("ko"),
			TextTranslation.Language.TURKISH => new CultureInfo("tr"),
			_ => new CultureInfo("en") // what
		};
}
