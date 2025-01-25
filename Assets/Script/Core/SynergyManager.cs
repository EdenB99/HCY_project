using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SynergieManager : MonoBehaviour
{
    public static SynergieManager Instance { get; private set; }

    [Header("Synergy Database")]
    public List<SynergyDatabase> synergies = new List<SynergyDatabase>();

    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    /// <summary>
    /// 유닛이 추가되었을 때 관련된 시너지 업데이트
    /// </summary>
    public void AddUnitToSynergies(Unit unit)
    {
        foreach (var synergy in unit.unitData.synergyList)
        {
            synergy.AddUnit(unit);
        }
    }
    /// <summary>
    /// 유닛이 제거되었을 때 관련된 시너지 업데이트
    /// </summary>
    public void RemoveUnitFromSynergies(Unit unit)
    {
        foreach (var synergy in unit.unitData.synergyList)
        {
            synergy.RemoveUnit(unit);
        }
    }
}
