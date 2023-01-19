using System;
using HarmonyLib;

namespace RisingSlash.FP2Mods.MillasToybox.Patches;

[HarmonyPatch(typeof(FPStage))]
[HarmonyPatch("UpdateObjectActivation")]
public class FP2TrainerPatchActivationState
{
    public static bool Prefix()
    {
        try
        {
            // The code inside this method will run before 'UpdateObjectActivation' is executed
            //MillasToybox.millasToyboxInstance.UpdateObjectActivationForAllPlayers(MillasToybox.fpplayers);
            MillasToybox.millasToyboxInstance.UpdateObjectActivationForFPStageCameras();
        }
        catch (Exception e)
        {
            MillasToybox.LogError($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
        }

        // Returning false causes the original method this is patching to be skipped.
        return false;
    }
}