using System;
using System.Security.Policy;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox;

public class BindEnemyBaseToPlayerStats : MonoBehaviour
{
    public FPBaseEnemy enemyRef;
    public FPPlayer playerRef;
    public bool forceDisableEnemy = true;

    public void Update()
    {
        enemyRef.health = playerRef.health;
        if (forceDisableEnemy)
        {
            enemyRef.enabled = false;
        }
    }

    public static void Bind(FPPlayer fpp, FPBaseEnemy ene, bool disableEnemy)
    {
        var binding = fpp.gameObject.AddComponent<BindEnemyBaseToPlayerStats>();
        binding.enemyRef = ene;
        binding.playerRef = fpp;
        binding.forceDisableEnemy = disableEnemy;

    }
}