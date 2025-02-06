using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    [Header("Enemy Data Reference")]
    public EnemyData enemyData;

    [Header("Runtime Stats")]
    private int currentHP;
    private int currentSP;
    private bool isStunned = false;

    [Header("Movement")]
    public List<Vector2Int> movementPath; // 이동 경로 (그리드 좌표)
    private int currentPathIndex = 0;
    private float moveSpeed;


    private void Start()
    {
        InitializeEnemy();
        StartCoroutine(MoveAlongPath());
        if (movementPath != null && movementPath.Count > 0) // 이동 경로가 있을 경우에만 실행
        {
            StartCoroutine(MoveAlongPath());
        }
    }

    private void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData가 설정되지 않았습니다.");
            return;
        }

        currentHP = enemyData.maxHP;
        currentSP = 0;
        moveSpeed = enemyData.moveSpeed;
    }

    /// <summary>
    /// 적 유닛의 이동 루틴
    /// </summary>
    private IEnumerator MoveAlongPath()
    {
        while (!isStunned && currentPathIndex < movementPath.Count)
        {
            Vector2Int gridPosition = movementPath[currentPathIndex];
            Vector3 worldPosition = GridManager.Instance.GetTile(gridPosition).transform.position;

            while (Vector3.Distance(transform.position, worldPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, worldPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            currentPathIndex++;
            yield return new WaitForSeconds(0.2f); // 이동 간 딜레이
        }

        if (currentPathIndex >= movementPath.Count)
        {
            ReachEndOfPath(); // 목표 지점 도달 시 호출
        }
    }

    /// <summary>
    /// 적 유닛이 목표 지점에 도달했을 때 실행
    /// </summary>
    private void ReachEndOfPath()
    {
        Debug.Log($"{enemyData.enemyName}이(가) 목표 지점에 도착!");
        EnemyWaveManager.Instance.ReturnEnemyToPool(this);
    }


    /// <summary>
    /// 적 유닛이 피해를 받을 때 호출
    /// </summary>
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (isStunned) return; // 기절 상태에서는 피해를 받지 않음

        int finalDamage = CalculateDamage(damage, damageType);
        currentHP -= finalDamage;

        Debug.Log($"{enemyData.enemyName}이(가) {finalDamage} 피해를 받음. 남은 HP: {currentHP}/{enemyData.maxHP}");

        if (currentHP <= 0)
        {
            StunUnit(); // 기절 처리
        }
    }
    /// <summary>
    /// 피해 타입에 따라 실제 적용되는 피해량 계산
    /// </summary>
    private int CalculateDamage(int baseDamage, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return Mathf.Max(0, baseDamage - enemyData.defense); // 방어력 적용

            case DamageType.Magical:
                return Mathf.Max(0, baseDamage - enemyData.resistance); // 내성치 적용

            case DamageType.True:
                return baseDamage; // 고정 피해는 그대로 적용

            case DamageType.StatusEffect:
                return Mathf.Max(0, baseDamage - enemyData.resistance/2); // 방어력과 내성치의 평균 적용
            default:
                return baseDamage;
        }
    }
    /// <summary>
    /// 적이 기절 상태가 되면 실행됨
    /// </summary>
    private void StunUnit()
    {
        isStunned = true;
        Debug.Log($"{enemyData.enemyName}이(가) 기절함!");
        EnemyWaveManager.Instance.ReturnEnemyToPool(this);
    }

    /// <summary>
    /// SP를 초당 회복 (기술 사용 가능할 경우)
    /// </summary>
    private void Update()
    {
        if (isStunned || enemyData.maxSP == 0) return;

        currentSP += Mathf.FloorToInt(1 + enemyData.focusPower * Time.deltaTime);

        if (currentSP >= enemyData.maxSP)
        {
            UseSkill();
            currentSP = 0;
        }
    }

    /// <summary>
    /// 적이 보유한 기술 사용
    /// </summary>
    private void UseSkill()
    {
        Debug.Log($"{enemyData.enemyName}이(가) {enemyData.skillName}을 사용!");
        // 실제 스킬 실행 코드 필요
    }
}
