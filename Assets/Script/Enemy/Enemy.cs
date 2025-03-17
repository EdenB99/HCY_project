using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data Reference")]
    public EnemyData enemyData;
    private EnemyStats stats;
    public EnemyStats Stats
    {
        get => stats;
        set
        {
            stats = value;
            UpdateStats();
        }
    }

    private bool isDead = false;

    [Header("Movement")]
    public List<Vector2Int> movementPath; // 이동 경로 (그리드 좌표)
    private int currentPathIndex = 0;
    
    [Header("Status Effects")]
    private bool isStunned = false; // 기절 여부
    private bool isRooted = false;  // 속박 여부
    private float slowMultiplier = 1.0f;


    /// <summary>
    /// SP를 초당 회복 (기술 사용 가능할 경우)
    /// </summary>
    private void Update()
    {
        if (isDead || stats.maxSP == 0) return;

        stats.RecoverSP(Time.deltaTime);
        if (stats.currentSP >= stats.maxSP)
        {
            UseSkill();
            stats.currentSP = 0;
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
        isStunned = false;
        isRooted = false;
        slowMultiplier = 1.0f;

        stats = new EnemyStats(enemyData); // 구조체로 데이터 초기화

        if (movementPath != null && movementPath.Count > 0)
            StartCoroutine(MoveAlongPath());
    }
    /// <summary>
    /// EnemyStats가 변경될 때 자동으로 호출
    /// </summary>
    private void UpdateStats()
    {
        if (isDead) return;
        Debug.Log($"{enemyData.enemyName}의 스탯이 업데이트됨!");
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

        while (!isDead && !isStunned && !isRooted && currentPathIndex < movementPath.Count)
        {
            if (currentPathIndex >= movementPath.Count) yield break;
            Vector2Int gridPosition = movementPath[currentPathIndex];
            GridTile targetTile = GridManager.Instance.GetTile(gridPosition);
            if (targetTile == null) yield break;

            Vector3 worldPosition = targetTile.transform.position + new Vector3(0, transform.localScale.y / 2, 0);

           
            Vector3 direction = (worldPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                while (Vector3.Distance(transform.position, worldPosition) > 0.1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, worldPosition, (stats.moveSpeed * slowMultiplier) * Time.deltaTime);
                    yield return null;
                }
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
    }






    //---------------------------------------------------------------------------
    //피해 관련 메소드

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



    //---------------------------------------------------------------------------
    //상태 관련 메소드

    /// <summary>
    /// 적이 사망하면 실행됨
    /// </summary>
    private void Die()
    {
        isDead = true;
        Debug.Log($"{enemyData.enemyName}이(가) 사망함!");
        EnemySpawner.Instance.ReturnEnemyToPool(this);
    }

    /// <summary>
    /// 기절 상태 적용
    /// </summary>
    public void Stun(float duration)
    {
        if (isStunned) return;

        isStunned = true;
        StartCoroutine(RemoveStun(duration));
    }

    private IEnumerator RemoveStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    /// <summary>
    /// 속박 상태 적용 (이동 불가, 공격 가능)
    /// </summary>
    public void ApplyRoot(float duration)
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
