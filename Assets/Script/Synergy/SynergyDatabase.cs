using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SynergyDatabase
{
    public string synergyName;             // 시너지 이름
    public string description;            // 시너지 설명
    public Sprite icon;                   // 시너지 아이콘
    public int synergyLevel;              //현재 활성화된 시너지 레벨
    public List<int> activationLevels;    // 활성화 레벨 (예: 2/4/6)
    private HashSet<Unit> uniqueUnits = new HashSet<Unit>(); // 관련 유닛 관리

    /// <summary>
    /// 유닛을 추가하고 레벨을 갱신
    /// </summary>
    public void AddUnit(Unit unit)
    {
        if (uniqueUnits.Add(unit)) // 유닛 추가 성공 시
        {
            synergyLevel = CalculateSynergyLevel();
            ApplyEffectToAll();
        }
    }

    /// <summary>
    /// 유닛을 제거하고 레벨을 갱신
    /// </summary>
    public void RemoveUnit(Unit unit)
    {
        if (uniqueUnits.Remove(unit)) // 유닛 제거 성공 시
        {
            synergyLevel = CalculateSynergyLevel();
            ApplyEffectToAll();
        }
    }

    /// <summary>
    /// 현재 유닛 개수를 기반으로 활성화 레벨 계산
    /// </summary>
    private int CalculateSynergyLevel()
    {
        int unitCount = uniqueUnits.Count;
        int level = 0;
        for (int i = 0; i < activationLevels.Count; i++)
        {
            if (unitCount >= activationLevels[i])
            {
                level = i + 1;
            }
        }
        return level;
    }

    /// <summary>
    /// 모든 관련 유닛에 효과를 적용
    /// </summary>
    private void ApplyEffectToAll()
    {
        foreach (var unit in uniqueUnits)
        {
            ApplyEffect(unit);
        }
    }

    /// <summary>
    /// 활성화된 시너지 효과를 유닛에 적용
    /// </summary>
    public abstract void ApplyEffect(Unit unit);
}
