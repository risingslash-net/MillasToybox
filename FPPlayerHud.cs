using System;
using System.IO;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    // The following code is heavily based on the FPBossHud class that already exists in FP2.
    // I cannot take creait for most of this class's code.
    public class FPPlayerHud : MonoBehaviour
    {
        [Header("Hud Prefabs")] public GameObject pfHudBase;

        public GameObject pfHudLifePetal;

        [Header("Player Properties")] public float maxHealth;

        public int maxPetals;

        public bool stopTimerOnDeath;

        public Vector2 healthBarOffset;

        public float barWidth = 384f;

        public Sprite barSprite;

        [Header("Player Dialog")] public PlayerDialogZone[] dialogKO;

        private FPBaseEnemy[] weakpointCheck;

        private GameObject hudBase;

        private FPHudDigit[] hudLifePetals;

        private SpriteRenderer hudBaseBarSpriteRenderer;

        private Renderer hudBaseParentMaterialRenderer;

        private float hudHealth;

        private bool isHudPetalFlashing;

        [HideInInspector] public bool bossDeathActionsExecuted;

        private bool isPaused;

        private int state = -1;

        [HideInInspector] public Vector2 hudPosition;

        public const int STATE_HIDDEN = -1;

        public const int STATE_NORMAL = 0;

        public const int STATE_MOVE_IN = 1;

        public const int STATE_MOVE_OUT = 2;

        public FPPlayer targetPlayer;

        private void Start()
        {
            // This will crunch every time because we don't have these prefabs...
            bossDeathActionsExecuted = false;
            isPaused = false;
            hudPosition.x = 320f + healthBarOffset.x;
            hudPosition.y = -342f + healthBarOffset.y;
            hudBase = UnityEngine.Object.Instantiate(pfHudBase);
            hudBaseBarSpriteRenderer = hudBase.GetComponent<SpriteRenderer>();
            hudBaseParentMaterialRenderer = hudBase.GetComponentInParent<Renderer>();
            if (hudBaseBarSpriteRenderer != null && barSprite != null)
            {
                hudBaseBarSpriteRenderer.sprite = barSprite;
            }

            float num = 227f;
            hudLifePetals = new FPHudDigit[maxPetals];
            isHudPetalFlashing = false;
            for (int i = 0; i < hudLifePetals.Length; i++)
            {
                GameObject gameObject =
                    UnityEngine.Object.Instantiate(pfHudLifePetal, new Vector3(num, 331f, 0f), default(Quaternion));
                hudLifePetals[i] = gameObject.GetComponent<FPHudDigit>();
                num += barWidth / 2f / (float)Mathf.Max(maxPetals - 1, 1);
            }

            if (targetPlayer != null)
            {
                weakpointCheck = targetPlayer.GetComponentsInChildren<BossWeakpoint>();
            }

            hudPosition.x = -320f;
            hudBase.transform.position = hudPosition;
        }

        public void MoveIn()
        {
            state = 1;
        }

        public void MoveOut()
        {
            state = 2;
        }

        public int GetState()
        {
            return state;
        }

        public void SetBarSprite(Sprite barSprite)
        {
            this.barSprite = barSprite;
            if (hudBaseBarSpriteRenderer != null)
            {
                hudBaseBarSpriteRenderer.sprite = barSprite;
            }
        }
        
        public static void LoadResourcesFromAssetBundle()
        {
            try
            {
                var pathApp = Application.dataPath;
                var pathMod = Path.Combine(Directory.GetParent(pathApp).FullName, "Mods");
                var pathModAssetBundles = Path.Combine(pathMod, "AssetBundles");
                var pathfp2t = Path.Combine(pathModAssetBundles, "fp2t");
                
                var currentAB = AssetBundle.LoadFromFile(pathfp2t);

            }
            catch (NullReferenceException e)
            {
                MillasToybox.LogError("Null reference exception when trying to load asset bundles for health bars. Canceling.");
                MillasToybox.LogError(e.StackTrace);
            }
        }

        public void AcquireHUDSprites()
        {
            if (pfHudBase != null && pfHudLifePetal != null)
            {
                return;
            }

            LoadResourcesFromAssetBundle();
            
            var testString = "Dabbington";
            MillasToybox.Log($"Dabbington: {testString.Contains("Dabbington")}");

            Transform goHudBossIcon = null;
            Transform goHudBossLifePetal = null;
            //Transform goHudBossLifePetal = null;
            Transform t1up = null;
            
            var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

            //var delete = "";
            foreach (var tf in allTransforms)
            {
                //delete += tf.name + "\n";
                if (tf.name.Contains("Hud Boss Icon"))
                {
                    goHudBossIcon = tf;
                    break;
                }
                
                if (tf.name.Contains("Hud Boss Life Petal"))
                {
                    goHudBossLifePetal = tf;
                    break;
                }
                
                /*
                if (tf.name.Contains("1up_"))
                if (tf.name.Contains("Hud Life Icon"))
                {
                    t1up = tf;
                    break;
                }
                */
            }

            //delete += "--------------------------------------------------";
            //MillasToybox.Log(delete);

            if (pfHudBase == null && goHudBossIcon != null)
            {
                pfHudBase = goHudBossIcon.gameObject;
            }
            
            if (pfHudLifePetal == null && goHudBossLifePetal != null)
            {
                pfHudLifePetal = goHudBossLifePetal.gameObject;
            }
        }
        
        public void GrabHealthValuesFromTarget() 
        {
            if (targetPlayer != null)
            {
                hudHealth = targetPlayer.health;
                maxHealth = targetPlayer.healthMax;
                maxPetals = Mathf.FloorToInt(targetPlayer.healthMax);
            }

            if (maxHealth <= 0)
            {
                maxHealth = 4;
            }

            if (maxPetals < 1)
            {
                maxPetals = Mathf.FloorToInt(targetPlayer.healthMax);
            }
        }

        private void LateUpdate()
        {
            return;
            AcquireHUDSprites();
            GrabHealthValuesFromTarget();
            
            
            if (targetPlayer == null)
            {
                return;
            }
            
            if (targetPlayer != null)
            {
                this.maxHealth = targetPlayer.healthMax;
            }

            bool flag = false;
            if (maxPetals < 1)
            {
                maxPetals = 1;
            }

            if (maxPetals != hudLifePetals.Length)
            {
                if (maxPetals < hudLifePetals.Length)
                {
                    FPHudDigit[] destinationArray = new FPHudDigit[maxPetals];
                    Array.Copy(hudLifePetals, destinationArray, maxPetals);
                    for (int i = maxPetals; i < hudLifePetals.Length; i++)
                    {
                        UnityEngine.Object.Destroy(hudLifePetals[i].gameObject);
                    }

                    hudLifePetals = destinationArray;
                }
                else
                {
                    FPHudDigit[] array = new FPHudDigit[maxPetals];
                    Array.Copy(hudLifePetals, array, hudLifePetals.Length);
                    float num = barWidth / 2f / (float)Mathf.Max(maxPetals - 1, 1);
                    for (int j = hudLifePetals.Length; j < maxPetals; j++)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate(pfHudLifePetal,
                            new Vector3(hudPosition.x - barWidth / 4f + (float)j * num, hudPosition.y + 10f, -j),
                            default(Quaternion));
                        array[j] = gameObject.GetComponent<FPHudDigit>();
                    }

                    hudLifePetals = array;
                }

                flag = true;
            }

            if (FPStage.state == FPStageState.STATE_PAUSED)
            {
                if (!isPaused)
                {
                    isPaused = true;
                    hudBase.transform.position = new Vector2(1000f, 1000f);
                    for (int k = 0; k < maxPetals; k++)
                    {
                        hudLifePetals[k].transform.position = new Vector3(1000f, 1000f, -k);
                    }
                }

                return;
            }

            if (isPaused)
            {
                isPaused = false;
                flag = true;
            }

            for (int l = 0; l < hudLifePetals.Length; l++)
            {
                if (FPStage.state == FPStageState.STATE_RUNNING)
                {
                    hudLifePetals[l].gameObject.SetActive(value: true);
                }
                else
                {
                    hudLifePetals[l].gameObject.SetActive(value: false);
                }
            }

            switch (state)
            {
                case 0:
                    if (hudHealth < targetPlayer.health)
                    {
                        hudHealth += 4f * FPStage.deltaTime;
                        if (hudHealth > targetPlayer.health)
                        {
                            hudHealth = targetPlayer.health;
                        }

                        flag = true;
                    }
                    else if (hudHealth > targetPlayer.health)
                    {
                        hudHealth -= 0.2f * FPStage.deltaTime;
                        if (hudHealth > targetPlayer.health + 1f)
                        {
                            hudHealth -= 1f * FPStage.deltaTime;
                        }

                        if (hudHealth < targetPlayer.health)
                        {
                            hudHealth = targetPlayer.health;
                        }

                        flag = true;
                    }

                    break;
                case 1:
                    if (hudPosition.x < 320f + healthBarOffset.x)
                    {
                        hudPosition.x += FPStage.deltaTime * 30f;
                    }

                    if (hudPosition.x >= 320f + healthBarOffset.x)
                    {
                        state = 0;
                        hudPosition.x = 320f + healthBarOffset.x;
                    }

                    flag = true;
                    break;
                case 2:
                    if (hudHealth < targetPlayer.health)
                    {
                        hudHealth += 4f * FPStage.deltaTime;
                        if (hudHealth > targetPlayer.health)
                        {
                            hudHealth = targetPlayer.health;
                        }
                    }
                    else if (hudHealth > targetPlayer.health)
                    {
                        hudHealth -= 1f * FPStage.deltaTime;
                        if (hudHealth < targetPlayer.health)
                        {
                            hudHealth = targetPlayer.health;
                        }
                    }

                    if (hudPosition.x < 960f)
                    {
                        hudPosition.x += FPStage.deltaTime * 30f;
                    }

                    if (hudPosition.x >= 960f)
                    {
                        state = -1;
                        hudPosition.x = 960f;
                    }

                    flag = true;
                    break;
            }

            if (flag)
            {
                hudBase.transform.position = hudPosition;
                float num2 = maxHealth / (float)maxPetals;
                for (int m = 0; m < maxPetals; m++)
                {
                    float num3 = hudHealth - (float)m * num2;
                    if (num3 > num2)
                    {
                        num3 = num2;
                    }

                    float num4 = num2 / (float)(hudLifePetals[m].digitFrames.Length - 1);
                    hudLifePetals[m].SetDigitValue((int)Mathf.Ceil(num3 / num4));
                }

                float num5 = barWidth / 2f / (float)Mathf.Max(maxPetals - 1, 1);
                if (hudHealth != targetPlayer.health)
                {
                    if (!isHudPetalFlashing)
                    {
                        isHudPetalFlashing = true;
                        for (int n = 0; n < maxPetals; n++)
                        {
                            hudLifePetals[n].GetRenderer().material = FPResources.material[8];
                        }

                        hudBaseParentMaterialRenderer.material = FPResources.material[8];
                    }

                    for (int num6 = 0; num6 < maxPetals; num6++)
                    {
                        hudLifePetals[num6].transform.position = new Vector3(
                            hudPosition.x - barWidth / 4f + (float)num6 * num5,
                            hudPosition.y + 10f + UnityEngine.Random.Range(-2f, 2f), -num6);
                    }
                }
                else if (isHudPetalFlashing)
                {
                    isHudPetalFlashing = false;
                    for (int num7 = 0; num7 < maxPetals; num7++)
                    {
                        hudLifePetals[num7].GetRenderer().material = FPResources.material[0];
                        hudLifePetals[num7].transform.position =
                            new Vector3(hudPosition.x - barWidth / 4f + (float)num7 * num5, hudPosition.y + 10f, -num7);
                    }

                    hudBaseParentMaterialRenderer.material = FPResources.material[0];
                }
                else
                {
                    for (int num8 = 0; num8 < maxPetals; num8++)
                    {
                        hudLifePetals[num8].transform.position =
                            new Vector3(hudPosition.x - barWidth / 4f + (float)num8 * num5, hudPosition.y + 10f, -num8);
                    }
                }
            }

            if (bossDeathActionsExecuted || !(targetPlayer.health <= 0f))
            {
                return;
            }

            bossDeathActionsExecuted = true;
            if (stopTimerOnDeath)
            {
                FPStage.timeEnabled = false;
            }

            if (dialogKO.Length > 0)
            {
                int num9 = UnityEngine.Random.Range(0, dialogKO.Length);
                if (dialogKO[num9] != null)
                {
                    dialogKO[num9].SetRange(new Vector2(9999f, 9999f));
                }
            }

            state = 2;
        }
    }
}