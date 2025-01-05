using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // 유닛의 주요 능력치
    public int maxHP;
    public int currentHP;
    public int maxSP;
    public int currentSP;
    public int attackPower; // 물리 공격력
    public int magicPower; // 마법 공격력
    public float attackSpeed; // 초당 공격 횟수
    public int range; // 공격 사거리
    public float critChance; // 치명타 확률 (%)
    public float lifesteal; // 피해 흡혈 (%)
    public int durability; // 내구력 (피해 감소율)

    // 유닛의 상태
    public bool isSelected = false;
    public Vector2Int currentTile; // 현재 타일 좌표

    // 기술 및 시너지
    public string skillName; // 기술 이름
    public string[] synergies; // 유닛의 시너지 배열

    private void Start()
    {
        // 초기화 (HP와 SP 설정)
        currentHP = maxHP;
        currentSP = 0;
    }

    // 유닛 선택/해제 처리
    public void SelectUnit()
    {
        if (isSelected)
        {
            DeselectUnit();
        }
        else
        {
            isSelected = true;
            Highlight(true);
            Debug.Log($"{name} 유닛 선택됨.");
        }
    }

    public void DeselectUnit()
    {
        isSelected = false;
        Highlight(false);
        Debug.Log($"{name} 유닛 선택 해제됨.");
    }

    // 강조 표시
    public void Highlight(bool highlight)
    {
        GetComponent<Renderer>().material.color = highlight ? Color.yellow : Color.white;
    }

    // 유닛 이동
    public void MoveToTile(Vector2Int newTileCoordinates)
    {
        currentTile = newTileCoordinates;
        transform.position = new Vector3(newTileCoordinates.x, 0, newTileCoordinates.y);
        Debug.Log($"{name} 유닛이 타일 {newTileCoordinates}로 이동.");
    }

    // 공격 처리
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
    }

    // 유닛 사망 처리
    private void Die()
    {
        Debug.Log($"{name} 유닛이 사망.");
        gameObject.SetActive(false); // 임시로 유닛 비활성화
    }
}