using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    public class FP2TrainerDamageNumber : MonoBehaviour
    {
        public float dmg;
        public bool showAsInt = true;
        public float dt;
        public float timeRemainingMax = 2f;
        public float timeRemaining = 2f;

        public Vector2 pos = new Vector2(0, 0);
        public Vector2 vel = new Vector2(0, 1);
        public Vector2 grav = new Vector2(0, 0.34f);

        public TextMesh tm;

        public void Start()
        {
            timeRemaining = timeRemainingMax;
        }

        public void Update()
        {
            timeRemaining -= FPStage.frameTime;

            if (timeRemaining <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                dt = FPStage.frameTime;
                vel -= grav * dt;
                var p = transform.position;
                transform.position = new Vector3(p.x + vel.x * dt, p.y + vel.y * dt, p.z);
            }
        }

        public static GameObject CreateDMGNumberObject(Vector3 newPos, float dmgNum)
        {
            GameObject goDmgNum = null;
            var goStageHUD = GameObject.Find("Stage HUD");

            if (goStageHUD != null)
            {
                //goStageHUD.energyBarGraphic.transform.parent;
                MillasToybox.Log("Looking for Energy Bar");
                var temp = goStageHUD.GetComponent<FPHudMaster>();
                GameObject temp2;
                MillasToybox.Log("6");
                if (temp != null)
                {
                    MillasToybox.Log("7");
                    temp2 = temp.pfHudEnergyBar;
                }
                else
                {
                    MillasToybox.Log("8");
                    MillasToybox.Log("This aint it.");
                    return goDmgNum;
                }


                MillasToybox.Log("9");
                var energyBarGraphic = Instantiate(temp2, temp2.transform.parent);

                energyBarGraphic.transform.localScale *= 2;

                var goNewDmgNum = energyBarGraphic;
                goNewDmgNum.SetActive(true);
                //GameObject.Destroy(goNewDmgNum.GetComponent<SpriteRenderer>()); // Can't have Sprite Renderer and Mesh Renderer.
                var tempGo = new GameObject();
                tempGo.transform.parent = goNewDmgNum.transform;
                tempGo.transform.localPosition = Vector3.zero;

                MillasToybox.Log("10");

                goNewDmgNum.transform.position = new Vector3(16, -160, 0);
                goNewDmgNum = tempGo;

                MillasToybox.Log("11");

                var textMeshDmgNum = goNewDmgNum.AddComponent<TextMesh>();
                if (textMeshDmgNum != null)
                {
                    MillasToybox.Log("12");
                    var fpMenuFont = MillasToybox.fpMenuFont;
                    MillasToybox.Log("Current value of fpMenuFont: " + fpMenuFont);
                    textMeshDmgNum.font = fpMenuFont;
                    textMeshDmgNum.characterSize = 10;
                    MillasToybox.Log("13");

                    //textMeshDmgNum.text = "I exist!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n@@@@@@@@@@@@@@@@@@@@@@@@";
                    //global::MillasToybox.MillasToybox.LogLog("Creating dmg number Attaching to Stage HUD.");
                }
                else
                {
                    MillasToybox.Log("Tried to create textMesh but failed.");
                }

                MillasToybox.Log("14");

                var fp2Tdmg = goNewDmgNum.AddComponent<FP2TrainerDamageNumber>();
                fp2Tdmg.tm = textMeshDmgNum;
                fp2Tdmg.dmg = dmgNum;
                fp2Tdmg.tm.text = fp2Tdmg.dmg.ToString();

                MillasToybox.Log("15");
                goDmgNum = goNewDmgNum;
                goDmgNum.transform.position = newPos;
            }
            else
            {
                MillasToybox.Log("16");
                goDmgNum = new GameObject();
            }

            MillasToybox.Log("17");

            return goDmgNum;
        }
    }
}