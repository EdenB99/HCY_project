using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPool : MonoBehaviour
{
    public static UnitPool Instance { get; private set; }

    [SerializeField] private GameObject unitPrefab;  // 기본 유닛 프리팹 (예제)
    private Queue<Unit> unitPool = new Queue<Unit>(); // 유닛 풀

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
    /// 오브젝트 풀에서 유닛 가져오기 (없으면 새로 생성)
    /// </summary>
    public Unit GetUnit(UnitData unitData)
    {
        Unit unit;
        if (unitPool.Count > 0)
        {
            unit = unitPool.Dequeue(); // 풀에서 가져오기
            unit.gameObject.SetActive(true);
        }
        else
        {
            GameObject newUnitObj = Instantiate(unitData.unitPrefab); // 새로 생성
            unit = newUnitObj.GetComponent<Unit>();

            if (unit == null)
            {
                unit = newUnitObj.AddComponent<Unit>();
            }
        }

        unit.unitData = unitData;
        return unit;
    }

    /// <summary>
    /// 유닛을 풀에 반환 (비활성화)
    /// </summary>
    public void ReturnUnit(Unit unit)
    {
        unit.gameObject.SetActive(false);
        unitPool.Enqueue(unit); // 다시 큐에 추가
    }
}
