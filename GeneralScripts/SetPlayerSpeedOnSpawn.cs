using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox.GeneralScripts;

public class SetPlayerSpeedOnSpawn : MonoBehaviour
{
    public float maxWaitTime = 5f;
    public bool done = false;
    public Vector2 spawnSpeed = Vector2.zero;
    public void Update()
    {
        maxWaitTime -= Time.deltaTime;
        if (maxWaitTime <= 0)
        {
            done = true;
        }

        var screenTransition = Component.FindObjectOfType<FPScreenTransition>();
        var fpp = FPStage.currentStage.GetPlayerInstance_FPPlayer();

        if (screenTransition != null && fpp != null)
        {
            screenTransition.loadingBarDuration = -1;
            
            screenTransition.gameObject.layer = 0;
            screenTransition.titleTextObj.Go();
            Destroy(screenTransition.titleLoadingBarObj.gameObject);
            done = true;
        }

        if (done)
        {
            Destroy(this);
        }
    }
}