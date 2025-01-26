using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SynergieManager : MonoBehaviour
{
    public static SynergieManager Instance { get; private set; }

    [Header("Synergy Database")]
    public List<SynergyDatabase> synergies = new List<SynergyDatabase>();

    // 해당 시너지 소속 유닛들을 모은 딕셔너리
    [SerializeField]
    private Dictionary<string, HashSet<Unit>> synergyUnits =
        new Dictionary<string, HashSet<Unit>>();
    // 해당 시너지의 레벨을 기록한 딕셔너리
    private Dictionary<string, int> synergyLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        //synergies의 시너지들에 
        
    }
    /// <summary>
    /// SynergyManager 초기화
    /// </summary>
    /// <param name="synergyList">GameConfig에서 받은 SynergyDatabase 리스트</param>
    public void InitializeSynergies(List<SynergyDatabase> synergyList)
    {
        synergies = synergyList;
        foreach (var synergy in synergies)
        {
            synergyUnits[synergy.synergyName] = new HashSet<Unit>();
            synergyLevels[synergy.synergyName] = 0;
        }
    }
    /// <summary>
    /// 유닛을 시너지에 추가
    /// </summary>
    public void AddUnit(Unit unit)
    {
        foreach (var synergy in unit.unitData.synergyList) //유닛이 가진 시너지 리스트 순회
        {
            if (!synergyUnits.ContainsKey(synergy.synergyName)) continue;

            HashSet<Unit> units = synergyUnits[synergy.synergyName];
            if (units.Add(unit)) // 유닛 추가 성공 시
            {
                UpdateSynergyLevel(synergy.synergyName);
            }
        }
    }
    /// <summary>
    /// 유닛을 기반으로 시너지에서 제거
    /// </summary>
    public void RemoveUnit(Unit unit)
    {
        foreach (var synergy in unit.unitData.synergyList) //유닛이 가진 시너지 리스트 순회
        {
            if (!synergyUnits.ContainsKey(synergy.synergyName)) continue;

            HashSet<Unit> units = synergyUnits[synergy.synergyName];
            if (units.Remove(unit)) // 유닛 제거 성공 시
            {
                UpdateSynergyLevel(synergy.synergyName);
            }
        }
    }
    /// <summary>
    /// 시너지 레벨 계산 및 효과 적용
    /// </summary>
    private void UpdateSynergyLevel(string synergyName)
    {
        //해당 시너지가 시너지그룹에 있는지 확인
        var synergy = synergies.Find(s => s.synergyName == synergyName);
        if (synergy == null) return;

        //해당 시너지에 속한 유닛그룹 검색 후 시너지 레벨 조정
        HashSet<Unit> units = synergyUnits[synergyName];
        int level = CalculateSynergyLevel(units.Count, synergy.activationLevels);
        synergyLevels[synergyName] = level;

        // 효과 적용
        ApplySynergyEffectToAll(synergyName, level);
    }
    /// <summary>
    /// 시너지 레벨 계산
    /// </summary>
    private int CalculateSynergyLevel(int unitCount, List<int> activationLevels)
    {
        int level = 0;
        for (int i = 0; i < activationLevels.Count; i++)
        {
            if (unitCount >= activationLevels[i])
                level = i + 1;
        }
        return level;
    }
    /// <summary>
    /// 모든 유닛에 효과 적용
    /// </summary>
    private void ApplySynergyEffectToAll(string synergyName, int level)
    {
        var units = synergyUnits[synergyName];
        foreach (var unit in units)
        {
            SynergyEffectHandler.ApplyEffect(unit, synergyName, level);
        }
    }
}
