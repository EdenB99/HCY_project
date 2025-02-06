using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
