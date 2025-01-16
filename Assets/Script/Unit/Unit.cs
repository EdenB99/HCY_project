using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData unitData;
    public UnitType unitType;
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
    public Vector2Int currentGridTile; // 현재 타일 그리드 좌표
    

    // 기술 및 시너지
    public string skillName; // 기술 이름

    private void Start()
    {
        /* // 초기화 (HP와 SP 설정)
         currentHP = maxHP;
         currentSP = 0;*/
        unitType = unitData.type;
    }
    private void OnMouseDown()
    {
         SelectionManager.Instance.SelectUnit(this);
    }
    // 강조 표시
    public void Highlight(bool highlight)
    {
        GetComponent<Renderer>().material.color = highlight ? Color.yellow : Color.white;
    }

    // 유닛 이동
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
