using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private UnitData unitdata;
    public UnitData UnitData {
        get => unitdata;
        set
        {
            unitdata = value;
            InitializeUnit();
        }
    }

    [Header("Runtime Data")]
    public int currentHP;              // 현재 체력
    public int currentSP;              // 현재 스킬 포인트
    public Vector2Int currentGridTile; // 현재 타일 그리드 좌표
    private int starLevel;
    public int StarLevel
    {
        get => starLevel;
        set
        {
            starLevel = value;
            ApplyStarLevelScaling();
        }
    }

    // 유닛의 위치 및 좌표 데이터
    [SerializeField]
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
    private void InitializeUnit()
    {
        if (UnitData == null)
        {
            Debug.LogError("UnitData가 설정되지 않았습니다.");
            return;
        }

        // ScriptableObject 데이터를 기반으로 유닛 초기화
        currentHP = UnitData.maxHP;
        currentSP = 0; // 시작 시 스킬 포인트는 0
    }
    private void OnMouseDown()
    {
         SelectionManager.Instance.SelectUnit(this);
         Debug.Log($"{unitdata.unitName}/{unitdata.starLevel}");
    }
    /// <summary>
    /// 유닛 강조 표시
    /// </summary>
    /// <param name="highlight">선택 여부</param>
    public void Highlight(bool highlight)
    {
        GetComponent<Renderer>().material.color = highlight ? Color.yellow : Color.white;
    }

    /// <summary>
    /// 유닛을 특정 타일로 이동
    /// </summary>
    /// <param name="TileGridPos">그리드 좌표</param>
    /// <param name="TileWorldPos">월드 좌표</param>
    public void MoveToTile(Vector2Int TileGridPos, Vector3 TileWorldPos)
    {
        currentGridTile = TileGridPos;
        transform.position = TileWorldPos;
    }
    /// <summary>
    /// StarLevel이 변경될 때 크기 조정
    /// </summary>
    private void ApplyStarLevelScaling()
    {
        float scaleMultiplier = 1.0f + ((starLevel - UnitData.starLevel) * 0.2f);
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    /// <summary>
    /// 일반공격
    /// </summary>
    /// <param name="target"></param>
    public void nomalAttack(Enemy target)
    {
        if (target == null) return;

        int baseDamage = UnitData.attackPower; // 기본 물리 피해량
        target.TakeDamage(baseDamage, DamageType.Physical);
    }


   

    // 유닛 사망 처리
    private void Die()
    {
        Debug.Log($"{name} 유닛이 사망.");
        gameObject.SetActive(false); // 임시로 유닛 비활성화
    }
}
