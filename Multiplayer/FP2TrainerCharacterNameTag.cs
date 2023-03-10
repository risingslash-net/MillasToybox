using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace RisingSlash.FP2Mods.MillasToybox
{
    public class FP2TrainerCharacterNameTag : MonoBehaviour
    {
        public static FP2TrainerCharacterNameTag instance;
        public Dictionary<int, GameObject> goNametags;
        public Dictionary<int, TextMesh> tmNametags;
        public Dictionary<int, FPPlayer> idToPlayers;
        private List<string> placeholderNames;

        public Camera renderCamera = null;
        public GameObject goStageCamera = null;

        public Vector3 posRelativeToCam = Vector3.zero;
        
        public void Start()
        {
            if (instance != null)
            {
                GameObject.Destroy(this);
            }
            else
            {
                instance = this;
                this.transform.parent = MillasToybox.goFP2Trainer.transform;
                if (!MillasToybox.DisplayNametags.Value)
                {
                    gameObject.SetActive(false);
                    this.enabled = false;
                }
            }

            try
            {
                renderCamera = GameObject.Find("Render Camera").GetComponent<Camera>();
            }
            catch (Exception e)
            {
                //do something
            }
            
            goNametags = new Dictionary<int, GameObject>();
            tmNametags = new Dictionary<int, TextMesh>();

            placeholderNames = new List<string>();
            placeholderNames.Add("Ann");
            placeholderNames.Add("Dazl");
            placeholderNames.Add("Sparks");
            placeholderNames.Add("Edna");

            if (MillasToybox.fpplayers != null)
            {
                foreach (FPPlayer fpp in MillasToybox.fpplayers)
                {
                    GameObject goNametag = InstantiateNewNametag(fpp);
                    goNametags.Add(fpp.GetInstanceID(), goNametag);
                    tmNametags.Add(fpp.GetInstanceID(), goNametag.GetComponent<TextMesh>());
                }
            }
        }

        public void Update()
        {
            foreach (var nt in goNametags.Values)
            {
                nt.SetActive(MillasToybox.DisplayNametags.Value);
            }
            
            if (!MillasToybox.DisplayNametags.Value)
            {
                return;
            }
            
            // Make sure cameras exist.
            try
            {
                if (renderCamera == null)
                {
                    renderCamera = GameObject.Find("Render Camera").GetComponent<Camera>();
                }

                if (renderCamera != null)
                {
                    //posRelativeToCam = renderCamera.WorldToScreenPoint(fpp.transform.position);
                }
                else
                {
                    //MillasToybox.Log("dabDABdabDABdabDAB");
                    //posRelativeToCam = MillasToybox.GetPositionRelativeToCamera(FPCamera.stageCamera, fpp.transform.position);
                }

                if (goStageCamera == null)
                {
                    goStageCamera = FPCamera.stageCamera.gameObject;
                }
            }
            catch (Exception e)
            {
                MillasToybox.Log(e.Message + e.StackTrace);
            }

            
            // Commenting this out because it seems to be breaking the rest of the fucntionality for the time being. Can fix later.
            /*
            foreach (var keyValuePair in idToPlayers)
            {
                if (keyValuePair.Value == null)
                {
                    GameObject.Destroy(goNametags[keyValuePair.Key]);
                    goNametags.Remove(keyValuePair.Key);
                    tmNametags.Remove(keyValuePair.Key);
                    tmNametags.Remove(keyValuePair.Key);
                    break; // Only removes one per frame. Potential optimization here.
                }
            }
            */

            foreach (FPPlayer fpp in MillasToybox.fpplayers)
            {
                if (fpp != null && !goNametags.ContainsKey(fpp.GetInstanceID()))
                {
                    GameObject goNametag = InstantiateNewNametag(fpp);
                    goNametags.Add(fpp.GetInstanceID(), goNametag);
                    tmNametags.Add(fpp.GetInstanceID(), goNametag.GetComponent<TextMesh>());
                    idToPlayers.Add(fpp.GetInstanceID(), fpp);
                }

                var go = goNametags[fpp.GetInstanceID()];
                var tm = tmNametags[fpp.GetInstanceID()];
                tm.transform.parent = fpp.transform;
                
                // Move it under the character.
                go.transform.localPosition = new Vector3(0, 64, 0);
                go.transform.localScale = new Vector3(Mathf.Abs(go.transform.localScale.x), 
                    go.transform.localScale.y, 
                    go.transform.localScale.z);
                go.transform.rotation = Quaternion.identity;
                go.layer = go.transform.parent.gameObject.layer;

                // TODO: This is the actual important text bit, don't forget to uncomment this.
                
                tm.text = Regex.Replace(tm.text, @"\(.+\)",
                    $"({String.Format("{0:0.00}",fpp.health)} / {String.Format("{0:0.00}",fpp.healthMax)})");
                
                
                
                /*
                this.transform.position = new Vector3(renderCamera.transform.position.x + (640f / 2f), 
                    renderCamera.transform.position.y - (360f / 2f), 
                    renderCamera.transform.position.z);
                */

                try
                {
                    var guessedWidth = tm.text.Length * tm.characterSize;
                    var guessedHeight = tm.characterSize;

                    go.transform.position += new Vector3(-guessedWidth / 2, 0, 0);



                    // Still bugged. Probably need to use some other way to gauge the bounds than the camera direction properties.
                    /*
                    if (go.transform.position.x + (guessedWidth / 2) > FPCamera.stageCamera.right)
                    {
                        go.transform.position += new Vector3(FPCamera.stageCamera.right - (guessedWidth / 2f), 0, 0);
                    }
                
                    if (go.transform.position.x - (guessedWidth / 2) < FPCamera.stageCamera.left)
                    {
                        go.transform.position += new Vector3(FPCamera.stageCamera.left + (guessedWidth / 2f), 0, 0);
                    }
                
                    if (go.transform.position.y + (guessedHeight / 2) > FPCamera.stageCamera.top)
                    {
                        go.transform.position += new Vector3(0, FPCamera.stageCamera.top - (guessedHeight / 2f), 0);
                    }
                
                    if (go.transform.position.y - (guessedHeight / 2) < FPCamera.stageCamera.bottom)
                    {
                        go.transform.position += new Vector3(0, FPCamera.stageCamera.bottom + (guessedHeight / 2f), 0);
                    }
                    */
                }

                catch (Exception e)
                {
                    MillasToybox.Log(e.Message + e.StackTrace);
                }

                
            }
        }

        public GameObject InstantiateNewNametagBaseObject(FPBaseObject fbo)
        {
            GameObject goNametag = null;
            if (MillasToybox.goFancyTextPosition != null)
            {
                goNametag = Object.Instantiate(MillasToybox.goFancyTextPosition);

                goNametag.layer = 5;
                goNametag.SetActive(true);
                var tm = goNametag.GetComponent<TextMesh>();
                tm.font = MillasToybox.fpMenuFont;
                tm.GetComponent<MeshRenderer>().materials[0] = MillasToybox.fpMenuMaterial;
                tm.characterSize = 10;
                tm.anchor = TextAnchor.LowerCenter;

                tm.text = fbo.name;
                
                // Stopgap measure just to make this look more interesting than it actually is:
                if (goNametags != null)
                {
                    int index = goNametags.Count % placeholderNames.Count;
                    tm.text = placeholderNames[index];
                }
            }

            return goNametag;
        }
        
        public GameObject InstantiateNewNametag(FPPlayer fpp)
        {
            GameObject goNametag = null;
            if (MillasToybox.goFancyTextPosition != null)
            {
                goNametag = Object.Instantiate(MillasToybox.goFancyTextPosition);

                goNametag.layer = 5;
                goNametag.SetActive(true);
                var tm = goNametag.GetComponent<TextMesh>();
                tm.font = MillasToybox.fpMenuFont;
                tm.GetComponent<MeshRenderer>().materials[0] = MillasToybox.fpMenuMaterial;
                tm.characterSize = 10;
                tm.anchor = TextAnchor.LowerCenter;
                tm.text = $"{fpp.name} ({Mathf.Round(fpp.health)} / {Mathf.Round(fpp.healthMax)})";

                // Stopgap measure just to make this look more interesting than it actually is:
                if (goNametags != null)
                {
                    int index = goNametags.Count % placeholderNames.Count;
                    tm.text = $"{placeholderNames[index]} ({Mathf.Round(fpp.health)} / {Mathf.Round(fpp.healthMax)})";
                    MillasToybox.Log($"index {index} - newName {tm.text}\n");
                }
                
                tm.transform.parent = fpp.transform;
                var go = tm.gameObject;
                // Move it under the character.
                go.transform.localPosition = new Vector3(0, 64, 0);
                go.transform.localScale = new Vector3(Mathf.Abs(go.transform.localScale.x), 
                    go.transform.localScale.y, 
                    go.transform.localScale.z);
                go.transform.rotation = Quaternion.identity;
                go.layer = go.transform.parent.gameObject.layer;
                var lrs = go.AddComponent<LockRotScale>();
                lrs.lockQuaternion = Quaternion.identity;
                lrs.lockScale = Vector3.one;

                BindNametagToPlayerInfo.Bind(tm, fpp);

            }

            return goNametag;
        }
    }
}