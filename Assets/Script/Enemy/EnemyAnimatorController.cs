using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    public Transform ACtransfrom;
    private Animator animator;

    private void Awake()
    {
        ACtransfrom = GetComponent<Transform>();
        animator = GetComponent<Animator>();
    }


    /// <summary>
    /// 공격 애니메이션 실행
    /// </summary>
    public void PlayAttackAnimation()
    {
        Debug.Log("is attack");
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// 스킬 애니메이션 실행
    /// </summary>
    public void PlaySkillAnimation()
    {
        animator.SetTrigger("Skill");
    }

    /// <summary>
    /// 혼란 상태 애니메이션 실행
    /// </summary>
    public void PlayMazeAnimation()
    {
        animator.SetTrigger("Maze");
    }

    /// <summary>
    /// 사망 애니메이션 실행
    /// </summary>
    public void PlayDeathAnimation()
    {
        animator.SetTrigger("Dead");
    }
}
