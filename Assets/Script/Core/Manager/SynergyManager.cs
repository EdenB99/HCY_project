using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SynergyEntry
{
    public SynergyDatabase synergyData; // 시너지 데이터
    public HashSet<Unit> units;         // 해당 시너지에 속한 유닛들

    public SynergyEntry(SynergyDatabase synergyData)
    {
        this.synergyData = synergyData;
        this.units = new HashSet<Unit>();
    }
}
public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    [Header("Synergy Database")]
    public List<SynergyEntry> synergyEntries = new List<SynergyEntry>(); // 시너지 데이터와 유닛 관리

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// SynergyManager 초기화
    /// </summary>
    /// <param name="synergyList">GameConfig에서 받은 SynergyDatabase 리스트</param>
    public void InitializeSynergies(List<SynergyDatabase> synergyList)
    {
        synergyEntries.Clear();
        foreach (var synergy in synergyList)
            synergyEntries.Add(new SynergyEntry(synergy));

    }

    public void AddUnit(Unit unit) => UpdateUnitInSynergy(unit, true);
    public void RemoveUnit(Unit unit) => UpdateUnitInSynergy(unit, false);

    private void UpdateUnitInSynergy(Unit unit, bool isAdding)
    {
        foreach (var synergy in unit.unitData.synergyList)
        {
            var entry = synergyEntries.Find(e => e.synergyData.synergyName == synergy.synergyName);
            if (entry == null) continue;

            if (isAdding ? entry.units.Add(unit) : entry.units.Remove(unit))
            {
                UpdateSynergyLevel(entry);
            }
        }
    }

    private void UpdateSynergyLevel(SynergyEntry entry)
    {
        var synergy = entry.synergyData;
        var units = entry.units;

        int level = CalculateSynergyLevel(units.Count, synergy.thresholds);
        SynergyEffectHandler.ApplySynergyEffect(synergy.synergyName, units, level);
    }

    private int CalculateSynergyLevel(int unitCount, List<int> thresholds)
    {
        int level = 0;
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (unitCount >= thresholds[i])
                level = i + 1;
        }
        return level;
    }
}