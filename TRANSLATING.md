# Translating QSB

## Progress :

QSB can only be translated to the languages Outer Wilds supports - so if you don't see your language here, contact Mobius Digital about translating the game to that language.

### Translated languages :
- English
- French
- Russian
- Portuguese (Brazil)
- German

### Un-translated languages :
- Spanish (Latin American)
- Italian
- Polish
- Japanese
- Chinese (Simplified)
- Korean
- Turkish

## Translating

### Creating the translation file

Create a new file, and name it `(language).json`. For example, you could have `de.json` or `german.json`. The name doesn't have to follow any rules, just as long as it's recognisable as that language.

Copy the contents of `en.json` into your file. `en.json` can be found [here](https://github.com/misternebula/quantum-space-buddies/blob/dev/QSB/Translations/en.json) on GitHub, or alternatively in the mod install - `Mods\QSB\Translations\en.json`

Go through each of the English items and translate them into your language.

You can test your translation by putting your file in `Mods\QSB\Translations\`, alongside the other translation files.

Note: The "Language" field at the top should be set as one of these options : `ENGLISH`, `SPANISH_LA`, `GERMAN`, `FRENCH`, `ITALIAN`, `POLISH`, `PORTUGUESE_BR`, `JAPANESE`, `RUSSIAN`, `CHINESE_SIMPLE`, `KOREAN`, `TURKISH`

Once you are happy with it, go to [this page](https://github.com/misternebula/quantum-space-buddies/new/dev/QSB/Translations) to create a PR with your translation file. In the "Name your file..." box put your file name and extension (e.g. `de.json`), and in the text box copy and paste the contents of your translation file.

Scroll down to the "Propose new file" box. In the title box (default "Create new file") write something along the lines of "Add translation for (language)".

Press the big green "Propose new file", and we'll review it soon!