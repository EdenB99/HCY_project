using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    // 타일의 기본 데이터
    public Vector2Int gridCoordinates; // 타일의 그리드 좌표
    public bool isOccupied = false; // 타일 점유 여부
    public GameObject occupant = null; // 타일 위의 유닛
    public TileType tileType = TileType.Default; // 타일의 타입
    public bool isSelected = false; // 타일 선택 여부

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

    // 유닛 배치 메서드
    public void PlaceUnit(GameObject unit)
    {
        if (isOccupied)
        {
            Debug.LogWarning($"타일 {gridCoordinates}에 이미 유닛이 배치되어 있습니다.");
            return;
        }

        occupant = unit;
        isOccupied = true;
        Debug.Log($"유닛이 타일 {gridCoordinates}에 배치되었습니다.");
    }

    // 유닛 제거 메서드
    public void RemoveUnit()
    {
        if (!isOccupied)
        {
            Debug.LogWarning($"타일 {gridCoordinates}에 배치된 유닛이 없습니다.");
            return;
        }

        occupant = null;
        isOccupied = false;
        Debug.Log($"타일 {gridCoordinates}에서 유닛이 제거되었습니다.");
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

    // 선택 상태 강조 표시
    public void Highlight(bool highlight)
    {
        if (highlight)
        {
            // 선택 강조 (예: 색상 변경)
            GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log($"타일 {gridCoordinates} 강조 표시");
        }
        else
        {
            // 기본 상태로 복귀
            GetComponent<Renderer>().material.color = Color.white;
            Debug.Log($"타일 {gridCoordinates} 강조 해제");
        }

        isSelected = highlight;
    }
}
