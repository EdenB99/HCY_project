using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public List<GridTile> spawnTiles;   // WaveIn 타일 (적이 생성될 위치)

    private Dictionary<EnemyData, Queue<GameObject>> enemyPools = new Dictionary<EnemyData, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 적을 스폰하고 경로를 설정
    /// </summary>
    public void SpawnEnemy(EnemyData enemyData, List<Vector2Int> path)
    {
        GameObject enemyObject = GetEnemyFromPool(enemyData);
        if (enemyObject == null)
            enemyObject = CreateNewEnemy(enemyData);
        // 랜덤한 WaveIn 타일에서 스폰
        GridTile spawnTile = spawnTiles[Random.Range(0, spawnTiles.Count)];
        enemyObject.transform.position = spawnTile.transform.position +
            new Vector3(0, enemyObject.transform.localScale.y / 2, 0);
        // 적 데이터 설정
        Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
        enemyComponent.InitializeEnemy(enemyData, path);
    }

    /// <summary>
    /// 풀에서 Enemy 오브젝트를 가져오기 (없으면 null 반환)
    /// </summary>
    private GameObject GetEnemyFromPool(EnemyData enemyData)
    {
        if (enemyPools.TryGetValue(enemyData, out Queue<GameObject> pool) && pool.Count > 0)
        {
            GameObject enemy = pool.Dequeue();
            enemy.SetActive(true);
            return enemy;
        }
        return null;
    }

    /// <summary>
    /// 새로운 Enemy 오브젝트를 생성하여 반환
    /// </summary>
    private GameObject CreateNewEnemy(EnemyData enemyData)
    {
        GameObject newEnemy = Instantiate(enemyData.enemyPrefab);
        newEnemy.SetActive(true);
        return newEnemy;
    }

    /// <summary>
    /// 적을 풀로 반환
    /// </summary>
    public void ReturnEnemyToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.position = transform.position; // 스포너 위치로 이동

        if (!enemyPools.ContainsKey(enemy.enemyData))
        {
            enemyPools[enemy.enemyData] = new Queue<GameObject>();
        }

        enemyPools[enemy.enemyData].Enqueue(enemy.gameObject);
    }
}
