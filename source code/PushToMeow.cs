using BepInEx;
using BepInEx.Logging;
using ImprovedInput;
using Microsoft.Win32.SafeHandles;
using MoreSlugcats;
using Newtonsoft.Json.Linq;
using PushToMeowMod.Vanilla_Hooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

// mod idea & programmer: andreweathan
// meow sound design, thumbnail & trailer: cioss (aka cioss21)
// translation/slugpup ai programmer: vultumast
// translators: 
// @NeiDrakos - Spanish (Spain)
// @Ray261 - Portuguese (Brazil)
// Jas3019 - Russian
// @thomasnet_mc - French (France)
// @daniela@lethallava.land - German

// changelogs:
// 1.0 - everything related to meowing
// 1.0.1 - bubbling when meowing underwater, also uses up lung air and slightly lower pitched
// 1.1 - add custom meow support for custom slugcats, small bugfixes n tweaks, meow volume slider in settings, massively clean up code
// 1.1.1 - add saint meow, add slugpup meowing
// 1.2.0 - add translations, add slugpup ai
// 1.2.1 - better slugpup ai, more settings toggles, and pups+ support, all by vultumast

namespace PushToMeowMod
{
	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	public partial class PushToMeowMain : BaseUnityPlugin
	{
		public static ManualLogSource PLogger { get; internal set; } = null;
        public static RainWorld RainWorld { get; internal set; } = null;

		public const string ROTUND_WORLD_IDENTIFER = "willowwisp.bellyplus";

		public const string PLUGIN_GUID = "pushtomeow";
		public const string PLUGIN_NAME = "Push to Meow";
        public const string PLUGIN_VERSION = "1.2.1";
		public const float LongMeowTime = 0.14f; // seconds needed to hold so that long meow plays
		public const float MeowCooldown = 0.24f; // seconds between meows

		public static PlayerKeybind Meow;
		public static MeowMeowOptions ModSettings;

		// this stores the last checked state of the meow button for a given player
		public Dictionary<int, bool> PlayersMeowButtonLastState = new Dictionary<int, bool>();

		// this stores the state of the player's meow being played, this gets reset once the button is let go
		public Dictionary<int, MeowState> PlayersMeowingState = new Dictionary<int, MeowState>();

		// this stores the last time the meow button was pressed per player, used for cooldowns
		public Dictionary<int, float> PlayersLastMeowTime = new Dictionary<int, float>();

		// stores a quick lookup table for players used to access the player entity to handle meow button release
		public Dictionary<int, Player> PlayersLookup = new Dictionary<int, Player>();

		public static bool RotundWorldSupportEnabled { get; private set; } = false;

		private void OnEnable()
		{
			PLogger = Logger;
			MeowUtils.PushToMeowPlugin = this;
			MeowUtils.InitialiseSoundIDs(Logger);

			// for good measure during hot reload
			On.RainWorldGame.RestartGame += ResetPTMStateValues;
			On.Player.Update += HandleMeowInput;
			On.RainWorld.OnModsInit += RainWorld_OnModsInit;

			if (Meow == null)
				Meow = PlayerKeybind.Get("pushtomeow:meow");

			if (Meow == null)
				Meow = PlayerKeybind.Register("pushtomeow:meow", "Push to Meow", "Meow", KeyCode.M, KeyCode.JoystickButton3);

            Logger.LogInfo("READY TO RRRRUMBL- i mean Meow Meow Meow Meow :3333"); // :3
		}

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
		{
            orig(self);

			RainWorld = self;
			
		
            Vanilla_Hooks.SlugNPCMeowAI.RainWorld = self;

            ModSettings = new MeowMeowOptions(this);
			MachineConnector.SetRegisteredOI(PLUGIN_GUID, ModSettings);
			Logger.LogInfo("Registered OI");

			MeowUtils.LoadCustomMeows();
			Logger.LogInfo("loaded custom meows :3");

            Vanilla_Hooks.SlugNPCMeowAI.AttachHooks();

            SetDefaultTranslations();

            for (int i = 0; i < ModManager.ActiveMods.Count; i++)
			{
				if (ModManager.ActiveMods[i].id == ROTUND_WORLD_IDENTIFER)
				{
					RotundWorldSupportEnabled = true;
					Logger.LogInfo("found rotund world, enabling support in PtM :)");
				}
			}
		}

        private void SetDefaultTranslations()
		{
            Logger.LogInfo("Adding Default Translations");

            SetSpanish();
			SetFrench();
			SetRussian();
			SetPortuguese();
			SetGerman();

			return;

			void SetSpanish()
			{
                var id = InGameTranslator.LanguageID.Spanish;

                Translator.AddTranslation("...", id, "...");
                Translator.AddTranslation("Are you taunting me?", id, "¿Te estás burlando de mi?");
                Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id, "...¿Que pasa? Oh, olvidalo, no me importa...");

                Translator.AddTranslation("Yes? What's wrong?", id, "¿Si? ¿Qué ocurre?");
                Translator.AddTranslation("Meow?", id, "Meow?");
                Translator.AddTranslation("Meow.", id, "Meow.");
                Translator.AddTranslation("Meow...?", id, "Meow...?");
                Translator.AddTranslation("Meow!", id, "Meow!");

                Translator.AddTranslation("...I assume from your meowing that you understand me now.", id, "...voy a suponer por tus maullidos que me entiendes.");
                Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id, "...¿Puedes parar de gritar? Esta información es importante para ti.");
                Translator.AddTranslation("Please quit that immediately.", id, "Por favor, detente ahora mismo.");
                Translator.AddTranslation("You had your chances. Leave now.", id, "Tuviste tu oportunidad. Vete ahora mismo.");
                Translator.AddTranslation("What is it?", id, "¿Qué ocurre?");
                Translator.AddTranslation("I have nothing for you. Please stop meowing.", id, "No tengo nada para ti. Porfavor para de maullar.");
                Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id, "Porfavor, deja de maullar, estoy leyendo tu perla.");
                Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id, "Si continuas asi voy a dejar de leerla.");
            }

            void SetFrench()
            {
                var id = InGameTranslator.LanguageID.French;

                Translator.AddTranslation("...", id, "...");
                Translator.AddTranslation("Are you taunting me?", id, "Tu te moques de moi ?");
                Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id, "...Quoi ? Pff, oublie. J'm'en fiche...");

                Translator.AddTranslation("Yes? What's wrong?", id, "Oui ? Tout va bien ?");
                Translator.AddTranslation("Meow?", id, "Miaou ?");
                Translator.AddTranslation("Meow.", id, "Miaou.");
                Translator.AddTranslation("Meow...?", id, "Miaou...?");
                Translator.AddTranslation("Meow!", id, "Miaou !");

                Translator.AddTranslation("...I assume from your meowing that you understand me now.", id, "..Je déduis de tes miaulements que tu me comprends, maintenant.");
                Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id, "...Tu pourrais arrêter avec tes cris ? C'est important pour toi, ce que je te dis.");
                Translator.AddTranslation("Please quit that immediately.", id, "Arrête ça immédiatement.");
                Translator.AddTranslation("You had your chances. Leave now.", id, "Tu as gâché ta chance. Pars d'ici. Maintenant.");
                Translator.AddTranslation("What is it?", id, "Quoi ?");
                Translator.AddTranslation("I have nothing for you. Please stop meowing.", id, "Je n'ai rien pour toi. Arrête avec tes miaulements.");
                Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id, "Arrête de miauler, je lis dans ta perle.");
                Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id, "Si tu n'arrêtes pas de miauler illico, ta perle, tu peux l'oublier !");
            }

            void SetRussian()
            {
                var id = InGameTranslator.LanguageID.Russian;

                Translator.AddTranslation("...", id, "...");
                Translator.AddTranslation("Are you taunting me?", id, "Ты насмехаешься надо мной?");
                Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id, "...Что опять? Ох, забудь, мне уже всё равно...");

                Translator.AddTranslation("Yes? What's wrong?", id, "Да? Что-то не так?");
                Translator.AddTranslation("Meow?", id, "Мяу?");
                Translator.AddTranslation("Meow.", id, "Мяу.");
                Translator.AddTranslation("Meow...?", id, "Мяу...?");
                Translator.AddTranslation("Meow!", id, "Мяу!");

                Translator.AddTranslation("...I assume from your meowing that you understand me now.", id, "...Насколько я слышу, ты меня понимаешь.");
                Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id, "...Может прекратишь кричать? Эта информация важна для тебя.");
                Translator.AddTranslation("Please quit that immediately.", id, "Прекрати сейчас же... Пожалуйста.");
                Translator.AddTranslation("You had your chances. Leave now.", id, "У тебя был шанс. Уходи.");
                Translator.AddTranslation("What is it?", id, "Что тебе нужно?");
                Translator.AddTranslation("I have nothing for you. Please stop meowing.", id, "У меня для тебя ничего нет. Пожалуйста, не мяукай.");
                Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id, "Пожалуйста прекрати мяукать, я читаю твою жемчужину.");
                Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id, "Если ты продолжишь то я перестану читать.");
            }

            void SetPortuguese()
            {
                var id = InGameTranslator.LanguageID.Portuguese;

                Translator.AddTranslation("...", id, "...");
                Translator.AddTranslation("Are you taunting me?", id, "Você está tentando me provocar?");
                Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id, "...O que foi? Ah, esqueça, eu não me importo...");

                Translator.AddTranslation("Yes? What's wrong?", id, "Sim? Tá tudo bem?");
                Translator.AddTranslation("Meow?", id, "Miau?");
                Translator.AddTranslation("Meow.", id, "Miau.");
                Translator.AddTranslation("Meow...?", id, "Miau...?");
                Translator.AddTranslation("Meow!", id, "Miau!");

                Translator.AddTranslation("...I assume from your meowing that you understand me now.", id, "...Eu assumo pelos seus miados que você possa me entender agora.");
                Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id, "...Você pode parar com os seus miados? Essa informação é importante para você.");
                Translator.AddTranslation("Please quit that immediately.", id, "Pare com isso imediatamente.");
                Translator.AddTranslation("You had your chances. Leave now.", id, "Você teve sua chance. Saia imediatamente.");
                Translator.AddTranslation("What is it?", id, "O que é?");
                Translator.AddTranslation("I have nothing for you. Please stop meowing.", id, "Eu não tenho nada para você. Por favor, pare de miar.");
                Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id, "Por favor, pare de miar, Eu estou lendo sua pérola.");
                Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id, "Se você continuar fazendo isso, eu não vou continuar lendo.");
            }
            void SetGerman()
            {
                var id = InGameTranslator.LanguageID.German;

                Translator.AddTranslation("...", id, "...");
                Translator.AddTranslation("Are you taunting me?", id, "Machst du dich lustig über mich?");
                Translator.AddTranslation("...What is it? Oh, forget it, I don't care...", id, "...Was ist es? Ach, vergiss es, es ist mir egal...");

                Translator.AddTranslation("Yes? What's wrong?", id, "Ja? Was ist los?");
                Translator.AddTranslation("Meow?", id, "Miau?");
                Translator.AddTranslation("Meow.", id, "Miau.");
                Translator.AddTranslation("Meow...?", id, "Miau...?");
                Translator.AddTranslation("Meow!", id, "Miau!");

                Translator.AddTranslation("...I assume from your meowing that you understand me now.", id, "...Ich vermute von deinem Miauen, dass du mich jetzt verstehst.");
                Translator.AddTranslation("...Can you stop with the yells? This information is relevant to you.", id, "...Kannst du mit dem Schreien aufhören? Diese Information ist für dich relevant.");
                Translator.AddTranslation("Please quit that immediately.", id, "Bitte, höre sofort damit auf.");
                Translator.AddTranslation("You had your chances. Leave now.", id, "Du hattest deine Chance. Gehe jetzt.");
                Translator.AddTranslation("What is it?", id, "Was ist es?");
                Translator.AddTranslation("I have nothing for you. Please stop meowing.", id, "Ich habe nichts für dich. Bitte höre auf zu Miauen.");
                Translator.AddTranslation("Please quit meowing, I'm reading you your pearl.", id, "Bitte höre mit dem Miauen auf, ich lese dir deine Perle.");
                Translator.AddTranslation("If you keep going with this, I will not continue reading it.", id, "Wenn du weiter damit machst, werde ich nicht weiter lesen.");
            }
        }


		// to reset all state values of Push to Meow
		private void ResetPTMStateValues(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
		{
#if DEBUG
			Logger.LogInfo("game session restart so clearing all meow meow data :)");
#endif
			PlayersMeowButtonLastState?.Clear();
			PlayersLastMeowTime?.Clear();
			PlayersLookup?.Clear();
			PlayersMeowingState?.Clear();

			orig(self);
		}

		// if holding meow button for 0.14s, long meow plays
		// if player lets go of meow button before 0.14s, short meow plays
		public void Update()
		{
			foreach (KeyValuePair<int, bool> kv in PlayersMeowButtonLastState)
			{
				int playerIdx = kv.Key;
				bool meowButtonState = kv.Value;
				float timeSinceMeowPress = Time.time - PlayersLastMeowTime[playerIdx];

				// if meow button is still pressed after time is over, then do a long meow 
				if (
					meowButtonState 
					&& timeSinceMeowPress > LongMeowTime 
					&& PlayersMeowingState[playerIdx] == MeowState.NotMeowed
				)  {
					DoMeow(PlayersLookup[playerIdx]);
					PlayersMeowingState[playerIdx] = MeowState.MeowedLong;
#if DEBUG
					Logger.LogInfo("meow long " + playerIdx + " " + PlayersLookup[playerIdx] + " " + timeSinceMeowPress);
#endif
				}
			}
		}

		public void DoMeow(Player self, bool isShortMeow = false)
		{
			// mute ahh mf
			if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				if (!ModSettings.SpearmasterMeow.Value)
					return;

				MeowUtils.DoSpearmasterTailWiggle(self);
			}

			if (!MeowUtils.CanMeow(self)) 
				return;

			MeowUtils.HandleOracleReactions(self);
			MeowUtils.DoMeowAnim(self, isShortMeow);

			(SoundID meowType, float pitch, float volume) = MeowUtils.FindMeowSoundID(self, isShortMeow);

			if (RotundWorldSupportEnabled)
			{
				// find base chunk weight of a slugcat's body chunk
				float baseChunkWeight = (0.7f * self.slugcatStats.bodyWeightFac) / 2f;

				// find the fattest body chunk's mass
				float fattestChunkMass = 0;
				foreach (var chunk in self.bodyChunks)
					fattestChunkMass = Mathf.Max(fattestChunkMass, chunk.mass);

				float pitchChange = Mathf.Clamp((fattestChunkMass - baseChunkWeight) / 1.35f, 0, 0.55f);
#if DEBUG
				Logger.LogInfo("ROTUND WORLD COMPAT: lowered pitch to " + (pitch - pitchChange) + " from " + pitch);
#endif
				pitch -= pitchChange;
			}

			if (!self.isNPC || (self.isNPC && ModSettings.SlugpupAlertMeow.Value))
			{
				// play meow sound
				try
				{
					self.room.PlaySound(meowType, self.bodyChunks[0], false, volume, pitch);
				}
				catch (Exception e)
				{
					Logger.LogError("Couldn't play meow type " + meowType + " for slugcat ID " + self.SlugCatClass.value + ", possible SoundID issues? Check your custom meow sound mod's modify/soundeffects/sounds.txt, make sure your .wav files have no underscores or other symbols!");
					Logger.LogError(e);
				}

				// alert all creatures around slugcat
				if (ModSettings.AlertCreatures.Value)
					self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10000f, self, 2f));
			}
			// drain slugcat's lungs a little (unless theyre rivulet)
			if (ModSettings.DrainLungs.Value)
				if (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
					self.airInLungs -= 0.08f + (isShortMeow ? 0 : 0.08f);

			// make little bubbles if submerged
			if (self.submerged)
				for (int i = 0; i < 2 + (isShortMeow ? 0 : 1); i++)
				{
					// for spearmaster, bubbles should come from tail
					if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear) 
					{
						if (self.graphicsModule is PlayerGraphics gm)
							self.room.AddObject(new Bubble(gm.tail[2].pos, gm.tail[2].vel, false, false));
					}
					else self.room.AddObject(new Bubble(self.firstChunk.pos, self.firstChunk.vel, false, false));
				}
        }

        private void HandleMeowInput(On.Player.orig_Update orig, Player self, bool wtfIsThisBool)
		{
			orig(self, wtfIsThisBool);

			if (self.isNPC)
				return;

			try
			{
				int plyNumber = self.playerState.playerNumber;
				bool plyPressingMeow = Meow.CheckRawPressed(plyNumber);

				if (!PlayersMeowButtonLastState.ContainsKey(plyNumber))
					PlayersMeowButtonLastState.Add(plyNumber, false);
				if (!PlayersLastMeowTime.ContainsKey(plyNumber))
					PlayersLastMeowTime.Add(plyNumber, 0);
				if (!PlayersMeowingState.ContainsKey(plyNumber))
					PlayersMeowingState.Add(plyNumber, MeowState.NotMeowed);
				if (!PlayersLookup.ContainsKey(plyNumber))
					PlayersLookup.Add(plyNumber, self);

				PlayersLookup[plyNumber] = self; // always assign the player to avoid bugs that happen after respawning

				// button press
				if (plyPressingMeow && !PlayersMeowButtonLastState[plyNumber])
				{
					if (Time.time - PlayersLastMeowTime[plyNumber] < MeowCooldown) return;

					PlayersMeowButtonLastState[plyNumber] = true;
					PlayersMeowingState[plyNumber] = MeowState.NotMeowed;
					PlayersLastMeowTime[plyNumber] = Time.time; // to check later

					//Debug.Log("btnpress " + plyNumber + " " + PlayersMeowButtonLastState.Count);
				}
				// button unpress
				else if (!plyPressingMeow && PlayersMeowButtonLastState[plyNumber])
				{
					// short meow due to early release
					if (Time.time - PlayersLastMeowTime[plyNumber] <= LongMeowTime && PlayersMeowingState[plyNumber] == MeowState.NotMeowed)
					{
						DoMeow(self, true);
						PlayersMeowingState[plyNumber] = MeowState.MeowedShort;
#if DEBUG
                        Logger.LogDebug("did short meow");
#endif
					}

					PlayersMeowButtonLastState[plyNumber] = false;
					//Debug.Log("btnrel " + plyNumber + " " + PlayersMeowButtonLastState.Count);
				}
			}
			catch (Exception e)
			{
				Logger.LogError("error when ticking meow update: " + e);
#if DEBUG
				Logger.LogDebug(e);
#endif
			}
		}
	}
}

// just dropping this here in hopes of the mod working :)
// JOHN 1
// The Word Became Flesh
// 1In the beginning was the Word, and the Word was with God, and the Word was God. 2He was with God in the beginning.
// 3Through him all things were made; without him nothing was made that has been made. 4In him was life, and that life
// was the light of all mankind. 5The light shines in the darkness, and the darkness has not overcome it.
// 6There was a man sent from God whose name was John. 7He came as a witness to testify concerning that light, so that
// through him all might believe. 8He himself was not the light; he came only as a witness to the light.
// 9The true light that gives light to everyone was coming into the world. 10He was in the world, and though the world
// was made through him, the world did not recognize him. 11He came to that which was his own, but his own did not receive
// him. 12Yet to all who did receive him, to those who believed in his name, he gave the right to become children of
// God—13children born not of natural descent, nor of human decision or a husband’s will, but born of God.
// 14The Word became flesh and made his dwelling among us. We have seen his glory, the glory of the one and only Son,
// who came from the Father, full of grace and truth.
// 15(John testified concerning him. He cried out, saying, “This is the one I spoke about when I said, ‘He who comes
// after me has surpassed me because he was before me.’ ”) 16Out of his fullness we have all received grace in place of
// grace already given. 17For the law was given through Moses; grace and truth came through Jesus Christ. 18No one has ever
// seen God, but the one and only Son, who is himself God and is in closest relationship with the Father, has made him known.
// John the Baptist Denies Being the Messiah
// 19Now this was John’s testimony when the Jewish leaders in Jerusalem sent priests and Levites to ask him who he was.
// 20He did not fail to confess, but confessed freely, “I am not the Messiah.”
// 21They asked him, “Then who are you? Are you Elijah?”
// He said, “I am not.”
// “Are you the Prophet?”
// He answered, “No.”
// 22Finally they said, “Who are you? Give us an answer to take back to those who sent us. What do you say about yourself?”
// 23John replied in the words of Isaiah the prophet, “I am the voice of one calling in the wilderness, ‘Make straight the
// way for the Lord.’ ”
// 24Now the Pharisees who had been sent 25questioned him, “Why then do you baptize if you are not the Messiah, nor Elijah,
// nor the Prophet?”
// 26“I baptize with water,” John replied, “but among you stands one you do not know. 27He is the one who comes after me,
// the straps of whose sandals I am not worthy to untie.”
// 28This all happened at Bethany on the other side of the Jordan, where John was baptizing.