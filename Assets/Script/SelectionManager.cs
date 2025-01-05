using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SelectionManager Instance { get; private set; }

    // 현재 선택된 유닛과 타일
    public Unit selectedUnit = null;
    public GridTile selectedTile = null;

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

    // 유닛 선택
    public void SelectUnit(Unit unit)
    {
        // 이미 선택된 유닛이 있을 경우 선택 해제
        if (selectedUnit == unit)
        {
            Debug.Log($"유닛 {unit.name} 선택 해제");
            DeselectUnit();
            return;
        }

        // 이전 선택 상태 초기화
        if (selectedUnit != null)
        {
            DeselectUnit();
        }

        // 새로운 유닛 선택
        selectedUnit = unit;
        Debug.Log($"유닛 {unit.name} 선택됨");
        selectedUnit.Highlight(true);
    }

    // 타일 선택
    public void SelectTile(GridTile tile)
    {
        // 이미 선택된 타일이 있을 경우 선택 해제
        if (selectedTile == tile)
        {
            Debug.Log($"타일 {tile.gridCoordinates} 선택 해제");
            DeselectTile();
            return;
        }

        // 이전 선택 상태 초기화
        if (selectedTile != null)
        {
            DeselectTile();
        }

        // 새로운 타일 선택
        selectedTile = tile;
        Debug.Log($"타일 {tile.gridCoordinates} 선택됨");
        selectedTile.Highlight(true);
    }

    // 유닛 선택 해제
    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.Highlight(false);
            selectedUnit = null;
        }
    }

    // 타일 선택 해제
    public void DeselectTile()
    {
        if (selectedTile != null)
        {
            selectedTile.Highlight(false);
            selectedTile = null;
        }
    }

    // 타일 클릭 시 유닛 이동 처리
    public void HandleTileClick(GridTile tile)
    {
        if (selectedUnit == null)
        {
            // 선택된 유닛이 없으면 타일 선택
            SelectTile(tile);
        }
        else
        {
            // 선택된 유닛이 있으면 유닛을 타일로 이동
            if (!tile.isOccupied)
            {
                Debug.Log($"유닛 {selectedUnit.name}이 타일 {tile.gridCoordinates}로 이동");
                tile.PlaceUnit(selectedUnit.gameObject); // 유닛 배치
                selectedUnit.MoveToTile(tile.gridCoordinates); // 유닛 이동
                DeselectUnit();
                DeselectTile();
            }
            else
            {
                Debug.LogWarning($"타일 {tile.gridCoordinates}는 이미 점유됨. 이동 불가");
            }
        }
    }
}
