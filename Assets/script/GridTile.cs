using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile 
{
    public Vector2Int coordinates; // 타일의 그리드 좌표 (x, z)
    public bool isOccupied; // 타일이 점유되었는지 여부
    public GameObject occupant; // 현재 타일에 있는 유닛 (없으면 null)

    public GridTile(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
        this.isOccupied = false;
        this.occupant = null;
    }

    // 유닛 배치
    public void PlaceUnit(GameObject unit)
    {
        isOccupied = true;
        occupant = unit;
    }

    // 유닛 제거
    public void RemoveUnit()
    {
        isOccupied = false;
        occupant = null;
    }
}
