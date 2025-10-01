namespace PushToMeowMod.Localization
{
	// Translator: @thomasnet_mc
	public static class FrenchTranslations
	{
		public static void Initialize()
		{
			var id = InGameTranslator.LanguageID.French;

			Translator.AddTranslation("...", id, "...");
			Translator.AddTranslation("Are you taunting me?", id, "Tu te moques de moi ?");
			Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id,
				"...Quoi ? Pff, oublie. J'm'en fiche...");

			Translator.AddTranslation("Yes? What's wrong?", id, "Oui ? Tout va bien ?");
			Translator.AddTranslation("Meow?", id, "Miaou ?");
			Translator.AddTranslation("Meow.", id, "Miaou.");
			Translator.AddTranslation("Meow...?", id, "Miaou...?");
			Translator.AddTranslation("Meow!", id, "Miaou !");

			Translator.AddTranslation("...I assume from your meowing that you understand me now.", id,
				"..Je déduis de tes miaulements que tu me comprends, maintenant.");
			Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id,
				"...Tu pourrais arrêter avec tes cris ? C'est important pour toi, ce que je te dis.");
			Translator.AddTranslation("Please quit that immediately.", id, "Arrête ça immédiatement.");
			Translator.AddTranslation("You had your chances. Leave now.", id,
				"Tu as gâché ta chance. Pars d'ici. Maintenant.");
			Translator.AddTranslation("What is it?", id, "Quoi ?");
			Translator.AddTranslation("I have nothing for you. Please stop meowing.", id,
				"Je n'ai rien pour toi. Arrête avec tes miaulements.");
			Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id,
				"Arrête de miauler, je lis dans ta perle.");
			Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id,
				"Si tu n'arrêtes pas de miauler illico, ta perle, tu peux l'oublier !");
		}
	}
}

