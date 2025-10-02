using System;
using MoreSlugcats;
using UnityEngine;

namespace PushToMeowMod
{
	public partial class PushToMeowMain
	{
		public void DoMeow(Player self, bool isShortMeow = false)
		{
			// Handle Spearmaster special case (mute character)
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

			// Apply Rotund World pitch adjustment if enabled
			if (RotundWorldSupportEnabled)
			{
				pitch = ApplyRotundWorldPitchAdjustment(self, pitch);
			}

			// Play meow sound (unless it's an NPC and alert meow is disabled)
			if (!self.isNPC || (self.isNPC && ModSettings.SlugpupAlertMeow.Value))
			{
				PlayMeowSound(self, meowType, pitch, volume);
				AlertCreaturesIfEnabled(self);
			}

			// Drain lungs if enabled
			DrainLungsIfEnabled(self, isShortMeow);

			// Create bubbles if submerged
			CreateBubblesIfSubmerged(self, isShortMeow);
		}

		private float ApplyRotundWorldPitchAdjustment(Player self, float pitch)
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
			return pitch - pitchChange;
		}

		private void PlayMeowSound(Player self, SoundID meowType, float pitch, float volume)
		{
			try
			{
				self.room.PlaySound(meowType, self.bodyChunks[0], false, volume, pitch);
			}
			catch (Exception e)
			{
				Logger.LogError("Couldn't play meow type " + meowType + " for slugcat ID " +
				                self.SlugCatClass.value +
				                ", possible SoundID issues? Check your custom meow sound mod's modify/soundeffects/sounds.txt, make sure your .wav files have no underscores or other symbols!");
				Logger.LogError(e);
			}
		}

		private void AlertCreaturesIfEnabled(Player self)
		{
			if (ModSettings.AlertCreatures.Value)
				self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10000f, self, 2f));
		}

		private void DrainLungsIfEnabled(Player self, bool isShortMeow)
		{
			if (ModSettings.DrainLungs.Value)
				if (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
					self.airInLungs -= 0.08f + (isShortMeow ? 0 : 0.08f);
		}

		private void CreateBubblesIfSubmerged(Player self, bool isShortMeow)
		{
			if (!self.submerged)
				return;

			for (int i = 0; i < 2 + (isShortMeow ? 0 : 1); i++)
			{
				// for spearmaster, bubbles should come from tail
				if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					if (self.graphicsModule is PlayerGraphics gm)
						self.room.AddObject(new Bubble(gm.tail[2].pos, gm.tail[2].vel, false, false));
				}
				else
				{
					self.room.AddObject(new Bubble(self.firstChunk.pos, self.firstChunk.vel, false, false));
				}
			}
		}
	}
}

