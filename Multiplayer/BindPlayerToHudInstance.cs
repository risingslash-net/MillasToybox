using System;
using System.Reflection;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;
public class BindPlayerToHudInstance : MonoBehaviour
{
    public FPPlayer fpplayer;
    public FPHudMaster hudMaster;
    public void Update()
    {
        if (fpplayer == null)
        {
            fpplayer = gameObject.GetComponent<FPPlayer>();
        }
        else
        {
            if (hudMaster != null)
            {
                hudMaster.targetPlayer = fpplayer;
                var hudTimeGlobalBar = (GameObject)(hudMaster.GetType()
                    .GetField("hudTimeGlobalBar", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(hudMaster));
                if (hudTimeGlobalBar != null)
                {
                    hudTimeGlobalBar.SetActive(false);
                }
            }
        }
    }
}