using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyEffectHandler : MonoBehaviour
{
    private SynergyManager synergyManager;
    private Coroutine currentPhlemoniaCoroutine;

    private void Start()
    {
        synergyManager = SynergyManager.Instance;
    }

     public void ApplySynergyEffect(SynergyEntry entry)
    {
        switch (entry.synergyData.synergyCode)
        {
            case SynergyCode.S01_Phlemonia:
                RestartPhlemoniaEffect(entry.units, entry.level);
                break;
            default:
                Debug.LogWarning($"Unknown synergy effect: {entry.synergyData.synergyName}");
                break;
        }
    }
    private void RestartPhlemoniaEffect(List<Unit> units, int level)
    {
        if (currentPhlemoniaCoroutine != null)
            StopCoroutine(currentPhlemoniaCoroutine);

        currentPhlemoniaCoroutine = StartCoroutine(ApplyPhlemoniaEffectCoroutine(units, level));
    }
    /// <summary>
    /// Phlemonia 시너지 효과 적용, 적 처치 수에 따라 버프 증가,
    /// 가장 높은 우선순위 유닛에 2배 버프와 지속시간 적용, 20초마다 10초 지속 버프 부여
    /// </summary>
    private static IEnumerator ApplyPhlemoniaEffectCoroutine(List<Unit> units, int level)
    {
        while (true)
        {
            if (level <= 0 || units == null || units.Count == 0) yield break;

            int killCount = 0; // 적 처치 수 초기화
            foreach (Unit unit in units)
                killCount += unit.killCount; // 각 유닛의 처치 수를 합산

            float buffValue = killCount * 0.1f; // 예: 처치 수당 10% 버프

            Unit highestPriorityUnit = FindHighestPriorityUnit(units);

            // 가장 높은 우선순위 유닛에 2배 버프 적용
            if (highestPriorityUnit != null)
            {
                highestPriorityUnit.AddBuff(new UnitBuff("Phlemonia Buff_Atk", "attackPower", buffValue * 2, 20f));
                highestPriorityUnit.AddBuff(new UnitBuff("Phlemonia Buff_Dbt", "durability", buffValue * 2, 20f));
            }

            // 나머지 유닛에 일반 버프 적용
            foreach (var unit in units)
            {
                if (unit != highestPriorityUnit)
                {
                    unit.AddBuff(new UnitBuff("Phlemonia Buff_Atk", "attackPower", buffValue, 10f)); // 10초 지속
                    unit.AddBuff(new UnitBuff("Phlemonia Buff_Dbt", "durability", buffValue, 10f));
                }
            }

            yield return new WaitForSeconds(20f); // 20초 대기
        }
    }


    private static Unit FindHighestPriorityUnit(List<Unit> units)
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

    public static void RemoveEffect(Unit unit, string effectName)
    {
        Debug.Log($"Removing {effectName} from {unit.name}");
        // 효과 제거 로직 추가 가능
    }
}

