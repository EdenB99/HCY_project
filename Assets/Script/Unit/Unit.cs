using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Data Reference")]
    public UnitData unitData;

    [Header("Runtime Data")]
    public UnitType unitType;
    public int currentHP;              // 현재 체력
    public int currentSP;              // 현재 스킬 포인트
    public Vector2Int currentGridTile; // 현재 타일 그리드 좌표


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

    private void Start()
    {
        InitializeUnit(); // 유닛 데이터 초기화
    }
    private void InitializeUnit()
    {
        if (unitData == null)
        {
            Debug.LogError("UnitData가 설정되지 않았습니다.");
            return;
        }

        // ScriptableObject 데이터를 기반으로 유닛 초기화
        unitType = unitData.type;
        currentHP = unitData.maxHP;
        currentSP = 0; // 시작 시 스킬 포인트는 0
    }
    private void OnMouseDown()
    {
         SelectionManager.Instance.SelectUnit(this);
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
    /* // 공격 처리
     public void Attack(Unit target)
     {
         int damage = attackPower; // 기본 물리 피해량
         if (Random.value <= critChance / 100f)
         {
             damage = Mathf.RoundToInt(damage * 1.5f); // 치명타
             Debug.Log("치명타 발생!");
         }

         target.TakeDamage(damage);
     }

     // 피해 처리
     public void TakeDamage(int damage)
     {
         int finalDamage = Mathf.Max(0, damage - durability); // 내구력 적용
         currentHP -= finalDamage;

         if (currentHP <= 0)
         {
             Die();
         }
         else
         {
             Debug.Log($"{name} 유닛이 {finalDamage}의 피해를 입음. 남은 HP: {currentHP}/{maxHP}");
         }
     }*/

    // 유닛 사망 처리
    private void Die()
    {
        Debug.Log($"{name} 유닛이 사망.");
        gameObject.SetActive(false); // 임시로 유닛 비활성화
    }
}
