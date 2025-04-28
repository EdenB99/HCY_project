using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SynergyType
{
    Region,
    Class,
    Unique
}
[System.Serializable]
public class SynergyEffect
{
    public string effectName; // 효과 이름
    public string statName; // 영향을 받는 스탯 이름
    public float value; // 효과 값
}

public enum SynergyCode
{
    S01_Phlemonia,

}

[CreateAssetMenu(fileName = "SynergyData", menuName = "Synergy/Create New SynergyData")]
public class SynergyDatabase : ScriptableObject
{
    public string synergyName;             // 시너지 이름
    public SynergyCode synergyCode;             // 시너지 코드 

    [TextArea(3, 5)]
    public string description;            // 시너지 설명
    public Sprite icon;                   // 시너지 아이콘
    public List<int> thresholds;    // 활성화 레벨 (예: 2/4/6)
    public List<SynergyEffect> effects;   // 각 레벨별 효과
    public SynergyType synergyType;        // 시너지 타입 (지역, 클래스, 고유)
    private HashSet<Unit> uniqueUnits = new HashSet<Unit>(); // 관련 유닛 관리

    public void AddUnit(Unit unit) => uniqueUnits.Add(unit);
    public void RemoveUnit(Unit unit) => uniqueUnits.Remove(unit);
    public HashSet<Unit> GetUnits() => uniqueUnits;
    public bool ValidateData()
    {
        return thresholds.Count == effects.Count;
    }
}
