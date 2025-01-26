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

    // 맵 위에 배치된 유닛 리스트
    public List<Unit> placedUnits = new List<Unit>();
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
    /// <summary>
    /// 그리드 초기화
    /// </summary>
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
    /// <summary>
    /// 타일 타입에 따른 메터리얼 가져오기
    /// </summary>
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
    /// <summary>
    /// 유닛 추가 (맵 위에 배치된 경우)
    /// </summary>
    public void AddUnit(Unit unit)
    {
        if (!placedUnits.Contains(unit))
        {
            placedUnits.Add(unit);
        }
    }
    /// <summary>
    /// 유닛 제거 (맵에서 제거된 경우)
    /// </summary>
    public void RemoveUnit(Unit unit)
    {
        if (placedUnits.Contains(unit))
        {
            placedUnits.Remove(unit);
        }
    }
    /// <summary>
    /// 특정 타입의 유닛 리스트 가져오기
    /// </summary>
    public List<Unit> GetUnitsByType(UnitType type)
    {
        return placedUnits.FindAll(unit => unit.unitType == type);
    }
    /// <summary>
    /// 모든 배치된 유닛 리스트 반환
    /// </summary>
    public List<Unit> GetAllUnits()
    {
        return new List<Unit>(placedUnits); // 새로운 리스트로 반환
    }
    /// <summary>
    /// 유닛을 타일로 이동
    /// </summary>
    public void MoveUnitToTile(Unit unit, GridTile targetTile)
    {
        if (unit == null || targetTile == null ||
            !targetTile.PlaceUnit(unit, ShopManager.Instance.shopData.shopLevel)) return;

        GridTile previousTile = GetTile(unit.currentGridTile);
        if (previousTile != null) previousTile.RemoveUnit();

        Vector3 targetPosition = targetTile.transform.position + new Vector3(0, unit.transform.localScale.y / 2, 0);
        unit.MoveToTile(targetTile.gridCoordinates, targetPosition);
    }

    /// <summary>
    /// 유닛과 유닛의 위치 교환
    /// </summary>
    public void SwapUnits(Unit unitA, Unit unitB)
    {
        if (unitA == null || unitB == null) return;

        GridTile tileA = GetTile(unitA.currentGridTile);
        GridTile tileB = GetTile(unitB.currentGridTile);

        if (tileA == null || tileB == null) return;

        tileA.RemoveUnit();
        tileB.RemoveUnit();

        MoveUnitToTile(unitA, tileB);
        MoveUnitToTile(unitB, tileA);
    }

}
