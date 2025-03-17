using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public string xmlFilePath = "Assets/Data/WaveConfig.xml"; // XML 파일 경로

    private Dictionary<string, Dictionary<EnemyData, int>> waves = new Dictionary<string, Dictionary<EnemyData, int>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadWaveDataFromXML();
    }

    private void LoadWaveDataFromXML()
    {
        WaveConfigXML waveConfig = WaveConfigXML.LoadFromFile(xmlFilePath);
        if (waveConfig == null) return;

        foreach (var wave in waveConfig.waves)
        {
            Dictionary<EnemyData, int> enemyGroup = new Dictionary<EnemyData, int>();

            foreach (var enemy in wave.enemyGroups)
            {
                EnemyData enemyData = Resources.Load<EnemyData>($"EnemyData/{enemy.enemyName}");
                if (enemyData != null)
                {
                    enemyGroup[enemyData] = enemy.count;
                }
                else
                {
                    Debug.LogError($"EnemyData {enemy.enemyName}을 찾을 수 없습니다.");
                }
            }

            waves[wave.waveName] = enemyGroup;
        }
    }

    /// <summary>
    /// 인스펙터에서 실행 가능 (ContextMenu)
    /// </summary>
    [ContextMenu("Start Wave")]
    public void StartWave(string waveName)
    {
        if (!waves.ContainsKey(waveName))
        {
            Debug.LogError($"웨이브 {waveName}을 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(SpawnWaveEnemies(waveName));
    }

    private IEnumerator SpawnWaveEnemies(string waveName)
    {
        Dictionary<EnemyData, int> enemyGroups = waves[waveName];

        foreach (var group in enemyGroups)
        {
            for (int i = 0; i < group.Value; i++)
            {
                EnemySpawner.Instance.SpawnEnemy(group.Key, new List<Vector2Int>());
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
