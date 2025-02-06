using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyWaveManager : MonoBehaviour
{

    public static EnemyWaveManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public List<Dictionary<EnemyData, int>> enemyWaves =
        new List<Dictionary<EnemyData, int>>(); // 웨이브별 적 데이터
    public List<GridTile> spawnTiles;  
    public List<Vector2Int> movementPath;
    [Header("Wave Settings")]
    public float spawnInterval = 2f; // 적 생성 간격
    public int currentWave = 0;

    private Dictionary<EnemyData, Queue<GameObject>> enemyPool = new Dictionary<EnemyData, Queue<GameObject>>(); // 오브젝트 풀링

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
    /// 오브젝트 풀링 초기화 (각 EnemyData별로 개별 풀 생성)
    /// </summary>
    private void InitializePool()
    {
        foreach (var wave in enemyWaves)
        {
            foreach (var entry in wave)
            {
                EnemyData enemyData = entry.Key;
                int count = entry.Value;

                if (!enemyPool.ContainsKey(enemyData))
                {
                    enemyPool[enemyData] = new Queue<GameObject>();
                    for (int i = 0; i < count; i++) // 각 적 개수만큼 생성
                    {
                        GameObject enemy = Instantiate(enemyData.enemyPrefab);
                        enemy.SetActive(false);
                        enemyPool[enemyData].Enqueue(enemy);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 웨이브 시작 (적들을 순차적으로 스폰)
    /// </summary>
    public void StartWave()
    {
        if (currentWave >= enemyWaves.Count)
        {
            Debug.Log("모든 웨이브가 끝났습니다!");
            return;
        }
        StartCoroutine(SpawnWaveCoroutine(enemyWaves[currentWave]));
    }
    /// <summary>
    /// 웨이브 내에서 적들을 일정 간격으로 생성하는 코루틴
    /// </summary>
    private IEnumerator SpawnWaveCoroutine(Dictionary<EnemyData, int> waveData)
    {
        foreach (var entry in waveData)
        {
            EnemyData enemyData = entry.Key;
            int count = entry.Value;

            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(enemyData);
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
    /// <summary>
    /// 적 유닛을 스폰하여 이동 경로를 설정
    /// </summary>
    public void SpawnEnemy(EnemyData enemyData)
    {
        if (!enemyPool.ContainsKey(enemyData) || enemyPool[enemyData].Count == 0)
        {
            Debug.LogWarning($"풀에 {enemyData.enemyName} 적이 부족함! 추가 생성 필요.");
            return;
        }

        GameObject enemyObj = enemyPool[enemyData].Dequeue();
        Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.enemyData = enemyData;
            enemyObj.transform.position = spawnTiles[Random.Range(0, spawnTiles.Count)].transform.position;
            enemyComponent.movementPath = new List<Vector2Int>(movementPath);
            enemyObj.SetActive(true);
        }
        enemyPool[enemyData].Enqueue(enemyObj); // 재사용을 위해 다시 큐에 추가
    }
    /// <summary>
    /// 적 유닛을 풀로 반환
    /// </summary>
    /// <param name="enemy"></param>
    public void ReturnEnemyToPool(Enemy enemy)
    {
        if (!enemyPool.ContainsKey(enemy.enemyData))
        {
            enemyPool[enemy.enemyData] = new Queue<GameObject>();
        }

        // 적 비활성화 후 풀에 반환
        enemy.gameObject.SetActive(false);
        enemyPool[enemy.enemyData].Enqueue(enemy.gameObject);
    }
}
