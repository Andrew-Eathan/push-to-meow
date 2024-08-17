using BepInEx.Logging;
using IL.Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PushToMeowMod.Vanilla_Hooks
{
    public static class SlugNPCMeowAI
    {
        internal static RainWorld RainWorld = null;
        internal static ManualLogSource Logger => PushToMeowMain.PLogger;


        //internal static Dictionary<Player, >
        internal static bool AttachHooks()
        {
            try
            {
                On.MoreSlugcats.SlugNPCAI.Update += SlugNPCAI_Update;
                On.Player.Update += Player_Update;
                On.Player.Stun += Player_Stun;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unable to attach {nameof(SlugNPCMeowAI)} hooks!! Exception details are as follows: \"{ex.Message}\"");
            }

            return true;
        }

        private static void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
        {
            orig(self, st);

            if (!self.isNPC || self.dead)
                return;

            if (self.stunDamageType.value == "Blunt")
            {
                MeowUtils.ClearNPCMeowTime(self);
                MeowUtils.HandleNPCSlugcat(self);
            }
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!self.isNPC || self.dead)
                return;

            if (PushToMeowMain.ModSettings.SlugpupPanicMeow.Value && self.dangerGrasp != null)
            {
                if (self.dangerGraspTime == 1)
                    MeowUtils.ClearNPCMeowTime(self);

                // Vultu: HELP!!!

                var graspPercentage = (float)self.dangerGraspTime / 1000f;
                float targetTime = Mathf.Lerp(0.75f, 3f, Math.Min(graspPercentage * UnityEngine.Random.Range(0.1f, 1.0f), 1.0f));


                MeowUtils.HandleNPCSlugcat(self, targetTime);
                return;
            }
        }


        internal static bool FreeHooks()
        {
            try
            {
                On.MoreSlugcats.SlugNPCAI.Update -= SlugNPCAI_Update;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unable to free {nameof(SlugNPCMeowAI)} hooks!! Exception details are as follows: \"{ex.Message}\"");
            }

            return true;
        }




        private static void SlugNPCAI_Update(On.MoreSlugcats.SlugNPCAI.orig_Update orig, MoreSlugcats.SlugNPCAI self)
        {
            orig(self);

            if (self.cat.dead || self.nap)
                return;

            if (PushToMeowMain.ModSettings.SlugpupHungryMeow.Value)
            {
                if (self.cat.slugcatStats.foodToHibernate > self.cat.CurrentFood)
                {
                    // Vultu: I am hungry and I am going to scream.
                    float hungerPercentage = (float)self.cat.CurrentFood / (float)self.cat.slugcatStats.foodToHibernate;
                    MeowUtils.HandleNPCSlugcat(self.cat, Mathf.Lerp(35, 160, 1 - hungerPercentage) + UnityEngine.Random.value * 35);
                }
            }
        }

    }
}
