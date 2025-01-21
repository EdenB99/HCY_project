using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }  //인스턴스
    [Header("Materials")]
    public Material defaultMaterial;
    public Material LockMaterial;
    public Material meleeMaterial;
    public Material rangeMaterial;
    public Material outlineMaterial;
    public Material WaveMaterial;
    public Material SpecialMaterial;
    public Material lockedMaterial; //잠긴메터리얼
    public Material transparentMaterial; //투명메터리얼

    [Header("References")]
    public Transform gridTilesParent;

    private ShopManager shopManager;
    // 타일별 그리드 좌표
    public Dictionary<Vector2Int, GridTile> gridTiles = new Dictionary<Vector2Int, GridTile>();
    

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
    void Start()
    {
        InitializeGrid();
        shopManager = ShopManager.Instance;
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
    public void UpdateAllTileMaterials(bool hasSelection)
    {
        foreach (var tile in gridTiles.Values)
        {
            if (!hasSelection)
            {
                tile.UpdateMaterial(transparentMaterial); // 선택된 것이 없으면 투명 처리
                continue;
            }
            // 타일 상태에 따른 메터리얼 적용
            bool isUnlocked = shopManager.shopData.shopLevel >= tile.unlockLevel;
            Material materialToApply = GetMaterialForTile(tile.tileType, isUnlocked);
            tile.UpdateMaterial(materialToApply);
        }
    }
    private Material GetMaterialForTile(GridTile.TileType tileType, bool isUnlocked)
    {
        if (!isUnlocked) return lockedMaterial;

        return tileType switch
        {
            GridTile.TileType.Default => defaultMaterial,
            GridTile.TileType.Lock => lockedMaterial,
            GridTile.TileType.Melee => meleeMaterial,
            GridTile.TileType.Range => rangeMaterial,
            GridTile.TileType.Outline => outlineMaterial,
            GridTile.TileType.WaveIn => WaveMaterial,
            GridTile.TileType.WaveOut => WaveMaterial,
            GridTile.TileType.Special => SpecialMaterial,
            _ => defaultMaterial,
        };
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
}
