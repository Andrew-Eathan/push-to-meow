namespace PushToMeowMod.Localization
{
	// Translator: @NeiDrakos
	public static class SpanishTranslations
	{
		public static void Initialize()
		{
			var id = InGameTranslator.LanguageID.Spanish;

			Translator.AddTranslation("...", id, "...");
			Translator.AddTranslation("Are you taunting me?", id, "¿Te estás burlando de mi?");
			Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id,
				"...¿Que pasa? Oh, olvidalo, no me importa...");

			Translator.AddTranslation("Yes? What's wrong?", id, "¿Si? ¿Qué ocurre?");
			Translator.AddTranslation("Meow?", id, "Meow?");
			Translator.AddTranslation("Meow.", id, "Meow.");
			Translator.AddTranslation("Meow...?", id, "Meow...?");
			Translator.AddTranslation("Meow!", id, "Meow!");

			Translator.AddTranslation("...I assume from your meowing that you understand me now.", id,
				"...voy a suponer por tus maullidos que me entiendes.");
			Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id,
				"...¿Puedes parar de gritar? Esta información es importante para ti.");
			Translator.AddTranslation("Please quit that immediately.", id, "Por favor, detente ahora mismo.");
			Translator.AddTranslation("You had your chances. Leave now.", id,
				"Tuviste tu oportunidad. Vete ahora mismo.");
			Translator.AddTranslation("What is it?", id, "¿Qué ocurre?");
			Translator.AddTranslation("I have nothing for you. Please stop meowing.", id,
				"No tengo nada para ti. Porfavor para de maullar.");
			Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id,
				"Porfavor, deja de maullar, estoy leyendo tu perla.");
			Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id,
				"Si continuas asi voy a dejar de leerla.");
		}
	}
}

