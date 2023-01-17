using System;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class LockRotScale : MonoBehaviour
{
    public Quaternion lockQuaternion;
    public Vector3 lockScale;
    public bool noFlip = true;

    private void Update()
    {
        if (lockQuaternion != null)
        {
            transform.rotation = lockQuaternion;
        }
        if (lockScale != null)
        {
            transform.localScale = lockScale;
            
        }
        
        if (noFlip)
        {
            if (transform.lossyScale.x < 0)
            {
                transform.localScale = new Vector3(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z);
            }
        }
    }
}