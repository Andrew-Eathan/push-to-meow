using System;
using UnityEngine;

namespace PushToMeowMod
{
	public partial class PushToMeowMain
	{
		public void Update()
		{
			foreach (var kv in PlayersMeowButtonLastState)
			{
				int playerIdx = kv.Key;
				bool meowButtonState = kv.Value;
				float timeSinceMeowPress = Time.time - PlayersLastMeowTime[playerIdx];

				// if meow button is still pressed after time is over, then do a long meow 
				if (
					meowButtonState
					&& timeSinceMeowPress > LongMeowTime
					&& PlayersMeowingState[playerIdx] == MeowState.NotMeowed
				)
				{
					DoMeow(PlayersLookup[playerIdx]);
					PlayersMeowingState[playerIdx] = MeowState.MeowedLong;
#if DEBUG
					Logger.LogInfo("meow long " + playerIdx + " " + PlayersLookup[playerIdx] + " " +
					               timeSinceMeowPress);
#endif
				}
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

				// Initialize dictionaries for this player if needed
				if (!PlayersMeowButtonLastState.ContainsKey(plyNumber))
					PlayersMeowButtonLastState.Add(plyNumber, false);
				if (!PlayersLastMeowTime.ContainsKey(plyNumber))
					PlayersLastMeowTime.Add(plyNumber, 0);
				if (!PlayersMeowingState.ContainsKey(plyNumber))
					PlayersMeowingState.Add(plyNumber, MeowState.NotMeowed);
				if (!PlayersLookup.ContainsKey(plyNumber))
					PlayersLookup.Add(plyNumber, self);

				PlayersLookup[plyNumber] = self; // always assign the player to avoid bugs after respawning

				// button press
				if (plyPressingMeow && !PlayersMeowButtonLastState[plyNumber])
				{
					if (Time.time - PlayersLastMeowTime[plyNumber] < MeowCooldown) 
						return;

					PlayersMeowButtonLastState[plyNumber] = true;
					PlayersMeowingState[plyNumber] = MeowState.NotMeowed;
					PlayersLastMeowTime[plyNumber] = Time.time; // to check later
				}
				// button unpress
				else if (!plyPressingMeow && PlayersMeowButtonLastState[plyNumber])
				{
					// short meow due to early release
					if (Time.time - PlayersLastMeowTime[plyNumber] <= LongMeowTime &&
					    PlayersMeowingState[plyNumber] == MeowState.NotMeowed)
					{
						DoMeow(self, true);
						PlayersMeowingState[plyNumber] = MeowState.MeowedShort;
#if DEBUG
						Logger.LogDebug("did short meow");
#endif
					}

					PlayersMeowButtonLastState[plyNumber] = false;
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

