using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackStrategy
{
    void ExecuteAttack(MonoBehaviour attacker, MonoBehaviour target);
}

public class RangeAttackStrategy : IAttackStrategy
{
    public void ExecuteAttack(MonoBehaviour attacker, MonoBehaviour target)
    {
        if (attacker is Unit unitAttacker && target is Enemy enemyTarget)
        {
            // 유닛이 적을 공격 - 투사체 발사
            var effect = unitAttacker.unitData.projectileEffectGroup;
            int damageVal = unitAttacker.unitData.attackPower;
            if (effect == null || effect.projectilePrefab == null)
            {
                Debug.LogWarning($"{unitAttacker.unitData.unitName}의 projectileEffectGroup이 할당되지 않았습니다. 기본 공격으로 대체합니다.");
                enemyTarget.TakeDamage(damageVal, DamageType.Physical);
                return;
            }
            var projectile = ProjectilePool.Instance.GetProjectile(effect);
            projectile.transform.position = attacker.transform.position;
            projectile.Initialize(effect, target, damageVal);
        }
        else if (attacker is Enemy enemyAttacker && target is Unit unitTarget)
        {
            // 적이 유닛을 공격 (기존 방식 유지)
            int damage = enemyAttacker.enemyData.attackPower;
            unitTarget.TakeDamage(damage, DamageType.Physical);
            Debug.Log($"{enemyAttacker.enemyData.enemyName}이(가) {unitTarget.unitData.unitName}에게 {damage}의 물리 피해를 입혔습니다.");
        }
    }
}

public class MeleeAttackStrategy : IAttackStrategy
{
    public void ExecuteAttack(MonoBehaviour attacker, MonoBehaviour target)
    {
        if (attacker is Unit unitAttacker && target is Enemy enemyTarget)
        {
            // 유닛이 적을 공격
            int damage = unitAttacker.unitData.attackPower;
            enemyTarget.TakeDamage(damage, DamageType.Physical);
            Debug.Log($"{unitAttacker.unitData.unitName}이(가) {enemyTarget.enemyData.enemyName}에게 {damage}의 물리 피해를 입혔습니다.");
        }
        else if (attacker is Enemy enemyAttacker && target is Unit unitTarget)
        {
            // 적이 유닛을 공격
            int damage = enemyAttacker.enemyData.attackPower;
            unitTarget.TakeDamage(damage, DamageType.Physical);
            Debug.Log($"{enemyAttacker.enemyData.enemyName}이(가) {unitTarget.unitData.unitName}에게 {damage}의 물리 피해를 입혔습니다.");
        }
    }
}
