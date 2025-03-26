using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Data Reference")]
    public UnitData unitData;
    private UnitStats stats;
    private IAttackStrategy attackStrategy;

    [Header("Components")]
    private BoxCollider attackRangeCollider;
    private UnitAnimatorController unitAC;
    private Renderer unitRenderer;
    public Material transparencyMaterial; // 투명 머티리얼
    private Material originalMaterial; // 원래 머티리얼 저장

    [Header("Runtime Data")]
    private float attackCooldown = 0f;
    private HashSet<Enemy> enemiesInRange = new HashSet<Enemy>();
    private HashSet<Enemy> StoppingEnemies = new HashSet<Enemy>();
    private Enemy targetEnemy;
    private float slowMultiplier = 1.0f;
    private bool isRooted = false;
    private bool isAttacking = false;
    private bool isSkill = false;
    private bool isDead = false;
    private bool isStunned = false;
    public Vector2Int currentGridTile; // 현재 타일 그리드 좌표
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


    private void Awake()
    {
        attackRangeCollider = GetComponent<BoxCollider>();
        unitAC = GetComponentInChildren<UnitAnimatorController>();
        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer != null)
            originalMaterial = unitRenderer.material; // 초기 머티리얼 저장

        stats = new UnitStats(unitData);

        InitializeAttackStrategy();
    }

    private void OnEnable()
    {
        if (unitData != null)
            InitializeUnit();
        Enemy.OnEnemyDied += HandleEnemyDeath;
    }
    void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDeath;
    }

    public void InitializeUnit()
    {
        AdjustAttackRangeCollider();
        UpdateEnemiesInRange();
        FindNearestTarget();
        CanStopState();
    }

    private void InitializeAttackStrategy()
    {
        // 유닛 타입에 따라 공격 방식을 설정
        switch (unitData.type)
        {
            case UnitType.Melee:
                attackStrategy = new MeleeAttackStrategy();
                break;
            case UnitType.Range:
                attackStrategy = new RangeAttackStrategy();
                break;
            default:
                attackStrategy = new RangeAttackStrategy();
                break;
        }
    }

    /// <summary>
    /// 적이 죽었을 때 호출되는 함수
    /// </summary>
    private void HandleEnemyDeath(Enemy deadEnemy)
    {
        // enemiesInRange에서 제거
        if (enemiesInRange.Contains(deadEnemy))
        {
            enemiesInRange.Remove(deadEnemy);
            Debug.Log($"{deadEnemy.enemyData.enemyName}이(가) 제거되었습니다. enemiesInRange 업데이트.");

            // StoppingEnemies에서 제거
            if (StoppingEnemies.Contains(deadEnemy))
            {
                deadEnemy.StopState(false);
                StoppingEnemies.Remove(deadEnemy);
                Debug.Log($"{deadEnemy.enemyData.enemyName}이(가) StoppingEnemies에서 제거되었습니다.");
            }

            // 가장 가까운 적 다시 찾기
            FindNearestTarget();

            // CanStopState 업데이트
            CanStopState();
        }
    }

    private void Update()
    {
        if (isDead) return;
        //Sp 회복 로직
        stats.RecoverSP(Time.deltaTime);
        if (stats.currentSP >= stats.maxSP && CanSkill())
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

        // 시선 처리
        if (targetEnemy != null) // targetEnemy가 null인지 확인
            RotateTowardsTarget(targetEnemy.gameObject);
    }




    /// <summary>
    /// BoxCollider의 크기를 stats.Range에 맞게 조정
    /// </summary>
    private void AdjustAttackRangeCollider()
    {
        if (attackRangeCollider != null)
        {
            attackRangeCollider.size = new Vector3(stats.range, 1, stats.range);
            attackRangeCollider.isTrigger = true;
        }
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
        UpdateEnemiesInRange();
        CanStopState();
    }


    /// <summary>
    /// 유닛 강조 표시
    /// </summary>
    /// <param name="highlight">선택 여부</param>
    public void Highlight(bool highlight)
    {
        if (unitRenderer != null)
            unitRenderer.material = highlight ? transparencyMaterial : originalMaterial;
    }



    //---------------------------------------------------------------------------
    //움직임 관련 메소드

    private void RotateTowardsTarget(GameObject targetObject)
    {
        if (unitAC == null || isAttacking) return;

        Vector3 targetPosition;
        if (targetObject == null)
            targetPosition = Vector3.zero;
        else
            targetPosition = targetObject.transform.position;
        Vector3 direction = (targetPosition - unitAC.ACtransfrom.position).normalized;
        if (direction != Vector3.zero)
        {
            direction.y = 0; // Y축 회전 방지
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            unitAC.ACtransfrom.rotation = Quaternion.Lerp(unitAC.ACtransfrom.rotation, targetRotation, 5.0f * Time.deltaTime);
        }
    }

    public void OnAnimationComplete(String animationName)
    {
        switch (animationName)
        {
            case "Attack":
                isAttacking = false;
                attackStrategy.ExecuteAttack(this, targetEnemy);
                break;

            case "Skill":
                isSkill = false;
                break;
            default:
                break;
        }

    }
    //---------------------------------------------------------------------------
    //피해 관련 메소드

    /// <summary>
    /// 범위 내 적을 감지
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other is BoxCollider) return;
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemiesInRange.Add(enemy); // 중복되지 않도록 추가
            FindNearestTarget();
            CanStopState();
        }
    }

    /// <summary>
    /// 범위에서 벗어난 적 제거
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other is BoxCollider) return;
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemiesInRange.Contains(enemy))
        {
            enemy.StopState(false);
            enemiesInRange.Remove(enemy);
            FindNearestTarget();
            CanStopState();
        }
    }
    /// <summary>
    /// 범위 내 적 업데이트
    /// </summary>
    private void UpdateEnemiesInRange()
    {
        // OverlapBox의 크기와 위치 설정
        Vector3 boxCenter = transform.position;
        Vector3 boxSize = attackRangeCollider.size / 2f; // BoxCollider의 크기를 사용
        Quaternion boxRotation = transform.rotation;

        // OverlapBox로 감지된 콜라이더 가져오기
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize, boxRotation);

        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy); // 중복되지 않도록 추가
            }
        }
    }

    /// <summary>
    /// 범위 내 가장 가까운 적을 찾음
    /// </summary>
    private void FindNearestTarget()
    {
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (Enemy enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        targetEnemy = closestEnemy;
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        Debug.Log(targetEnemy.name);
        // 공격 애니메이션 실행
        unitAC.PlayAttackAnimation(UnityEngine.Random.Range(0, 1));
        yield return null;
    }

    /// <summary>
    /// 공격이 가능한지 확인
    /// </summary>
    private bool CanAttack()
    {
        // 공격 불가능한 조건
        if (isDead || isStunned || isAttacking || isSkill || targetEnemy == null)
            return false;

        return true;
    }

    /// <summary>
    /// 유닛이 피해를 받을 때 호출
    /// </summary>
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (isDead) return;

        int finalDamage = CalculateDamage(damage, damageType);
        stats.currentHP -= finalDamage;

        if (stats.currentHP <= 0)
            Die();
    }


    private int CalculateDamage(int baseDamage, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.True:
                return baseDamage;
            case DamageType.StatusEffect:
                return baseDamage;
            default:
                return Mathf.Max(0, baseDamage - stats.durability);
        }
    }



    //---------------------------------------------------------------------------
    //이동 관련 메소드
    /// <summary>
    /// 유닛의 저지가능을 확인하고 실행
    /// </summary>
    private void CanStopState()
    {
        HashSet<Enemy> updatedStoppingEnemies = new HashSet<Enemy>();
        int count = 0;

        // 현재 범위 내 적 중에서 저지 가능한 적을 유지 (StoppingPower만큼 제한)
        foreach (Enemy enemy in StoppingEnemies)
        {
            if (count >= stats.stoppingPower)
                break;

            if (enemiesInRange.Contains(enemy))
            {
                updatedStoppingEnemies.Add(enemy);
                count++;
            }
            else
                enemy.StopState(false);
        }

        // 새로운 적을 추가 (StoppingPower를 초과하지 않도록 제한)
        foreach (Enemy enemy in enemiesInRange)
        {
            if (updatedStoppingEnemies.Count >= stats.stoppingPower)
                break;

            if (!updatedStoppingEnemies.Contains(enemy) && !enemy.IsStopped)
            {
                enemy.StopState(true);
                updatedStoppingEnemies.Add(enemy);
            }
        }
        // StoppingEnemies를 업데이트
        StoppingEnemies = updatedStoppingEnemies;
    }

    private bool CanStop()
    {
        if (isDead || isStunned || stats.type != UnitType.Melee)
            return false;
        return true;
    }

    //---------------------------------------------------------------------------
    //상태 관련 메소드

    private void Die()
    {
        isDead = true;
        Debug.Log($"{unitData.unitName}이(가) 사망!");
        gameObject.SetActive(false);
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
        CanStopState();
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

    //---------------------------------------------------------------------------
    //스킬 관련 메소드
    private void UseSkill()
    {
        Debug.Log($"{unitData.unitName} 스킬 사용!");
        //실제 스킬 코드 필요
    }
    private bool CanSkill()
    {
        if (isDead || isStunned || isSkill)
            return false;
        return true;
    }
}
