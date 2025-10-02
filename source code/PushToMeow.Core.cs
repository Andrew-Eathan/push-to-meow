using BepInEx;
using BepInEx.Logging;
using ImprovedInput;
using PushToMeowMod.Localization;
using System.Collections.Generic;
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
		#region Constants
		public const string ROTUND_WORLD_IDENTIFER = "willowwisp.bellyplus";
		public const string PLUGIN_GUID = "pushtomeow";
		public const string PLUGIN_NAME = "Push to Meow";
		public const string PLUGIN_VERSION = "1.2.1";
		
		public const float LongMeowTime = 0.14f; // seconds needed to hold so that long meow plays
		public const float MeowCooldown = 0.24f; // seconds between meows
		#endregion

		#region Static Properties
		public static ManualLogSource PLogger { get; internal set; } = null;
		public static RainWorld RainWorld { get; internal set; } = null;
		public static PlayerKeybind Meow;
		public static MeowMeowOptions ModSettings;
		public static bool RotundWorldSupportEnabled { get; private set; } = false;
		#endregion

		#region Instance State
		// this stores the last checked state of the meow button for a given player
		public Dictionary<int, bool> PlayersMeowButtonLastState = new Dictionary<int, bool>();

		// this stores the state of the player's meow being played, this gets reset once the button is let go
		public Dictionary<int, MeowState> PlayersMeowingState = new Dictionary<int, MeowState>();

		// this stores the last time the meow button was pressed per player, used for cooldowns
		public Dictionary<int, float> PlayersLastMeowTime = new Dictionary<int, float>();

		// stores a quick lookup table for players used to access the player entity to handle meow button release
		public Dictionary<int, Player> PlayersLookup = new Dictionary<int, Player>();
		#endregion

		#region Initialization
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
				Meow = PlayerKeybind.Register("pushtomeow:meow", "Push to Meow", "Meow", KeyCode.M,
					KeyCode.JoystickButton3);

			Logger.LogInfo("READY TO RRRRUMBL- i mean Meow Meow Meow Meow :3333"); // :3 // cornyahh
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

			LocalizationSetup.InitializeTranslations();

			CheckForRotundWorldSupport();
		}

		private void CheckForRotundWorldSupport()
		{
			for (int i = 0; i < ModManager.ActiveMods.Count; i++)
			{
				if (ModManager.ActiveMods[i].id == ROTUND_WORLD_IDENTIFER)
				{
					RotundWorldSupportEnabled = true;
					Logger.LogInfo("found rotund world, enabling support in PtM :)");
					break;
				}
			}
		}
		#endregion

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
	}
}

