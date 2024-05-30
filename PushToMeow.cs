using BepInEx;
using IL.Menu;
using ImprovedInput;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;

// mod idea & programmer: andreweathan
// meow sound design & thumbnail: cioss (aka cioss21)

namespace PushToMeowMod
{
	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	public class PushToMeowMain : BaseUnityPlugin
	{
		public const string PLUGIN_GUID = "pushtomeow"; // This should be the same as the id in modinfo.json!
		public const string PLUGIN_NAME = "Push to Meow"; // This should be a human-readable version of your mod's name. This is used for log files and also displaying which mods get loaded. In general, it's a good idea to match this with your modinfo.json as well.
		public const string PLUGIN_VERSION = "1.69"; // This follows semantic versioning. For more information, see https://semver.org/ - again, match what you have in modinfo.json
		public const float LongMeowTime = 0.14f; // seconds needed to hold so that long meow plays
		public const float MeowCooldown = 0.24f; // seconds between meows

		public static readonly PlayerKeybind Meow = PlayerKeybind.Register("pushtomeow:meow", "Push to Meow", "Meow", KeyCode.M, KeyCode.JoystickButton0);

		public static SoundID SlugcatMeowNormal { get; private set; }
		public static SoundID SlugcatMeowNormalShort { get; private set; }
		public static SoundID SlugcatMeowFat { get; private set; }
		public static SoundID SlugcatMeowFatShort { get; private set; }
		public static SoundID SlugcatMeowPup { get; private set; }
		public static SoundID SlugcatMeowPupShort { get; private set; }
		public static SoundID SlugcatMeowCoarse { get; private set; }
		public static SoundID SlugcatMeowCoarseShort { get; private set; }
		public static SoundID SlugcatMeowRivuletA { get; private set; }
		public static SoundID SlugcatMeowRivuletAShort { get; private set; }
		public static SoundID SlugcatMeowRivuletB { get; private set; }
		public static SoundID SlugcatMeowRivuletBShort { get; private set; }
		public static SoundID SlugcatMeowSofanthiel { get; private set; }
		public static SoundID SlugcatMeowSofanthielShort { get; private set; }

		private MeowMeowOptions options;

		public PushToMeowMain()
		{
			options = new MeowMeowOptions(this);
		}

		public enum MeowState
		{
			NotMeowed,
			MeowedLong,
			MeowedShort
		}

		private Dictionary<int, bool> _lastPressState = new Dictionary<int, bool>();
		private Dictionary<int, MeowState> _lastPressMeowState = new Dictionary<int, MeowState>();
		private Dictionary<int, float> _lastPressTime = new Dictionary<int, float>();
		private Dictionary<int, Player> _lastPressPlayerLookup = new Dictionary<int, Player>();

		private void OnEnable()
		{
			On.RainWorld.OnModsInit += Register;
			On.RainWorldGame.RestartGame += ResetValues;

			On.Player.Update += Player_Update;

			Logger.LogInfo("READY TO RRRRUMBL- i mean Meow Meow Meow Meow :3333"); // :3
		}

		private void ResetValues(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
		{
			_lastPressState?.Clear();
			_lastPressTime?.Clear();
			_lastPressPlayerLookup?.Clear();
			_lastPressMeowState?.Clear();

			Logger.LogInfo("game session restart so clear all meow meow data :)");
			orig(self);
		}

		private void Register(On.RainWorld.orig_OnModsInit orig, RainWorld self)
		{
			try
			{
				SlugcatMeowRivuletA = new SoundID("slugcatmeowrivuletA", true);
				SlugcatMeowRivuletB = new SoundID("slugcatmeowrivuletB", true);
				SlugcatMeowNormal = new SoundID("slugcatmeownormal", true);
				SlugcatMeowSofanthiel = new SoundID("slugcatmeowsofanthiel", true);
				SlugcatMeowFat = new SoundID("slugcatmeowfat", true); // gourmand (obviously (fat))
				SlugcatMeowPup = new SoundID("slugcatmeowpup", true);
				SlugcatMeowCoarse = new SoundID("slugcatmeowcoarse", true);

				SlugcatMeowRivuletAShort = new SoundID("slugcatmeowrivuletAshort", true);
				SlugcatMeowRivuletBShort = new SoundID("slugcatmeowrivuletBshort", true);
				SlugcatMeowNormalShort = new SoundID("slugcatmeownormalshort", true);
				SlugcatMeowSofanthielShort = new SoundID("slugcatmeowsofanthielshort", true);
				SlugcatMeowFatShort = new SoundID("slugcatmeowfatshort", true);
				SlugcatMeowPupShort = new SoundID("slugcatmeowpupshort", true);
				SlugcatMeowCoarseShort = new SoundID("slugcatmeowcoarseshort", true);

				MachineConnector.SetRegisteredOI("pushtomeow", options);
			}
			catch (Exception e)
			{
				Logger.LogError("couldnt load slugcat meow mod :( " + e.Message);
				Debug.LogException(e);
			}

			orig(self);
		}

		// if holding meow button for 0.3s, long meow plays
		// if player lets go of meow button before 0.3s, short meow plays

		public void Update()
		{
			foreach (KeyValuePair<int, bool> kv in _lastPressState)
			{
				float timeSinceMeowStart = Time.time - _lastPressTime[kv.Key];

				// if meow pressed and time is over short meow time
				if (kv.Value && timeSinceMeowStart > LongMeowTime && _lastPressMeowState[kv.Key] == MeowState.NotMeowed)
				{
					Logger.LogInfo("meow long " + kv.Key + " " + _lastPressPlayerLookup[kv.Key] + " " + timeSinceMeowStart);
					DoMeow(_lastPressPlayerLookup[kv.Key]);
					_lastPressMeowState[kv.Key] = MeowState.MeowedLong;
				}
			}
		}

		private void HandleOracleReactions(Player self)
		{
			foreach (List<PhysicalObject> objList in self.room.physicalObjects) // why is that an array of lists? :sob:
			{
				foreach (PhysicalObject obj in objList)
				{
					if (obj is Oracle)
					{
						Oracle oracle = obj as Oracle;
						MeowOracleReactions.HandleThisOraclesReaction(self, oracle, Logger);
					}
				}
			}
		}

		public void DoMeow(Player self, bool shortMeow = false)
		{
			if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear) return; // mute ahh mf

			bool isGrabbedByNonPlayer = self.grabbedBy.Count > 0 && !(self.grabbedBy[0].grabber is Player);

			// allow player to meow if they're all ok or if they're grabbed by a lizard or something
			if (self.dead && !isGrabbedByNonPlayer) // what a boubou
			{
				Logger.LogInfo("dead so no meow >:( " + self + " " + self.Consious + " " + self.playerState.playerNumber);
				return;
			}

			HandleOracleReactions(self);

			Logger.LogInfo(self.grabbedBy.Count);

			if (self.graphicsModule is PlayerGraphics gm)
			{
				// looks up after 170 ms, small delay
				Timer lookUpTimer = 
					new Timer(33) { AutoReset = false, Enabled = true };
				lookUpTimer.Elapsed += (object _, ElapsedEventArgs e) =>
				{
					gm?.LookAtPoint(new Vector2(0, 100000), 69420);
					self.Blink(shortMeow ? 9 : 11);
				};

				Timer resetLookTimer = 
					new Timer(33 + (shortMeow ? 160 : 260)) { AutoReset = false, Enabled = true };
				resetLookTimer.Elapsed += (object _, ElapsedEventArgs e) => gm.LookAtNothing();
			}


			SoundID meowType;
			float pitch = Mathf.Clamp(1 - (self.slugcatStats.bodyWeightFac - 1) * 1.2f, 0.4f, 2);
			bool isPup = self.playerState.isPup;

			switch (self.SlugCatClass.value) // i'd use switch directly on SlugCatClass with slugcat name extenums but i cant bc it wants constant cases
			{
				case "Artificer": // coarse meow for artificer
					{
						meowType = shortMeow ? SlugcatMeowCoarseShort : SlugcatMeowCoarse;

						if (isPup) // higher pitch for slugpup
							pitch = 1.5f;
						else pitch = 1f;
					}
					break;
				case "Rivulet":
					{
						var SlugcatMeowRivuletShort = options.AltRivuletSounds.Value ? SlugcatMeowRivuletBShort : SlugcatMeowRivuletAShort;
						var SlugcatMeowRivulet = options.AltRivuletSounds.Value ? SlugcatMeowRivuletB : SlugcatMeowRivuletA;
						meowType = shortMeow ? SlugcatMeowRivuletShort : SlugcatMeowRivulet;

						if (isPup) // higher pitch for slugpup
							pitch = 1.2f;
						else pitch = 1f;
					}
					break;
                case "Gourmand":
                    {
                        meowType = shortMeow ? SlugcatMeowFatShort : SlugcatMeowFat;

                        if (isPup) // higher pitch for slugpup
                            pitch = 1.3f;
                        else pitch = 1f;
                    }
                    break;
				case "Inv": // sofanthiel
                    {
                        meowType = shortMeow ? SlugcatMeowSofanthielShort : SlugcatMeowSofanthiel;

                        if (isPup) // higher pitch for slugpup
                            pitch = 1.3f;
                        else pitch = 1f;
                    }
                    break;
                default:
					{
						if (isPup) // pup meow
							meowType = shortMeow ? SlugcatMeowPupShort : SlugcatMeowPup;
						else
						{
							// normal meow
							meowType = shortMeow ? SlugcatMeowNormalShort : SlugcatMeowNormal;
						}
					}
					break;
			}

			if (isGrabbedByNonPlayer)
				pitch += 0.2f + UnityEngine.Random.value * 0.15f;

			self.room.PlaySound(meowType, self.bodyChunks[0], false, 0.7f, pitch);
			self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10000f, self, 2f));

			Logger.LogInfo("play meow " + (shortMeow ? "short" : "long") + " pitch " + pitch + " ply " + self + " type " + meowType);
		}

		private void Player_Update(On.Player.orig_Update orig, Player self, bool wtfIsThisBool)
		{
			orig(self, wtfIsThisBool);

			try
			{
				int plyNumber = self.playerState.playerNumber;
				bool plyPressingMeow = Meow.CheckRawPressed(plyNumber);

				if (!_lastPressState.ContainsKey(plyNumber))
					_lastPressState.Add(plyNumber, false);
				if (!_lastPressTime.ContainsKey(plyNumber))
					_lastPressTime.Add(plyNumber, 0);
				if (!_lastPressMeowState.ContainsKey(plyNumber))
					_lastPressMeowState.Add(plyNumber, MeowState.NotMeowed);
				if (!_lastPressPlayerLookup.ContainsKey(plyNumber))
					_lastPressPlayerLookup.Add(plyNumber, self);

				_lastPressPlayerLookup[plyNumber] = self; // always assign the player to avoid bugs that happen after respawning

				// button press
				if (plyPressingMeow && !_lastPressState[plyNumber])
				{
					if (Time.time - _lastPressTime[plyNumber] < MeowCooldown) return;

					_lastPressState[plyNumber] = true;
					_lastPressMeowState[plyNumber] = MeowState.NotMeowed;
					_lastPressTime[plyNumber] = Time.time; // to check later
				}
				// button unpress
				else if (!plyPressingMeow && _lastPressState[plyNumber])
				{
					// short meow due to early release
					if (Time.time - _lastPressTime[plyNumber] <= LongMeowTime && _lastPressMeowState[plyNumber] == MeowState.NotMeowed)
					{
						DoMeow(self, true);
						_lastPressMeowState[plyNumber] = MeowState.MeowedShort;
					}

					_lastPressState[plyNumber] = false;
				}
			}
			catch (Exception e)
			{
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