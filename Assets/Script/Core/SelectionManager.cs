using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SelectionManager Instance { get; private set; }
    //유닛이 타일에 배치될 때 호출할 이벤트
    public Action<Unit> onUnit;

    // 현재 선택된 유닛과 타일
    [SerializeField]
    private Unit selectedUnit = null;
    public Unit SelectedUnit
    {
        get => selectedUnit;
        set
        {
            if (selectedUnit != null)
            {
                selectedUnit.IsSelected = false; // 기존 선택 해제
            }
            selectedUnit = value;
            if (selectedUnit != null) selectedUnit.IsSelected = true;
        }
    }
    [SerializeField]
    private GridTile selectedTile = null;
    public GridTile SelectedTile
    {
        get => selectedTile;
        set
        {
            if (selectedTile != null) selectedTile.IsSelected = false; // 기존 선택 해제
            selectedTile = value;
            if (selectedTile != null) selectedTile.IsSelected = true;
        }
    }

    private void Awake()
    {
        // 싱글톤 구현
        if (Instance == null) Instance = this;
        else
        {
            Debug.LogError("SelectionManager가 두 개 이상 존재합니다.");
            Destroy(gameObject);
            return;
        }
    }
    //유닛 선택
    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit == unit)
        {
            Debug.Log($"유닛 {unit.name} 선택 해제");
            SelectedUnit = null;

        }
        else if (SelectedUnit != null && SelectedUnit != unit)
        {
            if (SelectedUnit.currentGridTile != null && unit.currentGridTile != null)
            {
                Debug.Log($"유닛 {SelectedUnit.name}과 {unit.name}의 위치를 교환합니다.");
                SwapUnits(SelectedUnit, unit); // 두 유닛의 위치를 교환
                DeselectCurrent(); // 선택 상태 초기화
            }
            else {
                Debug.Log($"유닛 {unit.name} 선택");
                SelectedUnit = unit;
                if (SelectedTile != null) BothObjectSelect();
            }
        }
        else
        {
            Debug.Log($"유닛 {unit.name} 선택");
            SelectedUnit = unit;
            if (SelectedTile != null) BothObjectSelect();
        }
    }


    //타일 선택
    public void SelectTile(GridTile tile)
    {
        if (SelectedTile == tile)
        {
            Debug.Log($"타일 {tile.gridCoordinates} 선택 해제");
            SelectedTile = null;
        } else
        {
            Debug.Log($"타일 {tile.gridCoordinates} 선택");
            SelectedTile = tile;
            if (SelectedUnit != null) BothObjectSelect();
        }
    }


    //타일과 유닛 선택시
    private void BothObjectSelect()
    {
        Debug.Log($"유닛 {SelectedUnit.name}이 타일 {SelectedTile.gridCoordinates}로 이동시도");
        MoveUnitToTile(SelectedUnit, SelectedTile); // 함수 호출
        DeselectCurrent();

    }

    //유닛과 유닛 선택시
    public void SwapUnits(Unit unitA, Unit unitB)
    {
        if (unitA == null || unitB == null)
        {
            Debug.LogWarning("교환하려는 유닛이 없음");
            return;
        }

        GridTile tileA = GridManager.Instance.GetTile(unitA.currentGridTile);
        GridTile tileB = GridManager.Instance.GetTile(unitB.currentGridTile);

        tileA.RemoveUnit();
        tileB.RemoveUnit();

        if (tileA == null || tileB == null)
        {
            Debug.LogWarning("유닛이 배치된 타일이 없음");
            return;
        }

        // 위치 교환
        MoveUnitToTile(unitA, tileB);
        MoveUnitToTile(unitB, tileA);

        Debug.Log($"유닛 {unitA.name}와 유닛 {unitB.name}의 위치를 교환");
    }


    //선택 상태 초기화
    private void DeselectCurrent()
    {
        SelectedTile = null;
        SelectedUnit = null;
    }

    //유닛을 해당 타일로 이동
    private void MoveUnitToTile(Unit unit, GridTile targetTile)
    {
        // 
        if (unit == null || targetTile == null || !targetTile.PlaceUnit(unit, 6)) return;
        // 기존 타일의 유닛 정보 초기화
        GridTile previousTile = GridManager.Instance.GetTile(unit.currentGridTile);
        if (previousTile != null)
        {
            previousTile.RemoveUnit();
        }

        // 타일로 유닛 이동
        Vector3 targetPosition = targetTile.transform.position + new Vector3(0, unit.transform.localScale.y / 2, 0);
        unit.MoveToTile(targetTile.gridCoordinates, targetPosition);
    }

    //선택된 유닛,혹은 타일의 유닛을 제거

}
