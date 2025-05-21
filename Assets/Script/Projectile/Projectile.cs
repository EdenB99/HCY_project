using System;
using UnityEngine;
using System.Collections;
[System.Serializable]
public class ProjectileEffectGroup
{
    public string effectName;
    public GameObject projectilePrefab;
    public GameObject muzzleEffectPrefab;
    public GameObject hitEffectPrefab;
    public AudioClip shootClip;
    public AudioClip hitClip;
    public float speed = 10f;

    public DamageType damageType;
    public bool isTargeting;
    public float rotSpeed;
    public float effectDelay = 1.0f; // 파티클 지속시간(머즐/히트 이펙트 반환용)
}

public class Projectile : MonoBehaviour
{
    [HideInInspector] public ProjectileEffectGroup effectData;
    [HideInInspector] public MonoBehaviour target;
    [HideInInspector] public int damage;
    [HideInInspector] public DamageType damageType;

    private float speed;
    private bool isTargeting;
    private float rotSpeed;
    private Coroutine autoReturnCoroutine;

    // 캐싱된 이펙트 오브젝트
    private GameObject cachedMuzzleEffect;
    private GameObject cachedHitEffect;

    private void OnEnable()
    {
        if (effectData != null)
        {
            speed = effectData.speed;
            isTargeting = effectData.isTargeting;
            rotSpeed = effectData.rotSpeed;
            damageType = effectData.damageType;

            // 사운드
            if (effectData.shootClip != null)
            {
                var audio = GetComponent<AudioSource>();
                if (audio == null)
                    audio = gameObject.AddComponent<AudioSource>();
                audio.clip = effectData.shootClip;
                audio.Play();
            }

            // 머즐 이펙트: 최초 한 번만 풀에서 받아 캐싱, 이후엔 재사용
            if (effectData.muzzleEffectPrefab != null)
            {
                if (cachedMuzzleEffect == null)
                {
                    cachedMuzzleEffect = ProjectilePool.Instance.GetMuzzleEffect(effectData);
                }
                if (cachedMuzzleEffect != null)
                {
                    cachedMuzzleEffect.transform.position = transform.position;
                    cachedMuzzleEffect.transform.rotation = transform.rotation;
                    cachedMuzzleEffect.SetActive(true);
                    StartCoroutine(ReturnMuzzleEffectAfterDelay(effectData.effectDelay));
                }
            }
        }
        if (autoReturnCoroutine != null)
        {
            StopCoroutine(autoReturnCoroutine);
        }
        autoReturnCoroutine = StartCoroutine(AutoReturnAfterSeconds(3f));
    }

    private IEnumerator AutoReturnAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ReturnToPool();
    }

    private IEnumerator ReturnMuzzleEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cachedMuzzleEffect != null && effectData != null)
        {
            cachedMuzzleEffect.SetActive(false);
            ProjectilePool.Instance.ReturnMuzzleEffect(cachedMuzzleEffect, effectData);
            // cachedMuzzleEffect를 null로 두지 않음: 재사용
        }
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }
        if (isTargeting)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.forward = dir;
        }
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, target.transform.position) < 0.8f)
            OnHit();
    }

    private void OnHit()
    {
        if (target.TryGetComponent<Enemy>(out var enemy))
            enemy.TakeDamage(damage, damageType);
        else if (target.TryGetComponent<Unit>(out var unit))
            unit.TakeDamage(damage, damageType);

        // 히트 이펙트: 최초 한 번만 풀에서 받아 캐싱, 이후엔 재사용
        if (effectData != null && effectData.hitEffectPrefab != null)
        {
            if (cachedHitEffect == null)
            {
                cachedHitEffect = ProjectilePool.Instance.GetHitEffect(effectData);
            }
            if (cachedHitEffect != null)
            {
                cachedHitEffect.transform.position = transform.position;
                cachedHitEffect.transform.rotation = Quaternion.identity;
                cachedHitEffect.SetActive(true);
                StartCoroutine(ReturnHitEffectAfterDelay(effectData.effectDelay));
            }
        }
        // 히트 사운드
        if (effectData != null && effectData.hitClip != null)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.clip = effectData.hitClip;
            audio.Play();
            Destroy(audio, effectData.hitClip.length);
        }
        ReturnToPool();
    }

    private IEnumerator ReturnHitEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cachedHitEffect != null && effectData != null)
        {
            cachedHitEffect.SetActive(false);
            ProjectilePool.Instance.ReturnHitEffect(cachedHitEffect, effectData);
            // cachedHitEffect를 null로 두지 않음: 재사용
        }
    }

    public void Initialize(ProjectileEffectGroup effect, MonoBehaviour target, int damage)
    {
        this.effectData = effect;
        this.target = target;
        this.damage = damage;
        this.damageType = effect != null ? effect.damageType : DamageType.Physical;
        transform.forward = (target.transform.position - transform.position).normalized;
    }

    private void ReturnToPool()
    {
        isTargeting = false;
        if (autoReturnCoroutine != null)
        {
            StopCoroutine(autoReturnCoroutine);
            autoReturnCoroutine = null;
        }
        ProjectilePool.Instance.ReturnProjectile(this, effectData);
        gameObject.SetActive(false);
    }
}