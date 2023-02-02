using System.Collections.Generic;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class CameraHotspotVisualizer : MonoBehaviour
{
     public static List<FPCameraHotspot> fpCameraHotspots;
        public static bool hasSpawnedVisualizers = false;
        List<GameObject> fpCameraHotspotsVisualizers;
        GameObject goCube;
        public GameObject goCubeExit;
        Renderer renCube;
        Sprite pixelSprite;
        public FPCameraHotspot myHotspot;

        // Use this for initialization
        public void SpawnVisualizers()
        {
            if (hasSpawnedVisualizers)
            {
                MillasToybox.Log("Tried to FPCameraHotspot visualizers, but flag suggest status was not Reset.");
                return;
            }
            
            MillasToybox.Log("Started FP Camera Hotspot Visualizer");
            fpCameraHotspotsVisualizers = new List<GameObject>();
            fpCameraHotspots = new List<FPCameraHotspot>((GameObject.FindObjectsOfType<FPCameraHotspot>()));
            return;
            
            MillasToybox.Log("Configuring visualizer properties.");
            Texture2D greenPixel = new Texture2D(1, 1);
            greenPixel.SetPixel(0, 0, new Color(0, 1, 0, 1f));
            pixelSprite = Sprite.Create(greenPixel, new Rect(0.0f, 0.0f, greenPixel.width, greenPixel.height),
                new Vector2(0.5f, 0.5f), 1.0f);
            
            
            Texture2D greenPixelTranslucent = new Texture2D(1, 1);
            greenPixelTranslucent.SetPixel(0, 0, new Color(0.2f, 1, 0.2f, 0.5f));
            var pixelSpriteExit = Sprite.Create(greenPixelTranslucent, new Rect(0.0f, 0.0f, greenPixelTranslucent.width, greenPixelTranslucent.height),
                new Vector2(0.5f, 0.5f), 1.0f);
            
            
            MillasToybox.Log("Spawning FPCameraHotspotVisualizer Objects.");
            foreach (FPCameraHotspot ps in fpCameraHotspots)
            {
                //Debug.Log(System.String.Format("Adding Sprite Indicators to {0} FPCameraHotspots.\n", ps.name));
                MillasToybox.Log(System.String.Format("Adding Sprite Indicators to {0} FPCameraHotspots.\n", ps.name));

                //goCube = new GameObject();

                var newVisualizerScript = goCube.AddComponent<CameraHotspotVisualizer>();
                newVisualizerScript.goCubeExit = new GameObject("CameraHotspotExitRangeVis-" + ps.name);
                
                goCube = ps.gameObject;
                
                var spriteRenderer = goCube.AddComponent<SpriteRenderer>();

                newVisualizerScript.myHotspot = ps;
                spriteRenderer.sprite = pixelSprite;
                
                goCube.transform.localScale = new Vector3((ps.range.x) * 2, 
                    (ps.range.y) * 2, 1f);

                /*
                if (blah != null)
                {
                    blah = GameObject.Instantiate(blah);
                    GameObject.Destroy(blah.GetComponent<Animator>());
                    blah.transform.position = new Vector3(ps.transform.position.x, ps.transform.position.y,
                        blah.transform.position.z);
                    blah.transform.localScale = new Vector3(ps.xsize, ps.ysize, 1f);

                    goCube.layer = blah.layer;
                    //spriteRenderer.material = blah.GetComponent<SpriteRenderer>().material;
                    //blah.transform.localScale = new Vector3(50, 50, 1f);
                    
                    blah.SetActive(false);
                }
                */

                goCube.layer = 8; //FG Plane A - D are 98 9 10 11. 13 - 27 are BG0 - BG15. 28 and 29 are LightingSetup and Lighting.
                renCube = goCube.GetComponent<SpriteRenderer>();
                renCube.sortingOrder = 2000;

                newVisualizerScript.renCube = renCube;

                //newVisualizerScript.goCubeExit.transform.parent = goCube.transform;
                newVisualizerScript.goCubeExit.transform.position = goCube.transform.position;
                var spriteRendererExit = newVisualizerScript.goCubeExit.AddComponent<SpriteRenderer>();
                
                spriteRendererExit.sprite = pixelSpriteExit;
                
                newVisualizerScript.goCubeExit.transform.localScale = new Vector3((ps.range.x + ps.exitOffset) * 2, 
                    (ps.range.y + ps.exitOffset) * 2, 1f);

                newVisualizerScript.goCubeExit.transform.position = goCube.transform.position;
                //renCube.material.color = new Color(1, 0, 0, 0.7f);
                
                fpCameraHotspotsVisualizers.Add(goCube);
                
            }
            hasSpawnedVisualizers = true;
        }

        public void Reset()
        {
            hasSpawnedVisualizers = false;
            if (fpCameraHotspotsVisualizers != null)
            {
                foreach (GameObject go in fpCameraHotspotsVisualizers)
                {
                    GameObject.Destroy(go);
                }
                fpCameraHotspotsVisualizers.Clear();
            }
        }

        public void SetActiveOfSpawnedVisualizers(bool planeSwitchVisualizersVisible)
        {
            foreach (GameObject go in fpCameraHotspotsVisualizers)
            {
                if (go != null)
                {
                    go.SetActive(planeSwitchVisualizersVisible);
                    go.GetComponent<SpriteRenderer>().enabled = planeSwitchVisualizersVisible;
                }
            }
        }

        public void AnnihilateGameObjects(params string[] gameObjectNames)
        {
            foreach (string nameOfGameObject in gameObjectNames)
            {
                GameObject go = GameObject.Find(nameOfGameObject);
                if (go != null)
                {
                    GameObject.Destroy(go);
                    MillasToybox.Log($"Deleted {go.name}\n");
                }
                else
                {
                    MillasToybox.Log($"Could not find {nameOfGameObject} to delete.\n");
                }
            }

        }

        public void Update()
        {
            if (myHotspot != null && renCube != null)
            {
                
            }
        }
}