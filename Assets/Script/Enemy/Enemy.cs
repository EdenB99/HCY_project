using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data Reference")]
    public EnemyData enemyData;
    private EnemyStats stats;

    [Header("Components")]
    private BoxCollider attackRangeCollider;
    private EnemyAnimatorController enemyAC;


    [Header("Movement")]
    public List<Vector2Int> movementPath; // 이동 경로 (그리드 좌표)
    private int currentPathIndex = 0;

    [Header("Runtime Data")]
    private float attackCooldown = 0f;
    private List<Unit> unitsInRange = new List<Unit>();
    private Unit targetUnit = null;
    private bool isAttacking = false;
    private bool isStopped = false;
    public bool IsStopped => isStopped;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isRooted = false;
    private float slowMultiplier = 1.0f;

    public static event Action<Enemy> OnEnemyDied;


    private void Awake()
    {
        attackRangeCollider = GetComponent<BoxCollider>();
        enemyAC = GetComponentInChildren<EnemyAnimatorController>();
    }

    private void Update()
    {
        if (isDead || stats.maxSP == 0) return;

        stats.RecoverSP(Time.deltaTime);
        if (stats.currentSP >= stats.maxSP)
        {
            UseSkill();
            stats.currentSP = 0;
        }
        // 공격 대기 시간 감소
        attackCooldown -= Time.deltaTime;
        attackCooldown = Mathf.Max(0f, attackCooldown);
        if (attackCooldown <= 0f && CanAttack())
        {
            StartCoroutine(Attack());
            attackCooldown = Mathf.Max(1.0f / 3.0f, 1.0f / stats.attackSpeed);
        }
    }
    private void OnEnable()
    {
        if (enemyData != null)
            InitializeEnemy(enemyData, movementPath);
    }


    //---------------------------------------------------------------------------
    //초기화 관련 메소드


    public void InitializeEnemy(EnemyData data, List<Vector2Int> path)
    {
        enemyData = data;
        movementPath = path;
        currentPathIndex = 0;
        isDead = false;
        isAttacking = false;
        isStunned = false;
        isRooted = false;
        slowMultiplier = 1.0f;

        stats = new EnemyStats(enemyData); // 구조체로 데이터 초기화
        AdjustAttackRangeCollider(); // 콜라이더 크기 조정
        if (movementPath != null && movementPath.Count > 0)
            StartCoroutine(MoveAlongPath());
    }

    /// <summary>
    /// BoxCollider의 크기를 stats.Range에 맞게 조정
    /// </summary>
    private void AdjustAttackRangeCollider()
    {
        if (attackRangeCollider != null)
        {
            attackRangeCollider.size = new Vector3(stats.range * 2, 2, stats.range * 2);
            attackRangeCollider.isTrigger = true;
        }
    }



    //---------------------------------------------------------------------------
    //이동 관련 메소드

    /// <summary>
    /// 적 유닛의 이동 루틴
    /// </summary>
    private IEnumerator MoveAlongPath()
    {
        if (movementPath == null || movementPath.Count == 0)
        {
            Debug.LogError($"{enemyData.enemyName}의 이동 경로가 설정되지 않았습니다.");
            yield break;
        }
        while (!isDead && currentPathIndex < movementPath.Count)
        {
            if (currentPathIndex >= movementPath.Count) yield break;

            Vector2Int gridPosition = movementPath[currentPathIndex];
            GridTile targetTile = GridManager.Instance.GetTile(gridPosition);
            if (targetTile == null) yield break;

            Vector3 worldPosition = targetTile.transform.position + new Vector3(0, transform.localScale.y / 2, 0);

            while (Vector3.Distance(transform.position, worldPosition) > 0.1f)
            {
                while (!CanMove())
                    yield return null; // CanMove가 true가 될 때까지 대기
                RotateTowardsTarget(worldPosition);
                transform.position = Vector3.MoveTowards(transform.position, worldPosition,
                (stats.moveSpeed * slowMultiplier) * Time.deltaTime);
                yield return null;
            }

            currentPathIndex++;
            yield return new WaitForSeconds(0.2f);
        }

        if (currentPathIndex >= movementPath.Count)
            ReachEndOfPath();
    }


    /// <summary>
    /// 적 유닛이 목표 지점에 도달했을 때 실행
    /// </summary>
    private void ReachEndOfPath()
    {
        EnemySpawner.Instance.ReturnEnemyToPool(this);
        Debug.Log("유닛통과");
    }

    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        if (enemyAC == null || enemyAC.ACtransfrom == null)
        {
            Debug.LogWarning("EnemyAnimatorController 또는 ACtransfrom이 초기화되지 않았습니다.");
            return;
        }
        Vector3 direction = (targetPosition - enemyAC.ACtransfrom.position).normalized;
        if (direction != Vector3.zero)
        {
            direction.y = 0; // Y축 회전 방지
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            enemyAC.ACtransfrom.rotation = Quaternion.Slerp(enemyAC.ACtransfrom.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    /// <summary>
    /// 이동이 가능한지 확인
    /// </summary>
    private bool CanMove()
    {
        if (isDead || isAttacking || isStopped || isStunned || isRooted)
            return false;
        return true;
    }

    public void StopState(bool isStop)
    {
        isStopped = isStop;
    }


    //---------------------------------------------------------------------------
    //피해 관련 메소드

    /// <summary>
    /// 감지된 유닛을 리스트에 추가
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other is BoxCollider) return;
        Unit unit = other.GetComponent<Unit>();
        if (unit != null && !unitsInRange.Contains(unit))
        {
            unitsInRange.Add(unit);
        }
    }
    /// <summary>
    /// 범위에서 벗어난 유닛을 리스트에서 제거
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other is BoxCollider) return;
        Unit unit = other.GetComponent<Unit>();
        if (unit != null && unitsInRange.Contains(unit))
        {
            unitsInRange.Remove(unit);
        }
    }

    /// <summary>
    /// 범위 내 유닛 중 가장 가까운 유닛 찾기
    /// </summary>
    private void FindNearestUnit()
    {
        float closestDistance = float.MaxValue;
        Unit closestUnit = null;

        foreach (Unit unit in unitsInRange)
        {
            float distance = Vector3.Distance(transform.position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = unit;
            }
        }

        targetUnit = closestUnit;
    }
    /// <summary>
    /// 유닛 공격 루틴
    /// </summary>
    private IEnumerator Attack()
    {
        isAttacking = true;
        // 대상 방향으로 회전
        RotateTowardsTarget(targetUnit.transform.position);
        // 공격 애니메이션 실행
        enemyAC.PlayAttackAnimation();
        isAttacking = false; // 공격 완료
        yield return null;
    }


    /// <summary>
    /// 적 유닛이 피해를 받을 때 호출
    /// </summary>
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (isDead) return;

        int finalDamage = CalculateDamage(damage, damageType);
        stats.currentHP -= finalDamage;

        if (stats.currentHP <= 0)
            Die();
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
                return Mathf.Max(0, baseDamage - enemyData.resistance / 2); // 방어력과 내성치의 평균 적용
            default:
                return baseDamage;
        }
    }



    /// <summary>
    /// 공격이 가능한지 확인
    /// </summary>
    private bool CanAttack()
    {
        // 공격 불가능한 조건
        if (isDead || isStunned || isAttacking || targetUnit == null)
            return false;

        return true; // 위 조건에 해당하지 않으면 공격 가능
    }

    //---------------------------------------------------------------------------
    //상태 관련 메소드

    /// <summary>
    /// 적이 사망하면 실행됨
    /// </summary>
    private void Die()
    {
        isDead = true;
        EnemySpawner.Instance.ReturnEnemyToPool(this);
        OnEnemyDied?.Invoke(this);
    }

    /// <summary>
    /// 기절 상태 적용
    /// </summary>
    public void Stun(float duration)
    {
        if (isStunned) return;

        isStunned = true;

        // 공격 중단
        StartCoroutine(RemoveStun(duration));
    }

    private IEnumerator RemoveStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false; // Stun 상태 해제
    }

    /// <summary>
    /// 속박 상태 적용 (이동 불가, 공격 가능)
    /// </summary>
    public void Root(float duration)
    {
        if (isRooted) return;

        isRooted = true;
        StartCoroutine(RemoveRoot(duration));
    }

    private IEnumerator RemoveRoot(float duration)
    {
        yield return new WaitForSeconds(duration);
        isRooted = false;
    }

    /// <summary>
    /// 이동 속도 감소 적용
    /// </summary>
    public void ApplySlow(float slowAmount, float duration)
    {
        slowMultiplier = Mathf.Clamp(1 - slowAmount, 0.2f, 1.0f); // 최소 20%까지 감속 가능
        StartCoroutine(RemoveSlow(duration));
    }

    private IEnumerator RemoveSlow(float duration)
    {
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1.0f; // 원래 속도로 복구
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
