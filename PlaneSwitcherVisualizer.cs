using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    public class PlaneSwitcherVisualizer : MonoBehaviour
    {
        public static List<PlaneSwitcher> planeSwitchers;
        public static bool hasSpawnedVisualizers = false;
        List<GameObject> planeSwitchersVisualizers;
        GameObject goCube;
        Renderer renCube;
        Sprite pixelSprite;

        // Use this for initialization
        public void SpawnVisualizers()
        {
            if (hasSpawnedVisualizers)
            {
                MillasToybox.Log("Tried to spawn visualizers, but flag suggest status was not Reset.");
                return;
            }

            MillasToybox.Log("Started Plane Switcher Visualizer");
            planeSwitchersVisualizers = new List<GameObject>();
            planeSwitchers = new List<PlaneSwitcher>((GameObject.FindObjectsOfType<PlaneSwitcher>()));
            
            /*
            Debug.Log(
                System.String.Format("Found {0} PlaneSwitchers. Attempting to visualize.\n", planeSwitchers.Count));
                */


            MillasToybox.Log("Configuring visualizer properties.");
            Texture2D redPixel = new Texture2D(1, 1);
            redPixel.SetPixel(0, 0, new Color(1, 0, 0, 1f));

            pixelSprite = Sprite.Create(redPixel, new Rect(0.0f, 0.0f, redPixel.width, redPixel.height),
                new Vector2(0.5f, 0.5f), 1.0f);
            
            MillasToybox.Log("Spawning PlaneSwitchVisualizer Objects. Now with sortOrder 2k");

            var blah = GameObject.Find("Crystal");

            foreach (PlaneSwitcher ps in planeSwitchers)
            {
                //Debug.Log(System.String.Format("Adding Sprite Indicators to {0} PlaneSwitchers.\n", ps.name));
                MillasToybox.Log(System.String.Format("Adding Sprite Indicators to {0} PlaneSwitchers.\n", ps.name));

                //goCube = new GameObject();

                
                goCube = ps.gameObject;
                var spriteRenderer = goCube.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = pixelSprite;
                
                goCube.transform.localScale = new Vector3(ps.xsize, ps.ysize, 1f);

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

                //renCube.material.color = new Color(1, 0, 0, 0.7f);
                
                planeSwitchersVisualizers.Add(goCube);
                
            }
            hasSpawnedVisualizers = true;
        }

        public void Reset()
        {
            hasSpawnedVisualizers = false;
            if (planeSwitchersVisualizers != null)
            {
                foreach (GameObject go in planeSwitchersVisualizers)
                {
                    GameObject.Destroy(go);
                }
                planeSwitchersVisualizers.Clear();
            }
        }

        public void SetActiveOfSpawnedVisualizers(bool planeSwitchVisualizersVisible)
        {
            foreach (GameObject go in planeSwitchersVisualizers)
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
    }
}