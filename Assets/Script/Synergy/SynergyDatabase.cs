using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SynergyData", menuName = "Synergy/Create New SynergyData")]
public class SynergyDatabase : ScriptableObject
{
    public string synergyName;             // 시너지 이름
    [TextArea (3, 5)]
    public string description;            // 시너지 설명
    public Sprite icon;                   // 시너지 아이콘
    public int synergyLevel;              //현재 활성화된 시너지 레벨
    public List<int> activationLevels;    // 활성화 레벨 (예: 2/4/6)
    private HashSet<Unit> uniqueUnits = new HashSet<Unit>(); // 관련 유닛 관리
}
