using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Unit/Create New Unit")]
public class UnitData : ScriptableObject
{
    [Header("Store")]
    public string unitName; // 유닛 이름
    public int costLevel; // 유닛 가격
    public int starLevel; // 유닛 등급
    public Sprite image; // 유닛 이미지
    public List<UnitSynergie> synergies; // 유닛 시너지 리스트

    [Header("Ingame Stats")]
    public UnitType type;
    public int maxHP;
    public int currentHP;
    public int maxSP;
    public int currentSP;
    public int attackPower; // 물리 공격력
    public int magicPower; // 마법 공격력
    public float attackSpeed; // 초당 공격 횟수
    public int range; // 공격 사거리
    public float critChance; // 치명타 확률 (%)
    public float lifesteal; // 피해 흡혈 (%)
    public int durability; // 내구력 (피해 감소율)
}
