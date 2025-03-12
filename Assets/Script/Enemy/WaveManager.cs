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
    }
}
