namespace PushToMeowMod.Localization
{
	// Translator: @Ray261
	public static class PortugueseTranslations
	{
		public static void Initialize()
		{
			var id = InGameTranslator.LanguageID.Portuguese;

			Translator.AddTranslation("...", id, "...");
			Translator.AddTranslation("Are you taunting me?", id, "Você está tentando me provocar?");
			Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id,
				"...O que foi? Ah, esqueça, eu não me importo...");

			Translator.AddTranslation("Yes? What's wrong?", id, "Sim? Tá tudo bem?");
			Translator.AddTranslation("Meow?", id, "Miau?");
			Translator.AddTranslation("Meow.", id, "Miau.");
			Translator.AddTranslation("Meow...?", id, "Miau...?");
			Translator.AddTranslation("Meow!", id, "Miau!");

			Translator.AddTranslation("...I assume from your meowing that you understand me now.", id,
				"...Eu assumo pelos seus miados que você possa me entender agora.");
			Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id,
				"...Você pode parar com os seus miados? Essa informação é importante para você.");
			Translator.AddTranslation("Please quit that immediately.", id, "Pare com isso imediatamente.");
			Translator.AddTranslation("You had your chances. Leave now.", id,
				"Você teve sua chance. Saia imediatamente.");
			Translator.AddTranslation("What is it?", id, "O que é?");
			Translator.AddTranslation("I have nothing for you. Please stop meowing.", id,
				"Eu não tenho nada para você. Por favor, pare de miar.");
			Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id,
				"Por favor, pare de miar, Eu estou lendo sua pérola.");
			Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id,
				"Se você continuar fazendo isso, eu não vou continuar lendo.");
		}
	}
}

