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

    //유닛배치 기능

    /// <summary>
    /// 유닛을 타일로 이동
    /// </summary>
    public void MoveUnitToTile(Unit unit, GridTile targetTile)
    {
        if (unit == null || targetTile == null ||
            !targetTile.CanPlaceUnit(unit.UnitData, ShopManager.Instance.shopData.shopLevel)) return;

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
    /// <summary>
    /// 유닛을 생성해서 배치까지 완료
    /// </summary>
    /// <param name="unitData">생성할 유닛데이터</param>
    /// <param name="spawnTile">배치할 타일</param>
    public void SpawnUnit(UnitData unitData, GridTile spawnTile)
    {
        if (spawnTile == null)
        {
            Debug.LogError("배치할 타일이 없습니다.");
            return;
        }

        // UnitBaseModel 생성
        GameObject unitBase = new GameObject($"Unit_{unitData.unitName}");
        Unit unitComponent = unitBase.GetComponent<Unit>();
        unitComponent.UnitData = unitData;

        // UnitData의 프리팹을 불러와서 자식으로 추가
        if (unitData.unitPrefab != null)
        {
            GameObject model = Instantiate(unitData.unitPrefab, unitBase.transform);
        }

        // 유닛을 해당 타일에 배치
        unitComponent.MoveToTile(spawnTile.gridCoordinates, spawnTile.transform.position);
        spawnTile.PlaceUnit(unitComponent);

        // 유닛을 관리 리스트에 추가
        placedUnits.Add(unitComponent);
        // 유닛을 시너지리스트에 추가
        SynergieManager.Instance.AddUnit(unitComponent);
    }
    //유닛리스트 기능

    /// <summary>
    /// 유닛 제거 (맵에서 제거된 경우)
    /// </summary>
    public void RemoveUnit(Unit unit)
    {
        if (placedUnits.Contains(unit)) placedUnits.Remove(unit);
    }



    // 검색기능

    /// <summary>
    /// 전체 타일 중 해당 유닛이 배치가능한 타일 검색
    /// </summary>
    /// <param name="unit">배치될 유닛</param>
    /// <returns>배치가능한 타일</returns>
    public GridTile GetAvailableTile(UnitData unitData)
    {
        foreach (var tile in gridTiles.Values)
        {
            if (tile.CanPlaceUnit(unitData, ShopManager.Instance.shopData.shopLevel)) return tile;
        }
        return null;
    }

    /// <summary>
    /// 특정 타입의 유닛 리스트 가져오기
    /// </summary>
    public List<Unit> GetUnitsByType(UnitType type)
    {
        return placedUnits.FindAll(unit => unit.UnitData.type == type);
    }

    /// <summary>
    /// 특정 그리드 좌표의 타일 가져오기
    /// </summary>
    /// <param name="coordinates">그리드 좌표</param>
    /// <returns>타일반환</returns>
    public GridTile GetTile(Vector2Int coordinates)
    {
        if (gridTiles.TryGetValue(coordinates, out GridTile tile))
        {
            return tile;
        }
        return null;
    }


    //메터리얼 기능

    /// <summary>
    /// 선택상태에 따른 전체타일 메터리얼 변경
    /// </summary>
    /// <param name="hasSelection">선택여부</param>
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
}
