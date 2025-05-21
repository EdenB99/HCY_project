using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }
    public int poolSizePerEffect = 3;

    // EffectGroup별로 각 프리팹별 풀 관리
    private Dictionary<ProjectileEffectGroup, Queue<Projectile>> projectilePoolDict = new Dictionary<ProjectileEffectGroup, Queue<Projectile>>();
    private Dictionary<ProjectileEffectGroup, Queue<GameObject>> muzzlePoolDict = new Dictionary<ProjectileEffectGroup, Queue<GameObject>>();
    private Dictionary<ProjectileEffectGroup, Queue<GameObject>> hitPoolDict = new Dictionary<ProjectileEffectGroup, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // Projectile
    public Projectile GetProjectile(ProjectileEffectGroup effect)
    {
        if (effect == null || effect.projectilePrefab == null)
        {
            Debug.LogError("EffectGroup 또는 projectilePrefab이 null입니다!");
            return null;
        }
        if (!projectilePoolDict.ContainsKey(effect))
            projectilePoolDict[effect] = new Queue<Projectile>();
        var queue = projectilePoolDict[effect];
        Projectile proj;
        if (queue.Count > 0)
            proj = queue.Dequeue();
        else
        {
            GameObject obj = Instantiate(effect.projectilePrefab, transform);
            proj = obj.GetComponent<Projectile>();
        }
        proj.gameObject.SetActive(true); // 항상 활성화!
        return proj;
    }
    public void ReturnProjectile(Projectile proj, ProjectileEffectGroup effect)
    {
        if (proj == null || effect == null) return;
        proj.gameObject.SetActive(false);
        if (!projectilePoolDict.ContainsKey(effect))
            projectilePoolDict[effect] = new Queue<Projectile>();
        projectilePoolDict[effect].Enqueue(proj);
    }

    // Muzzle Effect
    public GameObject GetMuzzleEffect(ProjectileEffectGroup effect)
    {
        if (effect == null || effect.muzzleEffectPrefab == null) return null;
        if (!muzzlePoolDict.ContainsKey(effect))
            muzzlePoolDict[effect] = new Queue<GameObject>();
        var queue = muzzlePoolDict[effect];
        if (queue.Count > 0)
            return queue.Dequeue();
        else
        {
            GameObject obj = Instantiate(effect.muzzleEffectPrefab, transform);
            obj.SetActive(true);
            return obj;
        }
    }
    public void ReturnMuzzleEffect(GameObject obj, ProjectileEffectGroup effect)
    {
        if (obj == null || effect == null) return;
        obj.SetActive(false);
        if (!muzzlePoolDict.ContainsKey(effect))
            muzzlePoolDict[effect] = new Queue<GameObject>();
        muzzlePoolDict[effect].Enqueue(obj);
    }

    // Hit Effect
    public GameObject GetHitEffect(ProjectileEffectGroup effect)
    {
        if (effect == null || effect.hitEffectPrefab == null) return null;
        if (!hitPoolDict.ContainsKey(effect))
            hitPoolDict[effect] = new Queue<GameObject>();
        var queue = hitPoolDict[effect];
        if (queue.Count > 0)
            return queue.Dequeue();
        else
        {
            GameObject obj = Instantiate(effect.hitEffectPrefab, transform);
            obj.SetActive(true);
            return obj;
        }
    }
    public void ReturnHitEffect(GameObject obj, ProjectileEffectGroup effect)
    {
        if (obj == null || effect == null) return;
        obj.SetActive(false);
        if (!hitPoolDict.ContainsKey(effect))
            hitPoolDict[effect] = new Queue<GameObject>();
        hitPoolDict[effect].Enqueue(obj);
    }
}