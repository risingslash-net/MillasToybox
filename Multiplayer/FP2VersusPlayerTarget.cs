using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class FP2VersusPlayerTarget : FPBaseObject
{
    public List<FPPlayer> fpplayers = new List<FPPlayer>();
    public float assumedZoom = 1;
    private float farthestDistance = 0f;
    private void Update()
    {
        fpplayers = MillasToybox.fpplayers;
        for (int i = 0; i < fpplayers.Count; i++)
        {
            if (fpplayers[i] == null)
            {
                fpplayers.RemoveAt(i);
                i--;
            }
        }

        var newLocation = Vector3.zero;
        var newLocationFP = Vector2.zero;
        foreach (var fpp in MillasToybox.fpplayers)
        {
            if (fpp == null)
            {
                continue;
            }

            newLocation += fpp.transform.position;
            newLocationFP += fpp.position;
        }

        newLocation = newLocation / MillasToybox.fpplayers.Count;
        newLocationFP = newLocationFP / MillasToybox.fpplayers.Count;
        transform.position = newLocation;
        position = newLocationFP;
        
        farthestDistance = 0f;
        var dist = 0f;
        foreach (var fpp in fpplayers)
        {
            dist = Vector2.Distance(position, fpp.position);
            if (dist > farthestDistance)
            {
                farthestDistance = dist;
            }
        }

        var halfHeight = Screen.height / 2f;
        var halfHeightIsh = Screen.height / 2.5f;
        if (farthestDistance <= halfHeightIsh)
        {
            assumedZoom = 1;
        }
        else
        {
            assumedZoom = farthestDistance / halfHeightIsh;
        }

        if (FPCamera.stageCamera != null)
        {
            FPCamera.stageCamera.targetPlayer = null;
            FPCamera.SetCameraTarget(gameObject);
            var zoomField = FPCamera.stageCamera.GetType().GetField("zoom", BindingFlags.Instance | BindingFlags.NonPublic);
            zoomField.SetValue(FPCamera.stageCamera, assumedZoom);
        }
    }

    public static void Spawn()
    {
        var go = new GameObject("RSN Versus Camera Target");
        var target = go.AddComponent<FP2VersusPlayerTarget>();
        target.activationMode = FPActivationMode.ALWAYS_ACTIVE;
        FPCamera.stageCamera.target = target;
    }
}