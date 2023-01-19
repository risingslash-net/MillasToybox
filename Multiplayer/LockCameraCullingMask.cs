using System;
using System.Reflection;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class LockCameraCullingMask : MonoBehaviour
{
    public int cullingMaskValue;
    public Camera cameraToLock;

    private void Update()
    {
        if (cameraToLock == null)
        {
            var fpCam = gameObject.GetComponent<FPCamera>();
            if (fpCam == null)
            {
                return;
            }

            var uiCamInfo = fpCam.GetType()
                .GetField("uiCam", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Log(uiCamInfo);
            var uiCam = (Camera)uiCamInfo.GetValue(fpCam);
            Debug.Log(uiCam);
            if (uiCam == null)
            {
                return;
            }

            cameraToLock = uiCam;
            //uiCam.cullingMask = cullingMaskValue;

            //cameraToLock = gameObject.GetComponent<Camera>();
        }
        else
        {
            cameraToLock.cullingMask = cullingMaskValue;
            Debug.Log("Culling Mask Set to: " + cameraToLock.cullingMask + " - " + gameObject.name);
            Destroy(this);
        }
    }
}