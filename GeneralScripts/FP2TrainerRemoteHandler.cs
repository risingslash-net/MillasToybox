using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RisingSlash.FP2Mods.MillasToybox.GeneralScripts;

public class FP2TrainerRemoteHandler : MonoBehaviour
{
    public FP2TrainerRemoteHandler handler;
    public string commandString;
    public NamedPipeClientStream pipePhantomFromMilla;
    public MillasToybox Trainer;
    private StreamWriter writer;
    
    public NamedPipeClientStream pipePhantomToMilla;
    private StreamReader reader;

    public float pollTimer = 1f;
    public float pollTimerStart = 1f;

    public bool sentConnectionMessage = false;

    public bool AllowDebugModeMessages = true;
    public bool shutdown = false;

    public List<string> messagesReceived;

    public Thread ReaderThread;
    public static Process PhantomCubeProcess;
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
            if (MillasToybox.LaunchPhantomRemoteOnStart.Value)
            {
                LaunchPhantomCubeRemote();
            }

            messagesReceived = new List<string>();
            
            StartStreamReaderThread();
            ConstructPipes();
            ConnectPipes();
        }
    }

    public void StartStreamReaderThread()
    {
        DebugModeLog("Starting StreamReader thread.");
        ReaderThread = new Thread(new ThreadStart(ThreadPollPipedMessagesFromRemote));
        ReaderThread.IsBackground = true;
        ReaderThread.Start();
        DebugModeLog("Started.");
    }

    public static void LaunchPhantomCubeRemote()
    {
        //Thread startserver = new Process(new ProcessStartInfo(LaunchPhantomCubeRemoteThread));
        //startserver.Start();
        LaunchPhantomCubeRemoteProcess();
    }
    
    public static void LaunchPhantomCubeRemoteProcess()
    {
        var path = Path.Combine(BepInEx.Paths.PluginPath, "MillasToybox");
        path = Path.Combine(path, "PhantomCube.exe");
        Debug.Log("Launching: " + path);
        if (PhantomCubeProcess != null && !PhantomCubeProcess.HasExited)
        {
            PhantomCubeProcess.CloseMainWindow();
        }

        PhantomCubeProcess = new Process { 
            StartInfo = new ProcessStartInfo(path)
        };
        PhantomCubeProcess.Start();
        //System.Diagnostics.Process.Start(path);
    }

    public void Update()
    {
        if (messagesReceived.Count > 0)
        {
            DebugModeLog("Parsing Line.");
            ParseCommand(messagesReceived[0]);
            messagesReceived.RemoveAt(0);
        }
    }

    public void UpdateReaderThread()
    {
        pollTimer -= Time.deltaTime;
        if (pollTimer <= 0)
        {
            pollTimer += pollTimerStart;
            //PollPipedMessagesFromRemote();
        }
        //SendTextThroughPipe();
        //reader.Close();
    }
    
    public void ThreadPollPipedMessagesFromRemote()
    {
        while (!shutdown)
        {

            if (pipePhantomToMilla is not { IsConnected: true })
            {
                Debug.Log("No connection established. Attempting to make new connection.");
                ConnectPipes();
                Thread.Sleep(250);
                continue;
            }
            else
            {
                if (!sentConnectionMessage)
                {
                    DebugModeLog("Phantom: Connection Established.");
                    sentConnectionMessage = true;
                }
            }

            if (reader == null)
            {
                DebugModeLog("Creating reader.");
                reader = new StreamReader(pipePhantomToMilla);
            }
            DebugModeLog("Before Reader Conditional...");
            if (!reader.EndOfStream)
            {
                DebugModeLog("Reading Line.");
                messagesReceived.Add(reader.ReadLine());
                //var line = reader.ReadLine();
            }
            else
            {
                DebugModeLog("Reader stream has ended.");
            }
        }

        return;
    }

    public void ConstructPipes()
    {
        if (pipePhantomFromMilla == null)
        {
            Debug.Log("Constructing Pipe From Phantom");
            pipePhantomFromMilla = new NamedPipeClientStream("RSNPhantomFromMilla");
        }
        if (pipePhantomToMilla == null)
        {
            Debug.Log("Constructing Pipe To Phantom");
            pipePhantomToMilla = new NamedPipeClientStream("RSNPhantomToMilla");
        }
    }

    private bool ConnectPipes()
    {
        bool failed = false;
        ConstructPipes();
        try
        {
            if (!pipePhantomFromMilla.IsConnected)
            {
                Debug.Log("Connecting pipe From Phantom");
                pipePhantomFromMilla.Connect(1000);
            }
            Debug.Log("Succeeded");
        }
        catch (Exception e)
        {
            failed = true;
            Debug.LogWarning("Time out when creating pipe connections.");
            //Debug.LogWarning($"{e.ToString()}\r\n{e.Message}\r\n{e.StackTrace }");
        }
        
        try
        {
            if (!pipePhantomToMilla.IsConnected)
            {
                Debug.Log("Connecting pipe To Phantom");
                pipePhantomToMilla.Connect(1000);
            }
            Debug.Log("Succeeded");
        }
        catch (Exception e)
        {
            failed = true;
            Debug.LogWarning("Time out when creating pipe connections.");
            //Debug.LogWarning($"{e.ToString()}\r\n{e.Message}\r\n{e.StackTrace }");
        }

        return failed;
    }


    public void SendTextThroughPipe(string text)
    {
        Debug.Log("Attempting to send: " + text);
        if (pipePhantomFromMilla == null) 
        {
            pipePhantomFromMilla = new NamedPipeClientStream("RSNPhantomFromMilla");
        }
        if (!pipePhantomFromMilla.IsConnected) 
        {
            var failed = false;
            try { failed = ConnectPipes(); }
            catch { failed = true; }
            if (failed) { return; }
        }
        if (writer == null) 
        {
            writer = new StreamWriter(pipePhantomFromMilla);
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
            Debug.Log("Transmission received: Experiment successful. Terminating test subject.");
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
        DebugModeLog("Done parsing command.");
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
        //Cleanup();
    }

    public void OnApplicationQuit()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        DebugModeLog("a");
        try
        {
            shutdown = true;
            
            // Forcefeed a read to encourage shutdown. If trying to handle normal communication, don't imitate this.
            if (pipePhantomToMilla != null)
            {
                DebugModeLog("a?");
                var shutdownWriter = new StreamWriter(pipePhantomToMilla);
                DebugModeLog("a??");
                shutdownWriter.WriteLine("SHUTDOWN");
                DebugModeLog("a???");
                shutdownWriter.Flush();
                shutdownWriter.Close();
                DebugModeLog("a????");
            }
            if (PhantomCubeProcess != null && !PhantomCubeProcess.HasExited)
            {
                PhantomCubeProcess.CloseMainWindow();
            }
            if (pipePhantomFromMilla != null)
            {
                Debug.Log("aa");
                pipePhantomFromMilla.Close();
                Debug.Log("aaa");
                //pipePhantomFromMilla.Disconnect();
                /*
                if (asyncResult != null)
                {
                    Debug.Log("aaaa");
                    //pipePhantomFromMilla.EndWaitForConnection(asyncResult);
                    Debug.Log("WHY");
                }
                */
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
        if (pipePhantomToMilla != null)
        {
            pipePhantomToMilla.Close();
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

    public void DebugModeLog(string txt)
    {
        if (AllowDebugModeMessages)
        {
            Debug.Log(txt);
        }
    }
}