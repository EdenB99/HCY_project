using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Unit Data Reference")]
    public UnitData unitData;
    public UnitStats stats;
    private IAttackStrategy attackStrategy;

    [Header("Components")]
    private BoxCollider attackRangeCollider;
    private UnitAnimatorController unitAC;
    private Renderer unitRenderer;
    public Material transparencyMaterial; // 투명 머티리얼
    private Material originalMaterial; // 원래 머티리얼 저장

    [Header("Buff Management")]
    private List<UnitBuff> activeBuffs = new List<UnitBuff>(); // 활성화된 버프 리스트
    private UnitStats tempStats; // 버프가 적용된 임시 스탯
    public Action onBuffChanged; // 버프 변경 시 호출되는 이벤트

    [Header("Runtime Data")]
    public int killCount;
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
    public Action<Unit> OnUnitDied; // 유닛 사망 시 호출
    public Action<Unit, Enemy> OnAttacked; // 공격 시 호출
    public Action<Unit> OnSkillUsed; // 스킬 사용 시 호출
    public Action<Unit, int, DamageType> OnTakeDamaged; // 피해를 받을 때 호출


    private void Awake()
    {
        attackRangeCollider = GetComponent<BoxCollider>();
        unitAC = GetComponentInChildren<UnitAnimatorController>();
        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer != null)
            originalMaterial = unitRenderer.material; // 초기 머티리얼 저장

        stats = new UnitStats(unitData);
        tempStats = stats;
        InitializeAttackStrategy();
    }

    private void OnEnable()
    {
        if (unitData != null)
            InitializeUnit();
    }
    void OnDisable()
    {
        foreach (var enemy in enemiesInRange)
            enemy.OnEnemyDied -= HandleEnemyDeath;
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

            // StoppingEnemies에서 제거
            if (StoppingEnemies.Contains(deadEnemy))
            {
                deadEnemy.StopState(false);
                StoppingEnemies.Remove(deadEnemy);
            }

            // 가장 가까운 적 다시 찾기
            FindNearestTarget();
            // CanStopState 업데이트
            CanStopState();
            killCount++;
        }
    }

    private void Update()
    {
        if (isDead) return;
        //Sp 회복 로직
        tempStats.RecoverSP(Time.deltaTime);
        if (tempStats.currentSP >= tempStats.maxSP && CanSkill())
        {
            UseSkill();
            tempStats.currentSP = 0;
        }


        // 공격 대기 시간 감소
        attackCooldown -= Time.deltaTime;
        attackCooldown = Mathf.Max(0f, attackCooldown);
        if (attackCooldown <= 0f && CanAttack())
        {
            StartCoroutine(Attack());
            attackCooldown = Mathf.Max(1.0f / 3.0f, 1.0f / tempStats.attackSpeed);
        }

        // 시선 처리
        if (targetEnemy != null) // targetEnemy가 null인지 확인
            RotateTowardsTarget(targetEnemy.gameObject);

         onBuffChanged += InitializeUnit;
    }




    /// <summary>
    /// BoxCollider의 크기를 stats.Range에 맞게 조정
    /// </summary>
    private void AdjustAttackRangeCollider()
    {
        if (attackRangeCollider != null)
        {
            attackRangeCollider.size = new Vector3(tempStats.range, 1, tempStats.range);
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
            enemy.OnEnemyDied += HandleEnemyDeath;
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
            enemy.OnEnemyDied -= HandleEnemyDeath;
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
        if (StoppingEnemies.Count > 0)
        {
            targetEnemy = null; // 초기화
            foreach (Enemy enemy in StoppingEnemies)
            {
                targetEnemy = enemy; // 첫 번째 적을 타겟으로 지정
                break;
            }
            return; // StoppingEnemies에 적이 있으면 여기서 종료
        }
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
        OnAttacked?.Invoke(this, targetEnemy); // OnAttacked 호출
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

        OnTakeDamaged?.Invoke(this, finalDamage, damageType); // OnTakeDamaged 호출

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
                return Mathf.Max(0, baseDamage - tempStats.durability);
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
            if (count >= tempStats.stoppingPower)
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
            if (updatedStoppingEnemies.Count >= tempStats.stoppingPower)
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
        OnUnitDied?.Invoke(this); // OnUnitDied 호출
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

    /// <summary>
    /// 버프 추가 메서드
    /// </summary>
    public void AddBuff(UnitBuff newBuff)
    {
        // 동일한 버프가 이미 존재하지 않을 경우 추가
        if (!activeBuffs.Exists(buff => buff.buffName == newBuff.buffName))
        {
            activeBuffs.Add(newBuff);
            StartCoroutine(Coroutine_BuffEnd(newBuff));
            UpdateTempStats();
            onBuffChanged?.Invoke();
        }
    }

    /// <summary>
    /// 버프 종료 코루틴
    /// </summary>
    private IEnumerator Coroutine_BuffEnd(UnitBuff buff)
    {
        yield return new WaitForSeconds(buff.duration);
        activeBuffs.Remove(buff);
        UpdateTempStats();
        onBuffChanged?.Invoke();
    }

    /// <summary>
    /// 버프 변경 시 임시 스탯 업데이트
    /// </summary>
    private void UpdateTempStats()
    {
        // 기본 stats를 복사
        tempStats = stats;

        // 활성화된 버프를 적용
        foreach (var buff in activeBuffs)
        {
            switch (buff.statName)
            {
                case "attackPower":
                    tempStats.attackPower += Mathf.RoundToInt(stats.attackPower * buff.value);
                    break;
                case "durability":
                    tempStats.durability += Mathf.RoundToInt(stats.durability * buff.value);
                    break;
                case "critChance":
                    tempStats.critChance += buff.value;
                    break;
                // 필요한 스탯 추가
                default:
                    Debug.LogWarning($"Unknown stat name: {buff.statName}");
                    break;
            }
        }
    }
    /// <summary>
    /// 현재 스탯 반환 (버프 적용된 상태)
    /// </summary>
    public UnitStats GetCurrentStats()
    {
        return tempStats;
    }
    //---------------------------------------------------------------------------
    //스킬 관련 메소드
    private void UseSkill()
    {
        Debug.Log($"{unitData.unitName} 스킬 사용!");
        OnSkillUsed?.Invoke(this); // OnSkillUsed 호출
        // 실제 스킬 코드 필요
    }
    private bool CanSkill()
    {
        if (isDead || isStunned || isSkill)
            return false;
        return true;
    }
}
