namespace PushToMeowMod.Localization
{
	// Translator: @daniela@lethallava.land
	public static class GermanTranslations
	{
		public static void Initialize()
		{
			var id = InGameTranslator.LanguageID.German;

			Translator.AddTranslation("...", id, "...");
			Translator.AddTranslation("Are you taunting me?", id, "Machst du dich lustig über mich?");
			Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id,
				"...Was ist es? Ach, vergiss es, es ist mir egal...");

			Translator.AddTranslation("Yes? What's wrong?", id, "Ja? Was ist los?");
			Translator.AddTranslation("Meow?", id, "Miau?");
			Translator.AddTranslation("Meow.", id, "Miau.");
			Translator.AddTranslation("Meow...?", id, "Miau...?");
			Translator.AddTranslation("Meow!", id, "Miau!");

			Translator.AddTranslation("...I assume from your meowing that you understand me now.", id,
				"...Ich vermute von deinem Miauen, dass du mich jetzt verstehst.");
			Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id,
				"...Kannst du mit dem Schreien aufhören? Diese Information ist für dich relevant.");
			Translator.AddTranslation("Please quit that immediately.", id, "Bitte, höre sofort damit auf.");
			Translator.AddTranslation("You had your chances. Leave now.", id,
				"Du hattest deine Chance. Gehe jetzt.");
			Translator.AddTranslation("What is it?", id, "Was ist es?");
			Translator.AddTranslation("I have nothing for you. Please stop meowing.", id,
				"Ich habe nichts für dich. Bitte höre auf zu Miauen.");
			Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id,
				"Bitte höre mit dem Miauen auf, ich lese dir deine Perle.");
			Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id,
				"Wenn du weiter damit machst, werde ich nicht weiter lesen.");
		}
	}
}

