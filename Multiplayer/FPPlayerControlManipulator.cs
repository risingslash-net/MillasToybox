using Rewired;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class FPPlayerControlManipulator : MonoBehaviour
{
    public Controller assignedController;
    public FPPlayer playerToTweak;
    public FPPlayerInput input;
    public Player rewiredPlayerInput;

    public float warpTime = 0f;
    public float warpTimeMax = 2f;

    public void Update()
    {
        if (playerToTweak.inputMethod.Method.Name.Equals("GetInputFromPlayer1"))
        {
            playerToTweak.inputMethod = ProcessRewiredWithControllerFilter;
        }
    }

    public void ProcessRewiredWithControllerFilter()
    {
        if (assignedController == null || playerToTweak == null)
        {
            return;
        }

        rewiredPlayerInput = FPPlayer.rewiredPlayerInput;
        
        // if (input == null)
        // {
        //     input = new FPPlayerInput();
        // }

        input.upPress = false;
        input.downPress = false;
        input.leftPress = false;
        input.rightPress = false;
        if (playerToTweak.IsPowerupActive(FPPowerup.MIRROR_LENS))
        {
            if (rewiredPlayerInput.GetButton("Left") 
                && rewiredPlayerInput.IsCurrentInputSource("Left", assignedController))
            {
                if (!input.left)
                {
                    input.leftPress = true;
                }
                input.left = true;
            }
            else
            {
                input.left = false;
            }
            if (rewiredPlayerInput.GetButton("Right") 
                && rewiredPlayerInput.IsCurrentInputSource("Right", assignedController))
            {
                if (!input.right)
                {
                    input.rightPress = true;
                }
                input.right = true;
            }
            else
            {
                input.right = false;
            }
        }
        else
        {
            if (rewiredPlayerInput.GetButton("Right") 
                && rewiredPlayerInput.IsCurrentInputSource("Right", assignedController))
            {
                if (!input.right)
                {
                    input.rightPress = true;
                }
                input.right = true;
            }
            else
            {
                input.right = false;
            }
            if (rewiredPlayerInput.GetButton("Left") 
                && rewiredPlayerInput.IsCurrentInputSource("Left", assignedController))
            {
                if (!input.left)
                {
                    input.leftPress = true;
                }
                input.left = true;
            }
            else
            {
                input.left = false;
            }
        }
        if (rewiredPlayerInput.GetButton("Up") 
            && rewiredPlayerInput.IsCurrentInputSource("Up", assignedController))
        {
            if (!input.up)
            {
                input.upPress = true;
            }
            input.up = true;
        }
        else
        {
            input.up = false;
        }
        if (rewiredPlayerInput.GetButton("Down") 
            && rewiredPlayerInput.IsCurrentInputSource("Down", assignedController))
        {
            if (!input.down)
            {
                input.downPress = true;
            }
            input.down = true;
        }
        else
        {
            input.down = false;
        }

        input.jumpPress = rewiredPlayerInput.GetButtonDown("Jump") &&
                          rewiredPlayerInput.IsCurrentInputSource("Jump", assignedController);
        input.jumpHold = rewiredPlayerInput.GetButton("Jump") &&
                         rewiredPlayerInput.IsCurrentInputSource("Jump", assignedController);
        input.attackPress = rewiredPlayerInput.GetButtonDown("Attack") &&
                            rewiredPlayerInput.IsCurrentInputSource("Attack", assignedController);
        input.attackHold = rewiredPlayerInput.GetButton("Attack") &&
                           rewiredPlayerInput.IsCurrentInputSource("Attack", assignedController);
        input.specialPress = rewiredPlayerInput.GetButtonDown("Special") &&
                             rewiredPlayerInput.IsCurrentInputSource("Special", assignedController);
        input.specialHold = rewiredPlayerInput.GetButton("Special") &&
                            rewiredPlayerInput.IsCurrentInputSource("Special", assignedController);
        input.guardPress = rewiredPlayerInput.GetButtonDown("Guard") &&
                           rewiredPlayerInput.IsCurrentInputSource("Guard", assignedController);
        input.guardHold = rewiredPlayerInput.GetButton("Guard") &&
                          rewiredPlayerInput.IsCurrentInputSource("Guard", assignedController);
        input.confirm = (input.jumpPress | InputControl.GetButtonDown(Controls.buttons.pause));
        input.cancel = (input.attackPress | Input.GetKey(KeyCode.Escape));

        HandlePlayerHoldGuardToCatchup();

        playerToTweak.input = input;
    }

    public void HandlePlayerHoldGuardToCatchup()
    {
        if (input.guardHold)
        {
            warpTime += Time.deltaTime;
            if (warpTime >= warpTimeMax)
            {
                warpTime = 0;
                
                var fpplayers = FindObjectsOfType<FPPlayer>();
                foreach (var fpp in fpplayers)
                {
                    if (fpp != playerToTweak)
                    {
                        playerToTweak.transform.position =
                            fpp.transform.position;
                        playerToTweak.position = fpp.position;
                        playerToTweak.velocity.x = fpp.velocity.x;
                        playerToTweak.velocity.y = fpp.velocity.y;
                        break;
                    }
                }

            }
        }
        else
        {
            warpTime = 0f;
        }
    }
}