using QSB.Localization;
using System;

namespace QSB.DeathSync;

public static class Necronomicon
{
	public static string GetPhrase(DeathType deathType, int index)
		=> QSBLocalization.Current.DeathMessages.ContainsKey(deathType)
			? QSBLocalization.Current.DeathMessages[deathType][index]
			: null;

	public static int GetRandomIndex(DeathType deathType)
		=> QSBLocalization.Current.DeathMessages.ContainsKey(deathType)
			? new Random().Next(0, QSBLocalization.Current.DeathMessages[deathType].Length)
			: -1;
}