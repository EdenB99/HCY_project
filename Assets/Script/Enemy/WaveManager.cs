using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Configuration")]
    public WaveConfig waveConfigs; // 여러 개의 웨이브 설정을 저장
    private int currentWaveIndex = 0;
    private bool isWaveActive = false;

    public List<GridTile> spawnTiles;  
    public List<Vector2Int> movementPath;
    [Header("Wave Settings")]
    public float spawnInterval = 2f; // 적 생성 간격
    public int currentWave = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNextWave();
    }

    /// <summary>
    /// 다음 웨이브를 시작
    /// </summary>
    public void StartNextWave()
    {
        if (currentWaveIndex >= waveConfigs.waves.Count)
        {
            Debug.Log("모든 웨이브 완료!");
            return;
        }
        EnemyWaveData currentWaveData = waveConfigs.waves[currentWaveIndex];
        StartCoroutine(HandleWave(currentWaveData));
        currentWaveIndex++;
    }
    /// <summary>
    /// 웨이브 실행 및 적 생성
    /// </summary>
    private IEnumerator HandleWave(EnemyWaveData waveData)
    {
        isWaveActive = true;
        Debug.Log($"[WaveType: {waveData.waveType}] 웨이브 진행 중");
        foreach (var enemyGroup in waveData.enemyGroups)
        {
            for (int i = 0; i < enemyGroup.count; i++)
            {
                EnemySpawner.Instance.SpawnEnemy(enemyGroup.enemy, movementPath);
                yield return new WaitForSeconds(1f); // 몬스터 생성 간격
            }
        }
        isWaveActive = false;
        Debug.Log("웨이브 종료!");

        yield return new WaitForSeconds(3f); // 웨이브 종료 후 대기 시간
        StartNextWave(); // 다음 웨이브 진행
    }
    
}
