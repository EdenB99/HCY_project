using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Create New Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName;         // 적 유닛 이름
    public int maxHP;                // 최대 HP
    public int maxSP;                // 최대 SP (일반 적은 0)
    public int attackPower;          // 전투력 (물리 공격력)
    public int focusPower;           // 집중력 (마법 공격력)
    public int defense;              // 방어력 (물리 피해 감소)
    public int resistance;           // 내성치 (마법 피해 감소)

    [Header("Combat Stats")]
    public float attackSpeed;        // 초당 공격 횟수
    public int range;                // 공격 사거리
    public float moveSpeed;          // 이동 속도
    public float critChance;         // 치명타 확률 (%)
    public float lifesteal;          // 피해 흡혈 (%)

    [Header("Skill Settings")]
    public string skillName;         // 적의 기술 이름
    public GameObject enemyPrefab;   // 해당 적 유닛의 프리팹
}
public struct EnemyStats
{
    public int maxHP;
    public int currentHP;
    public int maxSP;
    public int currentSP;
    public int attackPower;
    public int focusPower;
    public int defense;
    public int resistance;
    public float attackSpeed;       
    public int range;              
    public float moveSpeed;         
    public float critChance;       
    public float lifesteal;

    private float spRegenRate;  
    private float spRegenBuffer;

    public EnemyStats(EnemyData data)
    {
        maxHP = data.maxHP;
        currentHP = data.maxHP;
        maxSP = data.maxSP;
        currentSP = 0;
        attackPower = data.attackPower;
        focusPower = data.focusPower;
        defense = data.defense;
        resistance = data.resistance;
        attackSpeed = data.attackSpeed;
        range = data.range;
        moveSpeed = data.moveSpeed;
        critChance = data.critChance;
        lifesteal = data.lifesteal;

        spRegenRate = Mathf.RoundToInt(focusPower / 100f);
        spRegenBuffer = 0;
    }

    public void RecoverSP(float deltaTime)
    {
        spRegenBuffer += spRegenRate * deltaTime;
        int spGain = Mathf.FloorToInt(spRegenBuffer);
        if (spGain > 0)
        {
            currentSP += spGain;
            spRegenBuffer -= spGain;
            currentSP = Mathf.Min(currentSP, maxSP);
        }
    }
}
