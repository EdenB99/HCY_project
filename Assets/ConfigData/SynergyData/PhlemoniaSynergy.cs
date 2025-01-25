using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhlemoniaSynergy : SynergyDatabase
{
    public PhlemoniaSynergy()
    {
        synergyName = "empire_Phlemonia";
        description = "적 처치 시 유닛 강화";
        icon = null; // 아이콘 설정 (필요시)
        activationLevels = new List<int> { 2, 4, 6 }; // 2/4/6 유닛 활성화 시 효과
    }

    public override void ApplyEffect(Unit unit)
    {
        switch (synergyLevel)
        {
            case 1:
                
                break;
            case 2:
                
                break;
            case 3:
                
                break;
        }
        Debug.Log($"{unit.name}에 Phlemonia 시너지 효과 적용 (레벨 {synergyLevel})");
    }
}
