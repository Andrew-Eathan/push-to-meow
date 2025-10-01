using System;

namespace PushToMeowMod.Localization
{
	public static class LocalizationSetup
	{
		public static void InitializeTranslations()
		{
			PushToMeowMain.PLogger.LogInfo("Adding Default Translations");

			SpanishTranslations.Initialize();
			FrenchTranslations.Initialize();
			RussianTranslations.Initialize();
			PortugueseTranslations.Initialize();
			GermanTranslations.Initialize();
		}
	}
}

