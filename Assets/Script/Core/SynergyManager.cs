using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    private Dictionary<Vector2Int, Unit> tileUnits = new Dictionary<Vector2Int, Unit>(); // 타일별 유닛
    private HashSet<UnitSynergie> activeSynergies = new HashSet<UnitSynergie>(); // 활성화된 시너지
    private int totalUnits = 0; // 배치된 총 유닛 수
}
