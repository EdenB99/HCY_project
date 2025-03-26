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
