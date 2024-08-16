using BepInEx.Logging;
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
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unable to attach {nameof(SlugNPCMeowAI)} hooks!! Exception details are as follows: \"{ex.Message}\"");
            }

            return true;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!self.isNPC || self.dead)
                return;

            if (self.dangerGrasp != null)
            {
                // Vultu: HELP!!!

                var graspPercentage = (float)self.dangerGraspTime / 1000f;
                Logger.LogMessage($"PERC: {graspPercentage}");
                float targetTime = Mathf.Lerp(0.75f, 3f, graspPercentage * UnityEngine.Random.Range(0.1f, 1.0f));

                // Vultu: Reset time if it's greather than 3
                if (MeowUtils.SlugNPCLastMeow.ContainsKey(self))
                {
                    var meowTime = MeowUtils.SlugNPCLastMeow[self];

                    if ((meowTime - Time.time) > targetTime)
                        MeowUtils.SlugNPCLastMeow[self] = 0;
                }


                // Vultu: HELP!!!
                MeowUtils.HandleNPCSlugcat(self, targetTime));
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

            if (self.cat.dead)
                return;

            if (self.cat.slugcatStats.foodToHibernate > self.cat.CurrentFood)
            {
                // Vultu: I am hungry and I am going to scream.
                float hungerPercentage = (float)self.cat.CurrentFood / (float)self.cat.slugcatStats.foodToHibernate;
                MeowUtils.HandleNPCSlugcat(self.cat, Mathf.Lerp(35, 160, 1 - hungerPercentage) + UnityEngine.Random.value * 35);
            }
        }

    }
}
