using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    [Flags]
    public enum BitwiseInputState : short
    {
        NONE = 0,
        UP = 1,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 8,
        
        JUMP = 16,
        ATTACK = 32,
        SPECIAL = 64,
        GUARD = 128,
        
        PAUSE = 256
    };
    
    [Serializable]
    public class FP2TrainerInputQueue
    {
        // 9 bools, an index, and a timestamp.
        protected List<TimestampedInputs> timestampedInputsList;
        protected List<TimestampedInputs> timestampedInputsPurgedForFileWrite;
        //protected int maxLength = 30;
        protected int maxLength = 30;
        protected int maxPurgeListLength = 150;
        protected int countSteps = 0;
        public int charID = 0;
        public string stageName = "";
        public string userName = "";
        protected float elapsedTime;
        
        protected bool shouldSkipAddingHeader = false;
        protected bool usePurgeListForFileContentBuffer = true;

        public FP2TrainerInputQueue()
        {
            timestampedInputsList = new List<TimestampedInputs>();
            timestampedInputsList.Capacity = this.maxLength;
            
            timestampedInputsList = new List<TimestampedInputs>();
            timestampedInputsList.Capacity = this.maxPurgeListLength;
            elapsedTime = 0f;
        }
        
        public FP2TrainerInputQueue(int maxLength)
        {
            timestampedInputsList = new List<TimestampedInputs>();
            this.maxLength = maxLength;
            timestampedInputsList.Capacity = this.maxLength;
            elapsedTime = 0f;
        }

        public void AddTime(float amountOfTime)
        {
            elapsedTime += amountOfTime;
        }
        
        public void IncrementStep()
        {
            countSteps++;
        }
        
        public float GetTimeElapsed()
        {
            return elapsedTime;
        }

        public int GetCountSteps()
        {
            return this.countSteps;
        }
        
        public void SetCountSteps(int val)
        {
            this.countSteps = val;
        }

        public void SetMaxLength(int newMaxLength)
        {
            maxLength += newMaxLength;
            timestampedInputsList.Capacity = maxLength;
        }

        public TimestampedInputs Add(TimestampedInputs timestampedInputs)
        {
            TimestampedInputs purgedInputs = null;
            if (timestampedInputsList.Count >= maxLength && maxLength > 0 && !usePurgeListForFileContentBuffer)
            {
                SaveQueueToFile();
            }

            while (timestampedInputsList.Count >= maxLength && maxLength > 0)
            {
                purgedInputs = timestampedInputsList[0];
                //MillasToybox.Log("Purging: \n" + purgedInputs.ToString());
                timestampedInputsList.RemoveAt(0);
                
                if (usePurgeListForFileContentBuffer)
                {
                    if (timestampedInputsPurgedForFileWrite == null)
                    {
                        timestampedInputsPurgedForFileWrite = new List<TimestampedInputs>();
                        // We need to basically do the same behavior of this function on this other set... oops.
                    }
                    timestampedInputsPurgedForFileWrite.Add(purgedInputs);
                    if (timestampedInputsPurgedForFileWrite.Count >= maxPurgeListLength && maxPurgeListLength > 0)
                    {
                        // Write to file then drain.
                        SaveQueueToFile(null, timestampedInputsPurgedForFileWrite, String.Empty);
                        MillasToybox.Log("Purge is full, flushing.");
                        timestampedInputsPurgedForFileWrite.Clear();
                        MillasToybox.Log("Drained");
                    }
                }
            }
            
            /*
            if (usePurgeListForFileContentBuffer)
            {
                if (timestampedInputsPurgedForFileWrite != null && timestampedInputsPurgedForFileWrite.Count >= maxPurgeListLength && maxPurgeListLength > 0 )
                {
                    SavePurgeQueueToFile();
                }
            }
            */

            if (timestampedInputs.timestamp == 0f)
            {
                timestampedInputs.timestamp = elapsedTime;
            }

            if (timestampedInputs.numStep == -1)
            {
                timestampedInputs.numStep = countSteps;
                countSteps++;
            }
            else
            {
                countSteps = timestampedInputs.numStep + 1;
            }

            timestampedInputsList.Add(timestampedInputs);

            return purgedInputs;
        }
        
        public TimestampedInputs GetLatest()
        {
            TimestampedInputs ti = null;
            int count = timestampedInputsList.Count;
            if (count > 0)
            {
                ti = timestampedInputsList[count - 1];
            }

            return ti;
        }
        
        public TimestampedInputs GetPrevious()
        {
            TimestampedInputs ti = null;
            int count = timestampedInputsList.Count;
            if (count > 1)
            {
                ti = timestampedInputsList[count - 2];
            }
            else if (count > 0)
            {
                ti = timestampedInputsList[count - 1];
            }

            return ti;
        }
        
        public TimestampedInputs GetClosestToTimestamp(float targetTimestamp, float playerToNetworkTimeOffset)
        {
            float timestamp = targetTimestamp - playerToNetworkTimeOffset;
            if (timestamp < 0)
            {
                timestamp = 0;
            }
            
            TimestampedInputs ti = null;
            for (int i = 0; i < timestampedInputsList.Count; i++)
            {
                if (timestamp >= timestampedInputsList[i].timestamp)
                {
                    ti = timestampedInputsList[i];
                }
                else
                {
                    float distToPrev = float.PositiveInfinity;
                    float distToNext = float.PositiveInfinity;
                    if (ti != null)
                    {
                        distToPrev = Mathf.Abs(ti.timestamp - timestamp);
                    }
                    distToNext = Mathf.Abs(timestampedInputsList[i].timestamp - timestamp);

                    if (distToNext < distToPrev)
                    {
                        ti = timestampedInputsList[i];
                    }
                    break;
                }
            }

            return ti;
        }

        public TimestampedInputs GetByIndexStep(int step)
        {
            TimestampedInputs ti = null;
            
            if (step >= timestampedInputsList.Count)
            {
                step = timestampedInputsList.Count - 1;
            }
            else if (step < 0)
            {
                step = 0;
            }

            ti = timestampedInputsList[step];

            return ti;
        }

        override public string ToString()
        {
            string str = "---{InputQueue}---";
            foreach (var tsi in timestampedInputsList)
            {
                str += tsi.ToString() + "\n";
            }
            str += "---{EndInputQueue}---";

            return str;
        }

        public void SaveQueueToFile(FPPlayer fpp = null, 
            List<TimestampedInputs> timestampedInputsListToWrite = null, string additionalHeaderText = "")
        {
            if (MillasToybox.SaveGhostFiles.Value)
            {

                if (timestampedInputsListToWrite == null)
                {
                    if (usePurgeListForFileContentBuffer)
                    {
                        timestampedInputsListToWrite = this.timestampedInputsPurgedForFileWrite;
                    }
                    else
                    {
                        timestampedInputsListToWrite = this.timestampedInputsList;
                    }
                }

                if (fpp == null && FPStage.currentStage != null)
                {
                    fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                }


                var allTimestampedInputs = "";

                if (!shouldSkipAddingHeader)
                {
                    shouldSkipAddingHeader = true;
                    allTimestampedInputs += $"maxLength|{maxLength}\n";
                    allTimestampedInputs += $"countSteps|{countSteps}\n";
                    allTimestampedInputs += $"elapsedTime|{elapsedTime}\n";
                    
                    if (fpp!= null)
                    {
                        if (FPStage.currentStage != null)
                        {
                            allTimestampedInputs += $"stageName|{FPStage.currentStage.stageName}\n";
                            allTimestampedInputs += $"charID|{(int)fpp.characterID}\n";
                            allTimestampedInputs += "userName|SpeedRunner\n";
                        }
                    }
                
                    allTimestampedInputs += additionalHeaderText;
                }

                foreach (var tsi in timestampedInputsListToWrite)
                {
                    allTimestampedInputs += tsi.ToString() + "\n";
                }

                var fileName = "Inputs" + string.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".ghost"; // Need to make this so that once a file starts writing it appends to the same file.
                if (fpp!= null)
                {
                    if (FPStage.currentStage != null)
                    {
                        fileName = $"{FPStage.currentStage.stageName}-{fileName}";
                    }
                    
                    fileName = $"{fpp.name}-{fpp.GetInstanceID().ToString()}-{fileName}";
                }

                if (FPStage.currentStage != null)
                {
                    fileName = Path.Combine($"{FPStage.currentStage.stageName}\\", fileName);
                    System.IO.Directory.CreateDirectory($"ghosts\\{FPStage.currentStage.stageName}\\");
                }
                
                fileName = Path.Combine("ghosts\\", fileName);
                MillasToybox.Log($"Finna write {fileName}");

                if (File.Exists(fileName))
                {
                    MillasToybox.Log(fileName + " already exists... Appending.");
                    //return;
                }

                var sr = File.AppendText(fileName);
                sr.WriteLine(allTimestampedInputs);
                sr.Close();

                MillasToybox.Log("File written and closed.");
            }
            else
            {
                //MillasToybox.Log("Warped already...");
            }
        }
        
        public static FP2TrainerInputQueue LoadQueueFromFileMostRecent()
        {
            var result = new FP2TrainerInputQueue();
            result.maxLength = 1_000_000; //Roughly 4 hours. Probably needs more memory than a PC would have.

            if (MillasToybox.SaveGhostFiles.Value)
            {
                string fileName = "";

                if (FPStage.currentStage != null)
                {
                    fileName = Path.Combine($"{FPStage.currentStage.stageName}\\", fileName);
                }
                
                fileName = Path.Combine("ghosts\\", fileName);


                try
                {

                    var directory = new DirectoryInfo(fileName);
                    var ghostFilePaths = directory.GetFiles("*.ghost");
                    var latestGhostFile = ghostFilePaths.OrderByDescending(f => f.LastWriteTime).First();

                    int fileCharID = -1;
                    string fileStageName = String.Empty;
                    
                    string[] lines = System.IO.File.ReadAllLines(latestGhostFile.FullName);

                    Regex rxInputQueueLine = new Regex(@"^\d+\|[\d\.]+\|\d+");

                    foreach (var line in lines)
                    {
                        // Level and Character data
                        if (line.Contains("charID"))
                        {
                            result.charID = int.Parse(line.Split('|')[1]);
                        }
                        
                        if (line.Contains("stageName"))
                        {
                            result.stageName = line.Split('|')[1];
                        }
                        
                        if (line.Contains("userName"))
                        {
                            result.userName = line.Split('|')[1];
                        }

                        // Input Data
                        if (rxInputQueueLine.IsMatch(line))
                        {
                            var segments = line.Split('|');
                            var tsi = new TimestampedInputs(
                                Convert.ToInt32(segments[0]),
                                Convert.ToSingle(segments[1]),
                                Convert.ToInt16(segments[2], 2));
                            result.Add(tsi);
                        }
                    }
                }
                catch (NullReferenceException e)
                {
                    MillasToybox.Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                    MillasToybox.Log(e.StackTrace);
                }
                
                //asdfasdfasfd
            }
            return result;
        }
        
        public static FP2TrainerInputQueue LoadQueueFromFile(string filename)
        {
            var result = new FP2TrainerInputQueue();
            try
            {
            
                result.maxLength = 1_000_000; //Roughly 4 hours. Probably needs more memory than a PC would have.
                
                string ghostLevel = "";
                int ghostCharacter = -1;
                string ghostScreenName = "Speedrunner";
                
                if (MillasToybox.SaveGhostFiles.Value)
                {
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines(filename);

                        Regex rxInputQueueLine = new Regex(@"^\d+\|[\d\.]+\|\d+");

                        foreach (var line in lines)
                        {
                            if (line.Contains("charID"))
                            {
                                result.charID = int.Parse(line.Split('|')[1]);
                            }
                        
                            if (line.Contains("stageName"))
                            {
                                result.stageName = line.Split('|')[1];
                            }
                        
                            if (line.Contains("userName"))
                            {
                                result.userName = line.Split('|')[1];
                            }
                            
                            // Input Data
                            if (rxInputQueueLine.IsMatch(line))
                            {
                                var segments = line.Split('|');
                                result.Add(new TimestampedInputs(
                                    Convert.ToInt32(segments[0]),
                                    Convert.ToSingle(segments[1]),
                                        Convert.ToInt16(segments[2], 2)));
                            }
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        MillasToybox.Log("Null reference exception when trying to load ghost file. Canceling.");
                        MillasToybox.Log(e.StackTrace);
                    }
                }
            }
            catch (Exception e)
            {
                MillasToybox.Log(e.ToString());
                MillasToybox.Log(e.StackTrace);
            }
            
            return result;
        }
        
        public void SavePurgeQueueToFile(FPPlayer fpp = null, 
            List<TimestampedInputs> timestampedInputsListToWrite = null, string additionalHeaderText = "")
        {
            if (fpp == FP2TrainerAllyControls.leadPlayer)
            {
                SaveQueueToFile(fpp, timestampedInputsPurgedForFileWrite, additionalHeaderText);
            }
        }
    }

    public class TimestampedInputs
    {
        public int numStep = 0;
        public float timestamp = 0f;
        public BitwiseInputState bitwiseInputs = BitwiseInputState.NONE;

        public TimestampedInputs()
        {
            numStep = -1;
            timestamp = 0f;
            bitwiseInputs = BitwiseInputState.NONE;
        }
        
        public TimestampedInputs(int numStep, float timestamp, short flags)
        {
            this.numStep = numStep;
            this.timestamp = timestamp;
            bitwiseInputs = (BitwiseInputState)flags;
        }
        
        public TimestampedInputs(bool u, bool d, bool l, bool r, 
            bool j, bool a, bool s, bool g, 
            bool p ) : this(0f, u, d, l, r, j, a, s, g, p)
        {
        }

        public TimestampedInputs(float timestamp, bool u, bool d, bool l, bool r,
            bool j, bool a, bool s, bool g,
            bool p) : this(-1, 0f, u, d, l, r, j, a, s, g, p)
        {
        }

        public TimestampedInputs(int numStep, float timestamp, bool u, bool d, bool l, bool r,
            bool j, bool a, bool s, bool g,
            bool p)
        {
            this.numStep = numStep;
            this.timestamp = timestamp;
            this.bitwiseInputs = BitwiseInputState.NONE;

            this.bitwiseInputs |= u ? BitwiseInputState.UP : BitwiseInputState.NONE;
            this.bitwiseInputs |= d ? BitwiseInputState.DOWN : BitwiseInputState.NONE;
            this.bitwiseInputs |= l ? BitwiseInputState.LEFT : BitwiseInputState.NONE;
            this.bitwiseInputs |= r ? BitwiseInputState.RIGHT : BitwiseInputState.NONE;
            
            this.bitwiseInputs |= j ? BitwiseInputState.JUMP : BitwiseInputState.NONE;
            this.bitwiseInputs |= a ? BitwiseInputState.ATTACK : BitwiseInputState.NONE;
            this.bitwiseInputs |= s ? BitwiseInputState.SPECIAL : BitwiseInputState.NONE;
            this.bitwiseInputs |= g ? BitwiseInputState.GUARD : BitwiseInputState.NONE;
            
            this.bitwiseInputs |= p ? BitwiseInputState.PAUSE : BitwiseInputState.NONE;
        }
        
        public static bool HasFlag(BitwiseInputState theFlags, BitwiseInputState flagCondition)
        {
            return (theFlags & flagCondition) == flagCondition;
        }
        
        public void MapInputsToFPPlayer(FPPlayer fpp)
        {
            fpp.input.up = HasFlag(bitwiseInputs, BitwiseInputState.UP);
            fpp.input.down = HasFlag(bitwiseInputs, BitwiseInputState.DOWN);
            fpp.input.left = HasFlag(bitwiseInputs, BitwiseInputState.LEFT);
            fpp.input.right = HasFlag(bitwiseInputs, BitwiseInputState.RIGHT);
            
            fpp.input.jumpHold = HasFlag(bitwiseInputs, BitwiseInputState.JUMP);
            fpp.input.attackHold = HasFlag(bitwiseInputs, BitwiseInputState.ATTACK);
            fpp.input.specialHold = HasFlag(bitwiseInputs, BitwiseInputState.SPECIAL);
            fpp.input.guardHold = HasFlag(bitwiseInputs, BitwiseInputState.GUARD);
        }

        override public string ToString()
        {
            return String.Format("{0}|{1}|{2}\n", numStep, timestamp, Convert.ToString((int)bitwiseInputs, 2).PadLeft(8, '0'));
        }
        public string ToFriendlyString()
        {
            return String.Format("{0}|{1}|{2}\n", numStep, timestamp, bitwiseInputs);
        }
    }
}