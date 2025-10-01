namespace PushToMeowMod.Localization
{
	// Translator: Jas3019
	public static class RussianTranslations
	{
		public static void Initialize()
		{
			var id = InGameTranslator.LanguageID.Russian;

			Translator.AddTranslation("...", id, "...");
			Translator.AddTranslation("Are you taunting me?", id, "Ты насмехаешься надо мной?");
			Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id,
				"...Что опять? Ох, забудь, мне уже всё равно...");

			Translator.AddTranslation("Yes? What's wrong?", id, "Да? Что-то не так?");
			Translator.AddTranslation("Meow?", id, "Мяу?");
			Translator.AddTranslation("Meow.", id, "Мяу.");
			Translator.AddTranslation("Meow...?", id, "Мяу...?");
			Translator.AddTranslation("Meow!", id, "Мяу!");

			Translator.AddTranslation("...I assume from your meowing that you understand me now.", id,
				"...Насколько я слышу, ты меня понимаешь.");
			Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id,
				"...Может прекратишь кричать? Эта информация важна для тебя.");
			Translator.AddTranslation("Please quit that immediately.", id, "Прекрати сейчас же... Пожалуйста.");
			Translator.AddTranslation("You had your chances. Leave now.", id, "У тебя был шанс. Уходи.");
			Translator.AddTranslation("What is it?", id, "Что тебе нужно?");
			Translator.AddTranslation("I have nothing for you. Please stop meowing.", id,
				"У меня для тебя ничего нет. Пожалуйста, не мяукай.");
			Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id,
				"Пожалуйста прекрати мяукать, я читаю твою жемчужину.");
			Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id,
				"Если ты продолжишь то я перестану читать.");
		}
	}
}

