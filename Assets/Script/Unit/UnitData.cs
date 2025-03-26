using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.CullingGroup;

public enum UnitType
{
    Melee,
    Range,
    Marshal
}
public enum DamageType
{
    Physical,    
    Magical,     
    True,        
    StatusEffect 
}

[CreateAssetMenu(fileName = "UnitData", menuName = "Unit/Create New Unit")]
public class UnitData : ScriptableObject
{
    [Header("Store")]
    public string unitName; // 유닛 이름
    public int costLevel; // 유닛 가격
    public int starLevel; // 유닛 등급
    public Sprite image; // 유닛 이미지
    public GameObject unitPrefab;

    [Header("Synergies")]
    public List<SynergyDatabase> synergyList;

    [Header("Ingame Stats")]
    public UnitType type;
    public int maxHP;
    public int maxSP;
    public int attackPower; // 물리 공격력
    public int focusPower; // 마법 공격력
    public float attackSpeed; // 초당 공격 횟수
    public int range; // 공격 사거리
    public float critChance; // 치명타 확률 (%)
    public float lifesteal; // 피해 흡혈 (%)
    public int durability; // 내구력 (피해 감소율)
    public int stoppingPower; //저지력 (근접유닛만 유효)
}


public struct UnitStats
{
    public UnitType type;
    private int StarLevel;
    public int starLevel
    {
        get => StarLevel;
        set
        {
            StarLevel = value;
            onStarChanged?.Invoke();
        }
    }
    public int maxHP;
    public int currentHP;
    public int maxSP;
    public int currentSP;
    public int attackPower;
    public int focusPower;
    public float attackSpeed;
    public int range;
    public float critChance;
    public float lifesteal;
    public int durability;
    public int stoppingPower;

    private float spRegenRate;
    private float spRegenBuffer;

    public Action onStarChanged;

    public UnitStats(UnitData data)
    {
        type = data.type;
        StarLevel = data.starLevel;
        maxHP = data.maxHP;
        currentHP = data.maxHP;
        maxSP = data.maxSP;
        currentSP = 0;
        attackPower = data.attackPower;
        focusPower = data.focusPower;
        attackSpeed = data.attackSpeed;
        range = data.range;
        critChance = data.critChance;
        lifesteal = data.lifesteal;
        durability = data.durability;
        stoppingPower = data.stoppingPower;

        spRegenRate = Mathf.Round((focusPower / 100f) * 100f) / 100f;
        spRegenBuffer = 0;

        onStarChanged = null;
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

    public void TakeDamage(int damage, DamageType damageType)
    {
        int finalDamage = CalculateDamage(damage, damageType);
        currentHP -= finalDamage;
        currentHP = Mathf.Max(0, currentHP);
    }

    private int CalculateDamage(int baseDamage, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.True:
                return baseDamage;
            case DamageType.StatusEffect:
                return baseDamage;
            default:
                return Mathf.Max(0, baseDamage - durability);
        }
    }
}

