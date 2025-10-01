using BepInEx.Logging;
using System.Collections.Generic;

namespace PushToMeowMod
{
	public static class Translator
	{
		private static Dictionary<InGameTranslator.LanguageID, Dictionary<string, string>> _translations =
			new Dictionary<InGameTranslator.LanguageID, Dictionary<string, string>>();

		public static ManualLogSource Logger => PushToMeowMain.PLogger;
		public static RainWorld RainWorld => PushToMeowMain.RainWorld;

		public static bool AddTranslation(string englishText, InGameTranslator.LanguageID languageID, string translatedText)
		{
			if (languageID == InGameTranslator.LanguageID.English)
				return true;

			if (!_translations.ContainsKey(languageID))
			{
				Logger.LogInfo($"Added LanguageID: \"{languageID.value}\"");
				_translations.Add(languageID, new Dictionary<string, string>());
			}

			if (_translations[languageID].ContainsKey(englishText))
			{
				Logger.LogWarning($"Unable to add translation, it is already defined. Language: \"{languageID.value}\" Text: \"{englishText}\" -> \"{translatedText}\"");
				return false;
			}

			_translations[languageID].Add(englishText, translatedText);
			return true;
		}

		public static bool RemoveTranslation(string englishText, InGameTranslator.LanguageID languageID)
		{
			if (languageID == InGameTranslator.LanguageID.English)
				return true;

			if (!_translations.ContainsKey(languageID))
				return false;

			return _translations[languageID].Remove(englishText);
		}

		public static bool ContainsLanguageID(InGameTranslator.LanguageID languageID) =>
			languageID == InGameTranslator.LanguageID.English || _translations.ContainsKey(languageID);

		public static bool ContainsTranslation(InGameTranslator.LanguageID languageID, string englishText) =>
			languageID == InGameTranslator.LanguageID.English || _translations[languageID].ContainsKey(englishText);

		public static bool TryTranslate(string englishText, InGameTranslator.LanguageID languageID, out string translated)
		{
			if (languageID == InGameTranslator.LanguageID.English)
			{
				translated = englishText;
				return true;
			}

			if (string.IsNullOrWhiteSpace(englishText))
			{
				translated = "!! NO MEOW MOD TRANSLATION !!";
				return false;
			}

			if (!ContainsLanguageID(languageID) || !ContainsTranslation(languageID, englishText))
			{
#if DEBUG
				Logger.LogWarning($"No translation found for: \"{englishText}\" in \"{languageID.value}\"");
#endif
				translated = englishText;
				return false;
			}

			translated = _translations[languageID][englishText];
			return true;
		}

		public static string Translate(string englishText, InGameTranslator.LanguageID languageID)
		{
			if (languageID == InGameTranslator.LanguageID.English)
				return englishText;

			if (string.IsNullOrWhiteSpace(englishText))
				return "!! NO MEOW MOD TRANSLATION !!";

			if (!ContainsLanguageID(languageID) || !ContainsTranslation(languageID, englishText))
			{
#if DEBUG
				Logger.LogWarning($"No translation found for: \"{englishText}\" in \"{languageID.value}\"");
#endif
				return englishText;
			}

			return _translations[languageID][englishText];
		}

		public static bool TryTranslate(string text, out string translated) =>
			TryTranslate(text, RainWorld.inGameTranslator.currentLanguage, out translated);

		public static string Translate(string text) =>
			Translate(text, RainWorld.inGameTranslator.currentLanguage);
	}
}
