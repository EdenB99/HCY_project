using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    // 열거형(TileType) 정의
    public enum TileType
    {
        Default,
        Melee,
        Outline,
        Range,
        WaveIn,
        WaveOut,
        Special,
    }

    // 타일의 기본 데이터
    
    public TileType tileType = TileType.Default; // 타일의 타입
    public int unlockLevel;   // 타일이 공개되는 레벨
    public Vector2Int gridCoordinates; // 타일의 그리드 좌표
    public GameObject occupant = null; // 타일 위의 유닛




    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Highlight(value);
        }
    }

    
    // 타일 클릭 처리
    private void OnMouseDown()
    {
        if (occupant != null) // 유닛이 존재하면 유닛을 선택
        {
            SelectionManager.Instance.SelectUnit(occupant.GetComponent<Unit>());
        }
        else // 유닛이 없으면 타일을 선택
        {
            SelectionManager.Instance.SelectTile(this);
        }
    }
    // 현재 타일에 유닛을 배치할 수 있는지 확인
    public bool PlaceUnit(Unit unit, int currentLevel)
    {
        // 배치 불가능한 타일이면
        if (tileType == TileType.WaveIn || tileType == TileType.WaveOut)
        {
            Debug.LogWarning($"타일타입 {tileType}은 배치할 수 없습니다.");
            return false;
        }
        // 레벨이 타일레벨보다 부족하면
        if (currentLevel < unlockLevel)
        {
            Debug.LogWarning($"타일 {gridCoordinates}은 현재 레벨 {currentLevel}에서 잠겨 있습니다.");
            return false;
        }
        // 타일 타입에 따라 유닛 배치 가능 여부 확인
        if (tileType == TileType.Melee && unit.unitType != UnitType.Melee)
        {
            Debug.LogWarning("근접 유닛만 이 타일에 배치 가능합니다.");
            return false;
        }
        if (tileType == TileType.Range && unit.unitType != UnitType.Range)
        {
            Debug.LogWarning("원거리 유닛만 이 타일에 배치 가능합니다.");
            return false;
        }

        // 타일이 비어 있는지 확인
        if (occupant != null)
        {
            Debug.LogWarning("타일이 이미 점유되어 있습니다.");
            return false;
        }
        occupant = unit.gameObject;
        return true;
    }


    // 유닛 제거 메서드
    public void RemoveUnit()
    {
        occupant = null;
    }

    // 선택 상태 강조 표시
    public void Highlight(bool highlight)
    {
        GetComponent<Renderer>().material.color = highlight ? Color.yellow : Color.white;
    }
}
