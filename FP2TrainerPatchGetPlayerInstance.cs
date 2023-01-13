using System;
using HarmonyLib;

namespace RisingSlash.FP2Mods.MillasToybox
{
    [HarmonyPatch(typeof(FPStage))]
    [HarmonyPatch("GetPlayerInstance")]
    public class FP2TrainerPatchGetPlayerInstance
    {
        public static void Postfix(ref FPBaseObject __result)
        {
            // The code inside this method will run after 'PrivateMethod' has executed
            try
            {
                // The code inside this method will run before 'PrivateMethod' is executed
                if (MillasToybox.EnableGetPlayerInstanceMultiplayerPatch.Value)
                {
                    if (MillasToybox.fpplayers != null)
                    {
                        int count = MillasToybox.fpplayers.Count;
                        if (count > 0)
                        {
                            __result = (FPBaseObject)(MillasToybox.fpplayers[UnityEngine.Random.Range(0, count - 1)]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MillasToybox.LogError($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}