using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyEffectHandler
{
    private SynergyManager synergyManager;

    private void Start()
    {
        synergyManager = SynergyManager.Instance;
    }
    private void Update()
    {

    }
    public static void ApplySynergyEffect(string synergyName, HashSet<Unit> units, int level)
    {
        switch (synergyName)
        {
            case "empire_Phlemonia":
                ApplyPhlemoniaEffect(units, level);
                break;
            default:
                Debug.LogWarning($"Unknown synergy effect: {synergyName}");
                break;
        }
    }

   private static void ApplyPhlemoniaEffect(HashSet<Unit> units, int level)
    {
        if (level <= 0 || units == null || units.Count == 0) return;

        int killCount = 0; // 적 처치 수 초기화
        foreach(Unit unit in units)
                   killCount += unit.killCount; // 각 유닛의 처치 수를 합산
        // 적 처치 수 기반 버프 계산
        float buffValue = killCount * 0.1f; // 예: 처치 수당 10% 버프

        // 가장 높은 우선순위 유닛 찾기
        Unit highestPriorityUnit = FindHighestPriorityUnit(units);

        foreach (var unit in units)
        {
            float finalBuff = (unit == highestPriorityUnit) ? buffValue * 2 : buffValue;

            // 버프 추가
            unit.AddBuff(new UnitBuff("Phlemonia Buff", "attackPower", finalBuff, 10f)); // 10초 지속
            unit.AddBuff(new UnitBuff("Phlemonia Buff", "durability", finalBuff, 10f));
        }
    }

    private static Unit FindHighestPriorityUnit(HashSet<Unit> units)
    {
        Unit highestPriorityUnit = null;
        int highestPriority = int.MinValue;

        foreach (var unit in units)
        {
            int priority = unit.unitData.starLevel; // 우선순위 기준: 성급
            if (priority > highestPriority)
            {
                highestPriority = priority;
                highestPriorityUnit = unit;
            }
        }

        return highestPriorityUnit;
    }

    private static void ApplyBuffToUnit(Unit unit, float buffValue)
    {
        if (unit == null) return;

        unit.stats.attackPower += Mathf.RoundToInt(unit.unitData.attackPower * buffValue);
        unit.stats.durability += Mathf.RoundToInt(unit.unitData.durability * buffValue);
        Debug.Log($"{unit.unitData.unitName}에게 {buffValue * 100}% 버프가 적용되었습니다.");
    }

    public static void RemoveEffect(Unit unit, string effectName)
    {
        Debug.Log($"Removing {effectName} from {unit.name}");
        // 효과 제거 로직 추가 가능
    }
}

