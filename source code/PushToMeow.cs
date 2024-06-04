using BepInEx;
using BepInEx.Logging;
using ImprovedInput;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;

// mod idea & programmer: andreweathan
// meow sound design, thumbnail & trailer: cioss (aka cioss21)

// changelogs:
// 1.0 - everything related to meowing
// 1.0.1 - bubbling when meowing underwater, also uses up lung air and slightly lower pitched
// 1.1 - add custom meow support for custom slugcats, small bugfixes n tweaks, meow volume slider in settings, massively clean up code

namespace PushToMeowMod
{
	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	public partial class PushToMeowMain : BaseUnityPlugin
	{
		public static ManualLogSource PLogger;

		public const string PLUGIN_GUID = "pushtomeow";
		public const string PLUGIN_NAME = "Push to Meow";
		public const string PLUGIN_VERSION = "1.1";
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

		private void OnEnable()
		{
			PLogger = Logger;
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

            ModSettings = new MeowMeowOptions(this);
            MachineConnector.SetRegisteredOI("pushtomeow", ModSettings);
			Logger.LogInfo("Registered OI");

			MeowUtils.LoadCustomMeows();
        }

		// to reset all state values of Push to Meow
		private void ResetPTMStateValues(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
		{
			Logger.LogInfo("game session restart so clearing all meow meow data :)");
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
					Logger.LogInfo("meow long " + playerIdx + " " + PlayersLookup[playerIdx] + " " + timeSinceMeowPress);
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

			// play meow sound
			self.room.PlaySound(meowType, self.bodyChunks[0], false, volume, pitch);

			// alert all creatures around slugcat
			if (ModSettings.AlertCreatures.Value)
				self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10000f, self, 2f));

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

			Logger.LogInfo("play meow " + (isShortMeow ? "short" : "long") + " pitch " + pitch + " vol " + volume + "x ply " + self.SlugCatClass.value + " type " + meowType);
		}

		private void HandleMeowInput(On.Player.orig_Update orig, Player self, bool wtfIsThisBool)
		{
			orig(self, wtfIsThisBool);

			if (self.isNPC)
			{
				MeowUtils.HandleNPCSlugcat(self);
				return;
			}

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

					Debug.Log("btnpress " + plyNumber + " " + PlayersMeowButtonLastState.Count);
				}
				// button unpress
				else if (!plyPressingMeow && PlayersMeowButtonLastState[plyNumber])
				{
                    // short meow due to early release
                    if (Time.time - PlayersLastMeowTime[plyNumber] <= LongMeowTime && PlayersMeowingState[plyNumber] == MeowState.NotMeowed)
					{
						DoMeow(self, true);
						PlayersMeowingState[plyNumber] = MeowState.MeowedShort;
						Debug.Log("did short meow");
                    }

                    PlayersMeowButtonLastState[plyNumber] = false;
					Debug.Log("btnrel " + plyNumber + " " + PlayersMeowButtonLastState.Count);
                }
            }
            catch (Exception e)
			{
				Logger.LogError("error when ticking meow update: " + e);
				Debug.LogException(e);
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