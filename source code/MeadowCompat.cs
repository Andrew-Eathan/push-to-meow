using System;
using System.Collections.Generic;
using System.Linq;
using PushToMeowMod;
using RainMeadow;

namespace RainMeadowCompat
{
    /// <summary>
    /// NOTE: Referencing ANY methods within this class WILL throw an error if Meadow is not currently enabled,
    /// although referencing fields is alright.
    /// Be sure to EITHER check MeadowCompat.MeadowEnabled before making calls
    /// OR wrap all MeadowCompat method calls in a simple try { MeadowCompat.method() } catch { }
    /// </summary>
    public static class MeadowCompat
    {
        public const string MEADOW_BEPINEX_ID = "henpemaz.rainmeadow";
        public const string MEADOW_MOD_ID = "henpemaz_rainmeadow";

        public static bool MeadowEnabled = false;

        private static bool isInit = false;
        public static void InitCompatSetup()
        {
            MeadowEnabled = ModManager.ActiveMods.Any(mod => mod.id == MEADOW_MOD_ID);
            if (!MeadowEnabled) return;

            if (isInit) return;

            //register RPCs
            ModdedDataRPC.RegisterModRPC(PushToMeowMain.PLUGIN_GUID, MEOW_RPC_NAME, MeowRPC);

            isInit = true; //prevents RPCs from being double-registered
        }

        public const string MEOW_RPC_NAME = "MeowRPC";
        //public static void MeowRPC(RPCEvent _, OnlinePhysicalObject opo, bool isShortMeow)
        public static void MeowRPC(RPCEvent _, ModdedDataRPC.ArgData args)
        {
            if (!(args is MeowData data))
                return; //invalid arguments!

            if (data.player.apo.realizedObject is Player player)
            {
                PushToMeowMain.DoMeow(player, data.isShortMeow);
            }
        }

        internal class MeowData : ModdedDataRPC.ArgData
        {
            public OnlinePhysicalObject player;
            public bool isShortMeow;

            public MeowData() { }
            public MeowData(OnlinePhysicalObject player, bool isShortMeow)
            {
                this.player = player;
                this.isShortMeow = isShortMeow;
            }

            public override void CustomSerialize(Serializer serializer)
            {
                serializer.SerializeEntityById(ref player);
                serializer.Serialize(ref isShortMeow);
            }
        }

        public static void InvokeMeowRPC(Player player, bool isShortMeow)
        {
            if (OnlineManager.lobby == null) return;
            foreach (var recipient in OnlineManager.players) //send to EVERY player in the lobby
            {
                if (!recipient.isMe) //don't send to myself
                    ModdedDataRPC.InvokeModRPC(recipient, false, PushToMeowMain.PLUGIN_GUID, MEOW_RPC_NAME, new MeowData(player.abstractPhysicalObject.GetOnlineObject(), isShortMeow));
            }
        }
    }
}