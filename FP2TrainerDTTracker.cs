using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    public class FP2TrainerDTTracker : MonoBehaviour
    {
        public float dt;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LateUpdate()
        {
            dt = FPStage.frameTime;
            MillasToybox.SetFP2TDeltaTime(dt);
            MillasToybox.millasToyboxInstance.OnGameObjectUpdate();
        }
    }
}