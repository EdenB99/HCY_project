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
        OutRange,
        InRange,
        WaveIn,
        WaveOut,
        Special,
    }

    // 타일의 기본 데이터
    public Vector2Int gridCoordinates; // 타일의 그리드 좌표
    public GameObject occupant = null; // 타일 위의 유닛
    public TileType tileType = TileType.Default; // 타일의 타입

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

    // 유닛 배치 메서드
    public bool PlaceUnit(GameObject unit)
    { 
        if (occupant != null)
        {
            Debug.LogWarning($"타일 {gridCoordinates}에 이미 유닛이 배치");
            return false;
        }

        occupant = unit;
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
