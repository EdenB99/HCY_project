using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridTile : MonoBehaviour
{
    // 열거형(TileType) 정의
    public enum TileType
    {
        Default,
        Lock,
        Melee,
        Range,
        Outline,
        WaveIn,
        WaveOut,
        Special,
    }


    [Header("TileData")]
    public TileType tileType = TileType.Default; // 타일의 타입
    public int unlockLevel;   // 타일이 공개되는 레벨
    public Vector2Int gridCoordinates; // 타일의 그리드 좌표
    public Unit occupant = null; // 타일 위의 유닛

    private Renderer tileRenderer;

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
        }
    }
    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
    }
    /// <summary>
    /// 타일 업데이트
    /// </summary>
    /// <param name="material">변경될 메터리얼</param>
    public void UpdateMaterial(Material material)
    {
        if (tileRenderer != null && material != null)
        {
            tileRenderer.material = material;
        }
    }
    /// <summary>
    /// 타일 클릭 처리
    /// </summary>
    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        if (occupant != null) // 유닛이 존재하면 유닛을 선택
        {
            SelectionManager.Instance.SelectUnit(occupant);
        }
        else // 유닛이 없으면 타일을 선택
        {
            SelectionManager.Instance.SelectTile(this);
        }
    }
    /// <summary>
    /// 타일에 해당 유닛이 배치될 수 있는지 확인
    /// </summary>
    /// <param name="unit">배치될 유닛</param>
    /// <param name="currentLevel">현재 상점레벨</param>
    /// <returns></returns>

    public bool CanPlaceUnit(UnitData unitdata, int currentLevel)
    {
        // 배치 불가능한 타일이면
        if (tileType == TileType.WaveIn || tileType == TileType.WaveOut
            || tileType == TileType.Lock)
        {
            return false;
        }

        // 레벨이 타일레벨보다 부족하면
        if (currentLevel < unlockLevel)
        {
            return false;
        }
        // 타일이 비어 있는지 확인
        if (occupant != null)
        {
            return false;
        }
        // 타일 타입에 따라 유닛 배치 가능 여부 확인
        if (tileType == TileType.Melee && unitdata.type == UnitType.Range)
        {
            return false;
        }
        if (tileType == TileType.Range && unitdata.type == UnitType.Melee)
        {
            return false;
        }
        return true;
    }
    public void PlaceUnit(Unit unit)
    {
        occupant = unit;
    }
    /// <summary>
    /// 유닛제거
    /// </summary>
    public void RemoveUnit()
    {
        occupant = null;
    }

}
