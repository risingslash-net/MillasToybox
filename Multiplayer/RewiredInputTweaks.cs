using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;
using Rewired;

public class RewiredInputTweaks : MonoBehaviour
{
    public static List<Controller> AvailableControllers;
    public static void InitExtraRewiredPlayers()
    {
        var separator = "----------";
        var rPlayers = ReInput.players.Players;
        foreach (var rPlayer in rPlayers)
        {
            Debug.Log(rPlayer.ToString());
            foreach (var playerController in rPlayer.controllers.Controllers)
            {
                Debug.Log(playerController);
            }
            Debug.Log("---");
        }
        Debug.Log(separator);

        foreach (var rController in ReInput.controllers.Controllers)
        {
            Debug.Log(rController.ToString());
        }
        Debug.Log(separator);
        
        
        Debug.Log(separator);

        /*
         *gGEPEbbzglUGDaQUCpliUoduDJgB startingControllerMapInfo,
  ControllerMapLayoutManager.StartingSettings controllerMapLayoutManagerSettings,
  ControllerMapEnabler.csVYKUeMfMKoAASKAdmZgRmxxmSo controllerMapEnablerSettings)
         * 
         */
        /*
        while (rPlayers.Count < 4)
        {
            var nextCount = rPlayers.Count + 1;
            
            var playerArgs = new System.Object[]
            {
                false,
                nextCount,
                "Player" + nextCount,
                "Player" + nextCount,
                
            };
            var newRPlayer2 = Activator.CreateInstance(typeof(Rewired.Player), playerArgs);
            var newRPlayer = new Rewired.Player(false, nextCount, "Player" + nextCount, "", );
            rPlayers.Add();
        }
        */

        
    }

    public static Player BindAllControllersToPlayer1()
    {
        var result = ReInput.players.GetPlayer(0);

        foreach (var controller in ReInput.controllers.Controllers)
        {
            //if (!result.controllers.Controllers.Contains(controller))
            if (!ReInput.controllers.IsControllerAssignedToPlayer(controller.type, controller.id, result.id))
            {
                result.controllers.AddController(controller, false);
            }
        }

        return result;
    }

    public static FPPlayerControlManipulator AddControlManipulator(FPPlayer fpp)
    {
        var manipulator = fpp.gameObject.AddComponent<FPPlayerControlManipulator>();
        manipulator.assignedController = ReInput.controllers.GetLastActiveController(ControllerType.Joystick);
        manipulator.playerToTweak = fpp;
        fpp.inputMethod = manipulator.ProcessRewiredWithControllerFilter;
        return manipulator;
    }
}