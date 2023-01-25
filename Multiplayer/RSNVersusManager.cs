using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.MillasToybox;

public class RSNVersusManager : MonoBehaviour
{
    public List<Rewired.Controller> controllers;
    public List<Rewired.Controller> ActiveControllers;
    public List<System.Object[]> PlayerSpawnList;

    public Player rewiredPlayerInput;
    public TextMesh tm;

    public List<string> Teams;
    private bool fightStarted = false;
    public static TextMesh cloneThisTM;
    
    public static void Init()
    {
        if (SceneManager.GetActiveScene().name.Contains("Menu") 
            || SceneManager.GetActiveScene().name.Contains("Cutscene"))
        {
            return;
        }

        if (MillasToybox.cloneMeForText != null)
        {
            cloneThisTM = MillasToybox.cloneMeForText;
            cloneThisTM.transform.position = new Vector3(160, -32, 0f);
        }
        else
        {
            cloneThisTM = GameObject.Find("RSNTextMesh").GetComponent<TextMesh>();
        }

       
        var newTextMesh = Instantiate(cloneThisTM);
        SceneManager.MoveGameObjectToScene(newTextMesh.gameObject, SceneManager.GetActiveScene());
        var tmt = newTextMesh.GetComponent<TextMesh>();
        tmt.name = "tmVersus";
        //tmt.transform.position = tmt.transform.position + new Vector3(128, 64);
        tmt.transform.position = new Vector3(160, -32, 0f);
        tmt.text = "Versus Manager Ready";
        tmt.gameObject.SetActive(true);
        

        var newMan = tmt.gameObject.AddComponent<RSNVersusManager>();
        newMan.controllers = new List<Controller>();
        foreach (var con in ReInput.players.Players[0].controllers.Joysticks)
        {
            newMan.controllers.Add(con);
        }

        newMan.rewiredPlayerInput = ReInput.players.GetPlayer(0);
        newMan.tm = tmt;
        
        Debug.Log($"There appears to be {newMan.controllers.Count} controllers available.");

        newMan.ActiveControllers = new List<Rewired.Controller>();
        newMan.PlayerSpawnList = new List<object[]>();
        newMan.Teams = new List<string>()
        {
            "Shang Mu",
            "Shang Tu",
            "Shuigang",
            "Parusa",
            "Brevon",
            "Merga",
        };

        //DummyOutPlayerBosses();

    }

    public void PollForNewControllers()
    {
        var blep = ReInput.players.Players[0].controllers.Joysticks;
        bool join = false;
        foreach (var controller in blep)
        {
            //Debug.Log("BeforeActiveControllers");
            //Debug.Log("Starting: " + controller.name);
            if (ActiveControllers == null)
            {
                ActiveControllers = new List<Controller>();
            }

            //Debug.Log("Containment");
            if (ActiveControllers.Contains(controller) || controller == null)
            {
                continue;
            }

            //Debug.Log("PreJump");
            if (rewiredPlayerInput == null)
            {
                //Debug.Log("Null player???");
                continue;
            }

            if (rewiredPlayerInput.GetButtonDown("Jump")
                && rewiredPlayerInput.IsCurrentInputSource("Jump", controller))
            {
                //Debug.Log("New Controller: " + controller.name);
                join = true;
            }
            
            //Debug.Log("PreAttack");
            if (rewiredPlayerInput.GetButtonDown("Attack")
                && rewiredPlayerInput.IsCurrentInputSource("Attack", controller))
            {
                join = true;
            }

            //Debug.Log("PreJoin");
            if (join)
            {
                //Debug.Log("JoinStart");
                var newTM = Instantiate(cloneThisTM);
                newTM.gameObject.name = controller.name;
                PlayerSpawnList.Add(new []
                {
                    (object)controller,
                    (object)0,
                    (object)newTM,
                    (object)0,
                    (object)false,
                });
                newTM.text = $"Here comes a new challenger: {controller.name}";
                newTM.transform.position = new Vector3(128 - 32 , -128 + (-16 * (PlayerSpawnList.Count + 1)));
                newTM.gameObject.SetActive(true);
                SceneManager.MoveGameObjectToScene(newTM.gameObject, SceneManager.GetActiveScene());
                //Debug.Log(tm.text);
                
                //Debug.Log("New Controller: " + controller.name);
                ActiveControllers.Add(controller);
            }
        }
    }

    public static void DummyOutPlayerBosses()
    {
        var playerBosses = Component.FindObjectsOfType<PlayerBoss>();
        foreach (var pb in playerBosses)
        {
            pb.state = DummyState;
        }
    }

    public static void DummyState()
    {
        
    }

    public void HandleControlsPlayerSetup()
    {
        bool allReady = true;
        tm.text = "";
        foreach (var entry in PlayerSpawnList)
        {
            if (entry == null || entry[0] == null)
            {
                continue;
            }

            bool inputLeft = false;
            bool inputRight = false;
            bool inputUp = false;
            bool inputDown = false;
            bool inputReadyUp = false;
            // [controller, characterID, tmPlayerSetup, teamIndex, ready];
            //Debug.Log("UpdatingControllerState");
            var controller = (Rewired.Controller)entry[0];
            var charID = (int)entry[1];
            var playerTM = (TextMesh)entry[2];
            var teamID = (int)entry[3];
            var ready = (bool)entry[4];

            if (ready)
            {
                continue;
            }

            if (rewiredPlayerInput.GetButtonDown("Left")
                && rewiredPlayerInput.IsCurrentInputSource("Left", controller))
            {
                inputLeft = true;
            }
            
            if (rewiredPlayerInput.GetButtonDown("Right")
                && rewiredPlayerInput.IsCurrentInputSource("Right", controller))
            {
                inputRight = true;
            }
            if (rewiredPlayerInput.GetButtonDown("Up")
                && rewiredPlayerInput.IsCurrentInputSource("Up", controller))
            {
                inputUp = true;
            }
            
            if (rewiredPlayerInput.GetButtonDown("Down")
                && rewiredPlayerInput.IsCurrentInputSource("Down", controller))
            {
                inputDown = true;
            }
            
            if (rewiredPlayerInput.GetButtonDown("Attack")
                && rewiredPlayerInput.IsCurrentInputSource("Attack", controller))
            {
                inputReadyUp = true;
            }
            if (rewiredPlayerInput.GetButtonDown("Special")
                && rewiredPlayerInput.IsCurrentInputSource("Special", controller))
            {
                inputReadyUp = true;
            }

            if (inputRight)
            {
                charID++;
                if (charID >= Enum.GetValues(typeof(FPCharacterID)).Length)
                {
                    charID = 0;
                }
            }
            if (inputLeft)
            {
                charID--;
                if (charID < 0)
                {
                    charID = Enum.GetValues(typeof(FPCharacterID)).Length - 1;
                }
            }
            
            if (inputUp)
            {
                teamID++;
                if (teamID >= Teams.Count)
                {
                    teamID = 0;
                }
            }
            if (inputDown)
            {
                teamID--;
                if (teamID < 0)
                {
                    teamID = Teams.Count - 1;
                }
            }

            if (inputReadyUp)
            {
                ready = true;
            }

            entry[1] = charID;
            entry[3] = teamID;
            entry[4] = ready;

            if (!ready)
            {
                allReady = false;
            }

            playerTM.text = $"Character: {Enum.GetName(typeof(FPCharacterID), charID)}, Team: {Teams[teamID]}, Ready: {ready}";
        }

        tm.text += $"\r\nPlayers: ({PlayerSpawnList.Count} / 2)\r\nAll Ready?:{allReady}\r\nL/R: Character, U/D: Team, \r\nJump: Join, Attack: Confirm.\r\n";
        
        if (PlayerSpawnList.Count <= 1)
        {
            allReady = false;
            tm.text += " - Not Enough Players";
        }

        if (allReady && !fightStarted)
        {
            tm.text = "READY.";
            SpawnCharacters();
            fightStarted = true;
        }
    }

    public void Update()
    {
        PollForNewControllers();
        HandleControlsPlayerSetup();
    }
    
    public void SpawnCharacters()
    {
        var combatants = new List<FPPlayer>();
        var bossHUD = Component.FindObjectOfType<FPBossHud>();
        Vector3 baseOffset = new Vector3(-64, 32, 0);
        var index = -1;
        foreach (var spawnDetails in PlayerSpawnList)
        {
            index++;
            // [controller, characterID, tmPlayerSetup, teamIndex, ready];
            var tmPlayerSetup = (TextMesh)spawnDetails[2];
            var newPlayer = FPPlayer2p.SpawnExtraCharacterByID((FPCharacterID)spawnDetails[1]);
            var manip = RewiredInputTweaks.AddControlManipulator(newPlayer);
            manip.assignedController = (Rewired.Controller)spawnDetails[0];
            newPlayer.faction = Teams[(int)spawnDetails[3]];
            Destroy(tmPlayerSetup.gameObject);

            if (FPStage.currentStage != null)
            {
                FPStage.currentStage.SetPlayerInstance_FPPlayer(newPlayer);
            }

            if (bossHUD != null)
            {
                var newHUD = Instantiate(bossHUD);
                newHUD.transform.position += baseOffset + (new Vector3(32f * index, 0f, 0f));
                newHUD.name = newPlayer.name + " - HUD";
                var enemyRef = newPlayer.gameObject.AddComponent<FPBaseEnemy>();
                enemyRef.enabled = false;
                BindEnemyBaseToPlayerStats.Bind(newPlayer, enemyRef, true);
                newHUD.targetBoss = enemyRef;
            }
            
            combatants.Add(newPlayer);
        }

        /* Destroy all prior player characters?*/
        foreach (var fppOld in MillasToybox.fpplayers)
        {
            if (!combatants.Contains(fppOld))
            {
                Destroy(fppOld);
            }
        }

        if (bossHUD != null)
        {
            bossHUD.gameObject.SetActive(false);
            bossHUD.enabled = false;
        }

        Destroy(tm.gameObject);
        
        MillasToybox.fpplayers = MillasToybox.GetFPPlayers();

        FP2VersusPlayerTarget.Spawn();
    }

    public void AssignControllersToCharacters()
    {
        var fpplayers = MillasToybox.fpplayers;
        for (int i = 0; i < fpplayers.Count; i++)
        {
            int iClamp = Mathf.Clamp(i, 0, controllers.Count);
            Debug.Log("Binding" + fpplayers[i].gameObject.name + " to Controller " + iClamp + " - " + controllers[iClamp]);
            var manipulator = RewiredInputTweaks.AddControlManipulator(fpplayers[i]);
            manipulator.assignedController = controllers[iClamp];
        }
    }
}