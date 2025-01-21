using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; } // 싱글톤 인스턴스
    //유닛이 타일에 배치될 때 호출할 이벤트
    public Action<Unit> onUnit;
    // 현재 선택된 유닛
    [SerializeField]
    private Unit selectedUnit = null;
    public Unit SelectedUnit
    {
        get => selectedUnit;
        set
        {
            if (selectedUnit != null) selectedUnit.IsSelected = false; // 기존 선택 해제
            selectedUnit = value;
            if (selectedUnit != null) selectedUnit.IsSelected = true;
            UpdateTileMaterials();
        }
    }
    // 현재 선택된 타일
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
            UpdateTileMaterials();
        }
    }
    private void Awake()
    {
        // 싱글톤 구현
        if (Instance == null) Instance = this;
        else
        {
            Debug.LogError("ShopManager가 두 개 이상 존재합니다.");
            Destroy(gameObject);
            return;
        }
    }
    /// <summary>
    /// 유닛을 선택했을 때 발생하는 메소드
    /// </summary>
    /// <param name="unit">선택한 유닛</param>
    public void SelectUnit(Unit unit)
    {
        //현재 유닛이 선택한 유닛인 경우
        if (SelectedUnit == unit)
        {
            Debug.Log($"유닛 {unit.name} 선택 해제");
            SelectedUnit = null;
            return;
        }
        //선택된 유닛이 존재하며 그 유닛이 현재 유닛이 아니면
        if (SelectedUnit != null && SelectedUnit != unit)
        {
            if (SelectedUnit.currentGridTile != null && unit.currentGridTile != null)
            {
                Debug.Log($"유닛 {SelectedUnit.name}과 {unit.name}의 위치를 교환합니다.");
                SwapUnits(SelectedUnit, unit); // 유닛 교환
                DeselectCurrent();
                return;
            }
        }
        //선택된 유닛이 없거나 현재 유닛 혹은 선택된 유닛이 점유한 타일이 없다면
        Debug.Log($"유닛 {unit.name} 선택");
        SelectedUnit = unit;
        if (SelectedTile != null) BothObjectSelect(); // 유닛과 타일이 모두 선택된 상태
    }
    /// <summary>
    /// 타일선택
    /// </summary>
    /// <param name="tile"></param>
    public void SelectTile(GridTile tile)
    {
        if (SelectedTile == tile)
        {
            Debug.Log($"타일 {tile.gridCoordinates} 선택 해제");
            SelectedTile = null;
            return;
        }
        Debug.Log($"타일 {tile.gridCoordinates} 선택");
        SelectedTile = tile;
        if (SelectedUnit != null) BothObjectSelect(); // 유닛과 타일이 모두 선택된 상태
    }
    /// <summary>
    /// 유닛과 타일이 모두 선택된 상태이면 호출해 이동시키는 함수
    /// </summary>
    private void BothObjectSelect()
    {
        Debug.Log($"유닛 {SelectedUnit.name}이 타일 {SelectedTile.gridCoordinates}로 이동시도");
        MoveUnitToTile(SelectedUnit, SelectedTile); // 함수 호출
        DeselectCurrent();
    }
    /// <summary>
    /// 유닛과 유닛 위치 교환
    /// </summary>
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
    /// <summary>
    /// 유닛을 타일로 이동
    /// </summary>
    private void MoveUnitToTile(Unit unit, GridTile targetTile)
    {
        //유닛이 없거나 타일이 없거나 현재 타일이 배치여부가 거부될 경우
        if (unit == null || targetTile == null ||
            !targetTile.PlaceUnit(unit, ShopManager.Instance.shopData.shopLevel)) return;
        // 기존 타일의 유닛 정보 초기화
        GridTile previousTile = GridManager.Instance.GetTile(unit.currentGridTile);
        if (previousTile != null) previousTile.RemoveUnit();

        // 타일로 유닛 이동
        Vector3 targetPosition =
            targetTile.transform.position + new Vector3(0, unit.transform.localScale.y / 2, 0);
        unit.MoveToTile(targetTile.gridCoordinates, targetPosition);
    }

    //선택된 유닛,혹은 타일의 유닛을 제거

    /// <summary>
    /// 선택상태 초기화
    /// </summary>
    private void DeselectCurrent()
    {
        SelectedTile = null;
        SelectedUnit = null;
    }
    /// <summary>
    /// 모든 타일의 메터리얼 업데이트
    /// </summary>
    private void UpdateTileMaterials()
    {
        bool hasSelection = SelectedUnit != null || SelectedTile != null;
        GridManager.Instance.UpdateAllTileMaterials(hasSelection);
    }
}
