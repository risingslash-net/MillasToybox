using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class BindNametagToPlayerInfo : MonoBehaviour
{
    public FPPlayer fpp;
    public TextMesh nameTag;
    private void Update()
    {
        if (fpp == null || nameTag == null)
        {
            return;
        }

        nameTag.text = $"{fpp.name}: {fpp.health.ToString("0.00")} / {fpp.healthMax.ToString("0")}";
    }

    public static void Bind(TextMesh nameTag, FPPlayer fpp)
    {
        var newBind = fpp.gameObject.AddComponent<BindNametagToPlayerInfo>();
        newBind.fpp = fpp;

        if (nameTag != null)
        {
            newBind.nameTag = nameTag;
        }
        else
        {
            newBind.nameTag = fpp.GetComponentInChildren<TextMesh>();
        }
    }
}