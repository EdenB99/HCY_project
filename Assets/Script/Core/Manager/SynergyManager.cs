using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SynergyEntry
{
    public SynergyDatabase synergyData; // 시너지 데이터
    public List<Unit> units;         // 해당 시너지에 속한 유닛들
    public int level;               // 시너지 레벨
    public SynergyEntry(SynergyDatabase synergyData)
    {
        this.synergyData = synergyData;
        this.units = new List<Unit>();
        this.level = 0;
    }
}
public class SynergyManager : MonoBehaviour
{
    [Header("Components")]
    public GameObject synergyPanel;
    public Transform scrollContent;
    public static SynergyManager Instance { get; private set; }

    private SynergyEffectHandler synergyEffectHandler; // 시너지 효과 핸들러
    [Header("Synergy Database")]
    public List<SynergyEntry> synergyEntries = new List<SynergyEntry>(); // 시너지 데이터와 유닛 관리

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        synergyEffectHandler = GetComponent<SynergyEffectHandler>();
        ClearSynergyPanel();
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

            if (isAdding)
                entry.units.Add(unit);
            else
                entry.units.Remove(unit);
            UpdateSynergyLevel(entry);
        }
    }

    private void UpdateSynergyLevel(SynergyEntry entry)
    {
        var synergy = entry.synergyData;
        var units = entry.units;

        entry.level = CalculateSynergyLevel(units.Count, synergy.thresholds);
        synergyEffectHandler.ApplySynergyEffect(entry);
        UpdateSynergyPanel();
    }

    /// <summary>
    /// Synergy 레벨 계산, 현재 유닛 수에 따라 레벨 결정, 중복 유닛 허용중, 수정필요
    /// </summary>
    /// <param name="unitCount"></param>
    /// <param name="thresholds"></param>
    /// <returns></returns>
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

    public void UpdateSynergyPanel()
    {
        foreach (var entry in synergyEntries)
        {
            var units = entry.units;
            if (entry.units.Count != 0) // Synergy 레벨이 0이 아닐 때만 패널 업데이트
            {
                SynergyPanel existingPanel = null;
                foreach (Transform child in scrollContent)
                {
                    var panelScript = child.GetComponent<SynergyPanel>();
                    if (panelScript != null && panelScript.synergyData == entry)
                    {
                        existingPanel = panelScript;
                        break;
                    }
                }
                if (existingPanel != null) // 기존 패널이 있는 경우
                    existingPanel.SetSynergyData(entry, entry.level);
                else
                {
                    // 새 패널 생성
                    var synergyPanelInstance = Instantiate(synergyPanel, scrollContent);
                    SynergyPanel synergyPanelScript = synergyPanelInstance.GetComponent<SynergyPanel>();
                    synergyPanelScript.SetSynergyData(entry, entry.level);
                }
            }
            else
            {
                // Synergy 레벨이 0인 경우 패널 제거
                foreach (Transform child in scrollContent)
                {
                    var panelScript = child.GetComponent<SynergyPanel>();
                    if (panelScript != null && panelScript.synergyData == entry)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }
            }
        }
    }
    public void ClearSynergyPanel()
    {
        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);
    }
    public void AddSynergyPanel(SynergyEntry entry)
    {

    }
}