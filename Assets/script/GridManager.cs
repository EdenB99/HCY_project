using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridSize = 7; // 7x7 맵
    public float tileSize = 1.0f; // 타일 크기
    private Dictionary<Vector2Int, GridTile> gridTiles; // 타일 데이터 저장소

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        gridTiles = new Dictionary<Vector2Int, GridTile>();

        // 격자를 중심에 배치
        float offset = (gridSize - 1) * tileSize / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector2Int coordinates = new Vector2Int(x, z);
                GridTile tile = new GridTile(coordinates);

                // 타일의 월드 좌표 계산
                float xPos = x * tileSize - offset;
                float zPos = z * tileSize - offset;

                // 시각적 디버깅용
                Debug.DrawLine(new Vector3(xPos, 0, zPos), new Vector3(xPos, 0.1f, zPos), Color.green, 100f);

                // 타일 저장
                gridTiles[coordinates] = tile;
            }
        }
    }

    // 특정 좌표의 타일 가져오기
    public GridTile GetTile(Vector2Int coordinates)
    {
        if (gridTiles.ContainsKey(coordinates))
        {
            return gridTiles[coordinates];
        }
        return null;
    }

    // 특정 좌표에 유닛 배치
    public void PlaceUnitOnTile(Vector2Int coordinates, GameObject unit)
    {
        GridTile tile = GetTile(coordinates);
        if (tile != null && !tile.isOccupied)
        {
            tile.PlaceUnit(unit);
            // 유닛을 월드 공간에 배치
            unit.transform.position = new Vector3(
                coordinates.x * tileSize - ((gridSize - 1) * tileSize / 2),
                0,
                coordinates.y * tileSize - ((gridSize - 1) * tileSize / 2)
            );
        }
        else
        {
            Debug.Log("타일이 이미 점유되었거나 존재하지 않습니다.");
        }
    }
    void OnDrawGizmos()
{
    if (gridTiles == null) return;

    Gizmos.color = Color.cyan;
    foreach (var tile in gridTiles.Values)
    {
        Vector3 tilePosition = new Vector3(
            tile.coordinates.x * tileSize - ((gridSize - 1) * tileSize / 2),
            0,
            tile.coordinates.y * tileSize - ((gridSize - 1) * tileSize / 2)
        );

        Gizmos.DrawWireCube(tilePosition, new Vector3(tileSize, 0.1f, tileSize));
        if (tile.isOccupied)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(tilePosition, new Vector3(tileSize * 0.8f, 0.1f, tileSize * 0.8f));
        }
    }
}
}
