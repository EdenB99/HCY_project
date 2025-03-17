using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimatorController : MonoBehaviour
{
    private Animator animator;
    private Renderer modelRenderer;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        modelRenderer = GetComponent<Renderer>();
    }
    /// <summary>
    /// 애니메이션 트리거 실행
    /// </summary>
    public void PlayAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
    /// <summary>
    /// 공격 애니메이션 실행 (변형 선택 가능)
    /// </summary>
    public void PlayAttackAnimation(int attackVariant)
    {
        if (animator != null)
        {
            animator.SetInteger("AttackVariant", attackVariant);
            animator.SetTrigger("Attack");
        }
    }
    /// <summary>
    /// 사망 애니메이션 실행
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    
}
