using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RisingSlash.FP2Mods.MillasToybox.GeneralScripts;

[DefaultExecutionOrder(-100)]
public class SkipScreenTransitionCountdownBar : MonoBehaviour
{
    public float maxWaitTime = 5f;
    public bool done = false;
    public void Update()
    {
        maxWaitTime -= Time.deltaTime;
        if (maxWaitTime <= 0)
        {
            done = true;
        }

        var screenTransition = Component.FindObjectOfType<FPScreenTransition>();

        if (screenTransition != null && 
            (screenTransition.transitionType == FPTransitionTypes.TITLECARD
            || screenTransition.transitionType == FPTransitionTypes.TITLECARD_2))
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