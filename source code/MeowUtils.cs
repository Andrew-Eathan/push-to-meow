using BepInEx.Logging;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Timers;
using Random = UnityEngine.Random;
using System.Linq;
using System.ComponentModel;
using MoreSlugcats;
using IL.JollyCoop;
using IL.MoreSlugcats;

namespace PushToMeowMod
{
	public enum MeowState
	{
		NotMeowed,
		MeowedLong,
		MeowedShort
	}

	public struct CustomMeow
	{
		// named to match the json field names
		public string SlugcatID;
		public string FilePath;
		public float VolumeMultiplier;
		public SoundID ShortMeowSoundID;
		public SoundID LongMeowSoundID;
		public SoundID ShortMeowPupSoundID;
		public SoundID LongMeowPupSoundID;
	}

	public class CustomMeowJson
	{
		// named to match the json field names
		public string slugcat_id;
		public float volume_multiplier = 1f;
		public string short_meow_soundid;
		public string long_meow_soundid;
		public string short_meow_pup_soundid;
		public string long_meow_pup_soundid;
	}

	public struct CustomMeowWrapperJson
	{
		public int priority;
		public string _file;
		public CustomMeowJson[] custom_meows;
	}

	public static class MeowUtils
	{
		public static SoundID SlugcatMeowNormal { get; internal set; }
		public static SoundID SlugcatMeowNormalShort { get; internal set; }
		public static SoundID SlugcatMeowPup { get; internal set; }
		public static SoundID SlugcatMeowPupShort { get; internal set; }
		public static SoundID SlugcatMeowRivuletA { get; internal set; }
		public static SoundID SlugcatMeowRivuletAShort { get; internal set; }
		public static SoundID SlugcatMeowRivuletB { get; internal set; }
		public static SoundID SlugcatMeowRivuletBShort { get; internal set; }

		public static SoundID SlugcatMeowKatzenEasterEgg { get; internal set; }

		public static Dictionary<string, CustomMeow> CustomMeows = new Dictionary<string, CustomMeow>();

        internal static PushToMeowMain PushToMeowPlugin = null;


        public static void InitialiseSoundIDs(ManualLogSource logger)
		{
			SlugcatMeowRivuletA = new SoundID("SlugcatMeowRivuletA", true);
			SlugcatMeowRivuletB = new SoundID("SlugcatMeowRivuletB", true);
			SlugcatMeowNormal = new SoundID("SlugcatMeowNormal", true);
			SlugcatMeowPup = new SoundID("SlugcatMeowPup", true);

			SlugcatMeowRivuletAShort = new SoundID("SlugcatMeowRivuletAShort", true);
			SlugcatMeowRivuletBShort = new SoundID("SlugcatMeowRivuletBShort", true);
			SlugcatMeowNormalShort = new SoundID("SlugcatMeowNormalShort", true);
			SlugcatMeowPupShort = new SoundID("SlugcatMeowPupShort", true);
            SlugcatMeowKatzenEasterEgg = new SoundID("SlugcatMeowKatzenEasterEgg", true);

            logger.LogInfo("initialised default sound IDs!");
		}

		public static void HandleOracleReactions(Player self)
		{
			foreach (List<PhysicalObject> objList in self.room.physicalObjects) // why is that an array of lists? :sob:
			{
				foreach (PhysicalObject obj in objList)
				{
					if (obj is Oracle)
					{
						Oracle oracle = obj as Oracle;
						MeowOracleReactions.HandleThisOraclesReaction(self, oracle);
					}
				}
			}
		}

		// taken from https://github.com/MatheusVigaro/DressMySlugcat thank you DMS devs for this
		public static string[] ListDirectory(string path, bool directories = false, bool includeAll = false)
		{
			if (Path.IsPathRooted(path))
			{
				return (directories ? Directory.GetDirectories(path) : Directory.GetFiles(path));
			}

			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			list3.Add(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"));
			for (int i = 0; i < ModManager.InstalledMods.Count; i++)
			{
				list3.Add(ModManager.InstalledMods[i].path);
			}

			list3.Add(Custom.RootFolderDirectory());
			foreach (string item in list3)
			{
				string path2 = Path.Combine(item, path.ToLowerInvariant());
				if (!Directory.Exists(path2))
				{
					continue;
				}

				string[] array = (directories ? Directory.GetDirectories(path2) : Directory.GetFiles(path2));
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j].ToLowerInvariant();
					string fileName = Path.GetFileName(text);
					if (!list2.Contains(fileName) || includeAll)
					{
						list.Add(text);
						if (!includeAll)
						{
							list2.Add(fileName);
						}
					}
				}
			}

			return list.ToArray();
		}

		public static bool CanMeow(Player self)
		{
			bool isGrabbedByNonPlayer = self.grabbedBy.Count > 0 && !(self.grabbedBy[0].grabber is Player);

			// ignore grab check if panic meowing isnt allowed
			if (self.dead || (isGrabbedByNonPlayer && !PushToMeowMain.ModSettings.CanPanicMeow.Value))
			{
				PushToMeowMain.PLogger.LogInfo("dead so no meow >:( " + self + " " + self.Consious + " " + self.playerState.playerNumber);
				return false;
			}

			return true;
		}

		public static void DoMeowAnim(Player self, bool isShortMeow)
		{
			if (self.graphicsModule is PlayerGraphics gm)
			{
				// looks up after 33 ms, small delay
				Timer lookUpTimer =
					new Timer(33) { AutoReset = false, Enabled = true };
				lookUpTimer.Elapsed += (object _, ElapsedEventArgs e) =>
				{
					// spearmaster wont look up
					if (self.SlugCatClass != MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear)
						gm?.LookAtPoint(new Vector2(0, 100000), 69420);

					self.Blink(isShortMeow ? 9 : 11);
				};

				Timer resetLookTimer =
					new Timer(33 + (isShortMeow ? 160 : 260)) { AutoReset = false, Enabled = true };
				resetLookTimer.Elapsed += (object _, ElapsedEventArgs e) => gm.LookAtNothing();
			}
		}

		public static (SoundID meowSoundID, float pitch, float volume) FindMeowSoundID(Player self, bool isShortMeow)
		{
			var Logger = PushToMeowMain.PLogger;



			SoundID meowType;
			bool isPup = self.playerState.isPup;
			float pitch = isPup ? 1.3f : 1f;
			float volume = PushToMeowMain.ModSettings.MeowVolumeMultiplier.Value;
			string slugcatID = self.SlugCatClass.value;

			// if something adds a custom meow sound for rivulet, ignore the default one
			if (slugcatID == "Rivulet" && !CustomMeows.ContainsKey(slugcatID))
			{
				bool altRivuletSounds = PushToMeowMain.ModSettings.AltRivuletSounds.Value;

				var SlugcatMeowRivuletShort = altRivuletSounds ? SlugcatMeowRivuletBShort : SlugcatMeowRivuletAShort;
				var SlugcatMeowRivulet = altRivuletSounds ? SlugcatMeowRivuletB : SlugcatMeowRivuletA;
				meowType = isShortMeow ? SlugcatMeowRivuletShort : SlugcatMeowRivulet;

				volume *= 0.8f; // 0.8x volume for rivulet
			}
			else {
				// try to find a custom SoundID for this slugcat type
				if (CustomMeows.ContainsKey(slugcatID))
				{
					var meow = CustomMeows[slugcatID];
					volume *= meow.VolumeMultiplier;

					if (isPup)
					{
						// pick the pup variants of the meows if possible
						meowType = isShortMeow
							? (meow.ShortMeowPupSoundID ?? meow.ShortMeowSoundID)
							: (meow.LongMeowPupSoundID ?? meow.LongMeowSoundID);

						// set pitch to normal if we've found a custom pup sound, since itll likely be fittingly pitched up anyway
						if (meowType == meow.ShortMeowPupSoundID || meowType == meow.LongMeowPupSoundID)
							pitch = 1f;
					}
					else meowType = isShortMeow ? meow.ShortMeowSoundID : meow.LongMeowSoundID;
				}
				else
				{
					Logger.LogDebug("Using normal meow type for " + slugcatID + " because they don't have a custom meow registered");

					if (isPup) // pup meow
						meowType = isShortMeow ? SlugcatMeowPupShort : SlugcatMeowPup;
					else // normal meow
						meowType = isShortMeow ? SlugcatMeowNormalShort : SlugcatMeowNormal;
				}
			}

			// panic when chomped by something
			bool isGrabbedByNonPlayer = self.grabbedBy.Count > 0 && !(self.grabbedBy[0].grabber is Player);
			if (isGrabbedByNonPlayer)
				pitch += 0.2f + UnityEngine.Random.value * 0.15f;

			// slightly deeper meow when underwater, for now (will probably add custom sounds for this aswell)
			if (self.submerged)
				pitch -= 0.1f + Random.value * 0.15f;

			// Vultu: Katzen easter egg
            var name = JollyCoop.JollyCustom.GetPlayerName(self.playerState.playerNumber);
			if (name == "Katzen")
				meowType = SlugcatMeowKatzenEasterEgg;
            
			

            return (meowType, pitch, volume);
		}

		public static void DoSpearmasterTailWiggle(Player self)
		{
			if (!(self.graphicsModule is PlayerGraphics gm)) return;

			// fling tail to side opposite to the one spear is facing
			float horizFling = -self.flipDirection * Random.Range(2, 3);

			// flop tail up
			for (int i = 0; i < gm.tail.Length; i++)
				gm.tail[i].vel = new Vector2(horizFling, Random.Range(3, 6) / 2f * (i - 1) * 1.5f);

			// flop tail back down
			Timer down = new Timer(Random.Range(80, 140)) { AutoReset = false };
			down.Elapsed += (object a, ElapsedEventArgs b) =>
			{
				for (int i = 0; i < gm.tail.Length; i++)
					gm.tail[i].vel = new Vector2(horizFling, -Random.Range(4, 8) / 3f * (i - 1) * 1.5f);

				down.Dispose();
			};
			down.Start();

			PushToMeowMain.PLogger.LogInfo("spear ma balz");
		}

		public static void LoadCustomMeows()
		{
			CustomMeows.Clear();

			var Logger = PushToMeowMain.PLogger;
			Logger.LogInfo("Loading custom slugcat meows:");

			try
			{

				string[] pushToMeowDirs = ListDirectory("pushtomeow", false, true);
				List<CustomMeowWrapperJson> meowfiles = new List<CustomMeowWrapperJson>();

				foreach (string file in pushToMeowDirs)
				{
					string fileName = Path.GetFileName(file);
					if (fileName != "custom_meows.json") continue;

					Logger.LogInfo("Reading file " + file);
					CustomMeowWrapperJson meowfile = JsonConvert.DeserializeObject<CustomMeowWrapperJson>(File.ReadAllText(file));
					CustomMeowJson[] meows = meowfile.custom_meows;

					if (meows == null || meows.Length == 0)
					{
						Logger.LogError("Custom meow file " + file + " had no meows inside! Please add some :(");
						continue;
					}

					meowfile._file = file;
					meowfiles.Add(meowfile);
				}

				// sort files from lowest priority to highest,
				// this means higher priority files will overwrite existing slugcats from lower ones
				meowfiles = meowfiles.OrderBy(o => o.priority).ToList();

				foreach (CustomMeowWrapperJson meowfile in meowfiles)
				{
					Logger.LogInfo("Loading meows from file " + meowfile._file + " (priority: " + meowfile.priority + ")");
					CustomMeowJson[] meows = meowfile.custom_meows;

					foreach (CustomMeowJson slugcat in meows) 
					{
						if (CustomMeows.ContainsKey(slugcat.slugcat_id))
						{
							Logger.LogWarning("Overwriting " + slugcat.slugcat_id + ":");
							CustomMeows.Remove(slugcat.slugcat_id);
						}

						Logger.LogInfo("Registered custom meow for slugcat ID \"" + slugcat.slugcat_id + "\":");
						Logger.LogInfo("        short meow SoundID = " + (slugcat.short_meow_soundid ?? "(none)"));
						Logger.LogInfo("         long meow SoundID = " + (slugcat.long_meow_soundid ?? "(none)"));
						Logger.LogInfo("    short meow pup SoundID = " + (slugcat.short_meow_pup_soundid ?? "(none)"));
						Logger.LogInfo("     long meow pup SoundID = " + (slugcat.long_meow_pup_soundid ?? "(none)"));
						Logger.LogInfo("     volume mul: " + slugcat.volume_multiplier + "x");
						Logger.LogInfo("");

						CustomMeows.Add(slugcat.slugcat_id, new CustomMeow()
						{
							SlugcatID = slugcat.slugcat_id,
							FilePath = meowfile._file,
							VolumeMultiplier = slugcat.volume_multiplier,
							ShortMeowSoundID = slugcat.short_meow_soundid != null ? new SoundID(slugcat.short_meow_soundid, true) : null,
							LongMeowSoundID = slugcat.long_meow_soundid != null ? new SoundID(slugcat.long_meow_soundid, true) : null,
							ShortMeowPupSoundID = slugcat.short_meow_pup_soundid != null ? new SoundID(slugcat.short_meow_pup_soundid, true) : null,
							LongMeowPupSoundID = slugcat.long_meow_pup_soundid != null ? new SoundID(slugcat.long_meow_pup_soundid, true) : null
						});
					}
				}

				Logger.LogInfo("Successfully loaded " + CustomMeows.Count + " custom meow(s) :3");
			}
			catch (Exception e)
			{
				Logger.LogError("Couldn't load custom slugcat meows!");
				Logger.LogError(e);
				Debug.LogException(e);
			}
		}

		public static Dictionary<Player, float> SlugNPCLastMeow = new Dictionary<Player, float>();

		public static void ClearNPCMeowTime(Player self)
		{
            if (!self.isNPC || !SlugNPCLastMeow.ContainsKey(self))
                return;

			SlugNPCLastMeow[self] = 0;
        }

		public static void HandleNPCSlugcat(Player self, float meowTimer = 0)
		{
			if (!self.isNPC)
				return;

			if (!SlugNPCLastMeow.ContainsKey(self))
				SlugNPCLastMeow.Add(self, 0);
			
			if (Time.time - SlugNPCLastMeow[self] > 0.5)
			{
                PushToMeowPlugin.DoMeow(self, Random.value > 0.5f);
				SlugNPCLastMeow[self] = Time.time + meowTimer;
				// PushToMeowMain.PLogger.LogMessage($"Next meow will be: {Time.time + meowTimer}");
			}
		}


	}
}
