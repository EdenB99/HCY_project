using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public GameObject enemyBasePrefab;
    public int initialPoolCount = 10; // 초기 생성할 적 개수
    public List<GridTile> spawnTiles;   // WaveIn 타일 (적이 생성될 위치)

    private Queue<GameObject> enemyPool = new Queue<GameObject>(); // 오브젝트 풀링

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// 오브젝트 풀링을 통해 적을 미리 생성
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolCount; i++)
        {
            GameObject enemy = Instantiate(enemyBasePrefab, this.transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    /// <summary>
    /// 적을 스폰하고 경로를 설정
    /// </summary>
    public void SpawnEnemy(EnemyData enemyData, List<Vector2Int> path)
    {
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("적 풀에 개체가 부족하여 추가 생성.");
            InitializePool(); // 부족하면 추가 생성
        }
        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);

        // 랜덤한 WaveIn 타일에서 스폰
        GridTile spawnTile = spawnTiles[Random.Range(0, spawnTiles.Count)];
        enemy.transform.position = spawnTile.transform.position +
            new Vector3(0, enemy.transform.localScale.y / 2, 0); ;

        // 적 데이터 설정
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        enemyComponent.InitializeEnemy(enemyData, path);
        if (enemyData.enemyPrefab != null)
        {
            GameObject model = Instantiate(enemyData.enemyPrefab, enemy.transform);
        }
        enemyPool.Enqueue(enemy); // 풀로 다시 반환
    }
    /// <summary>
    /// 적을 풀로 반환
    /// </summary>
    public void ReturnEnemyToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        foreach (Transform child in enemy.transform)
        {
            Destroy(child.gameObject); // 자식 오브젝트 제거
        }
        enemy.transform.position = transform.position; // 스포너 위치로 이동
    }
}
