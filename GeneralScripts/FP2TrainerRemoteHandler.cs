using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RisingSlash.FP2Mods.MillasToybox.GeneralScripts;

public class FP2TrainerRemoteHandler : MonoBehaviour
{
    public FP2TrainerRemoteHandler handler;
    public string commandString;
    public NamedPipeServerStream pipeServFromTrainer;
    public MillasToybox Trainer;
    private StreamReader reader;
    
    public NamedPipeClientStream pipeToRemote;
    private StreamWriter writer;

    public bool sentConnectionMessage = false;
    
    public IAsyncResult asyncResult;
    public void Start()
    {
        if (handler != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            handler = this;
        }

        if (MillasToybox.EnablePhantomRemotePipes.Value)
        {
            pipeServFromTrainer = new NamedPipeServerStream("RSNDreadbox");
            asyncResult = pipeServFromTrainer.BeginWaitForConnection(meh, null);
            pipeToRemote = new NamedPipeClientStream("RSNPhantomCube");

            if (MillasToybox.LaunchPhantomRemoteOnStart.Value)
            {
                LaunchPhantomCubeRemote();
            }
        }
    }

    public static void LaunchPhantomCubeRemote()
    {
        Thread startserver = new Thread(new ThreadStart(LaunchPhantomCubeRemoteThread));
        startserver.Start();
    }
    
    public static void LaunchPhantomCubeRemoteThread()
    {
        var path = Path.Combine(BepInEx.Paths.PluginPath, "MillasToybox");
        path = Path.Combine(path, "PhantomCube.exe");
        Debug.Log("Launching: " + path);
        System.Diagnostics.Process.Start(path);
    }

    public void Update()
    {
        PollPipedMessagesFromRemote();
        //SendTextThroughPipe();
        //reader.Close();
    }

    public void PollPipedMessagesFromRemote()
    {
        if (pipeServFromTrainer is not { IsConnected: true })
        {
            return;
        }
        else
        {
            if (!sentConnectionMessage)
            {
                Debug.Log("Phantom: Connection Established.");
                sentConnectionMessage = true;
            }
        }

        if (reader == null)
        {
            reader = new StreamReader(pipeServFromTrainer);   
        }
        var line = reader.ReadLine();
        ParseCommand(line);
    }
    
    
    public void SendTextThroughPipe(string text)
    {
        Debug.Log("Attempting to send: " + text);
        if (pipeToRemote == null) 
        {
            pipeToRemote = new NamedPipeClientStream("RSNPhantomCube");
        }
        if (!pipeToRemote.IsConnected) 
        {
            var failed = false;
            try { pipeToRemote.Connect(100); }
            catch { failed = true; }
            if (failed) { return; }
        }
        if (writer == null) 
        {
            writer = new StreamWriter(pipeToRemote);
        }
        writer.WriteLine(text);
        writer.Flush();
    }

    public void ParseCommand(string txt)
    {
        if (String.IsNullOrEmpty(txt))
        {
            return;
        }
        
        if (txt.StartsWith("DREADBOX"))
        {
            Debug.Log("Transmission received: Experiment successful.");
            Trainer.InstaKOPlayer();
        }

        else
        {
            Debug.Log("Phantom Command: " + txt);
            var parts = txt.Replace(" ", "").Split(',');
            var cmd = parts[0].ToLower();
            if (cmd.Equals("spawn_velocity"))
            {
                Trainer.spawnVelocity = new Vector2(float.Parse(parts[1]), float.Parse(parts[2]));
            }
            else if (cmd.Equals("character_swap"))
            {
                FPPlayer2p.PerformInstaSwap((FPCharacterID)int.Parse(parts[1]));
            }
            else if (cmd.Equals("get_character_ids"))
            {
                GetCharacterIDs();
            }
            else if (cmd.Equals("get_scene_list"))
            {
                GetSceneList();
            }
            else if (cmd.Equals("get_shield_ids"))
            {
                GetShieldIDs();
            }
            else if (cmd.Equals("set_speed_boost"))
            {
                SetSpeedBoost(parts);
            }
            
        }
    }

    public void GetCharacterIDs()
    {
        var characterIDsAsText = "set_character_ids,";
        
        var FPCharacterIDNames = Enum.GetNames(typeof(FPCharacterID));
        for (int i = 0; i < FPCharacterIDNames.Length; i++)
        {
            if (i > 0)
            {
                characterIDsAsText += ",";
            }

            characterIDsAsText += (FPCharacterIDNames[i]);
        }
        SendTextThroughPipe(characterIDsAsText);
    }

    public void GetSceneList()
    {
        var sceneListAsText = "set_scene_list,";
        
        var sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            if (i > 0)
            {
                sceneListAsText += ",";
            }

            sceneListAsText += (SceneManager.GetSceneAt(i).name);
        }
        SendTextThroughPipe(sceneListAsText);
    }
    
    public void GetShieldIDs()
    {
        var characterIDsAsText = "set_shield_ids,Wood,Earth,Water,Fire,Metal";
        SendTextThroughPipe(characterIDsAsText);
    }

    public void SetSpeedBoost(string[] parts)
    {
        MillasToybox.SpeedBoostValue.Value = float.Parse(parts[1]);
    }

    public void OnDestroy()
    {
        Cleanup();
    }

    public void OnApplicationQuit()
    {
        Cleanup();
    }

    public void meh(IAsyncResult res)
    {
        return;
        try
        {
            if (pipeServFromTrainer != null)
            {
                Debug.Log("aa");
                pipeServFromTrainer.Close();
                Debug.Log("aaa");
                //pipeServFromTrainer.Disconnect();
                if (asyncResult != null)
                {
                    Debug.Log("aaaa");
                    pipeServFromTrainer.EndWaitForConnection(res);
                    Debug.Log("WHY");
                }
            }
        }
        catch
        {
        }
    }

    public void Cleanup()
    {
        Debug.Log("a");
        try
        {
            if (pipeServFromTrainer != null)
            {
                Debug.Log("aa");
                pipeServFromTrainer.Close();
                Debug.Log("aaa");
                //pipeServFromTrainer.Disconnect();
                if (asyncResult != null)
                {
                    Debug.Log("aaaa");
                    //pipeServFromTrainer.EndWaitForConnection(asyncResult);
                    Debug.Log("WHY");
                }
            }
        }
        catch
        {
        }

        

        Debug.Log("b");
        if (reader != null)
        {
            reader.Close();
            reader.Dispose();
        }
        
        Debug.Log("c");
        if (pipeToRemote != null)
        {
            pipeToRemote.Close();
        }
        
        Debug.Log("d");
        if (writer != null)
        {
            writer.Close();
            writer.Dispose();
        }
        Debug.Log("e");
            //throw new NotImplementedException();
            
        Application.Quit();
    }
}