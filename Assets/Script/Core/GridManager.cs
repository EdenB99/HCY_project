using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public Transform gridTilesParent; // GridTiles 부모 오브젝트
    public Dictionary<Vector2Int, GridTile> gridTiles = new Dictionary<Vector2Int, GridTile>();
    private void Awake()
    {
        // 싱글톤 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("GridManager가 두 개 이상 존재합니다. 이 스크립트는 하나만 존재해야 합니다.");
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        // GridTiles 부모 오브젝트에서 모든 자식 GridTile 찾기
        foreach (Transform child in gridTilesParent)
        {
            GridTile tile = child.GetComponent<GridTile>();
            if (tile == null)
            {
                Debug.LogWarning($"'{child.name}'에 GridTile 스크립트가 없습니다. 무시됩니다.");
                continue;
            }

            Vector2Int coordinates = tile.gridCoordinates;

            if (gridTiles.ContainsKey(coordinates))
            {
                Debug.LogWarning($"중복된 타일 좌표: {coordinates}. 해당 타일은 무시됩니다.");
                continue;
            }

            gridTiles.Add(coordinates, tile);
        }
    }

    // 특정 좌표의 타일 가져오기
    public GridTile GetTile(Vector2Int coordinates)
    {
        if (gridTiles.TryGetValue(coordinates, out GridTile tile))
        {
            return tile;
        }
        return null;
    }
    public void ResetHighlights()
    {
        foreach (var tile in gridTiles.Values)
        {
            tile.Highlight(false);
        }
    }
}
