using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemySpawn : MonoBehaviour
{
    [Header("Test Spawn Settings")]
    public EnemyData enemyData;          // 생성할 적 데이터
    public List<Vector2Int> movementPath; // 이동 경로

    private void Start()
    {
        if (enemyData == null || movementPath == null || movementPath.Count == 0)
        {
            Debug.LogError("EnemyData 또는 이동 경로가 설정되지 않았습니다.");
            return;
        }
        StartCoroutine(StartSpawn());
    }
    private  IEnumerator StartSpawn()
    {
        EnemySpawner.Instance.SpawnEnemy(enemyData, movementPath);
        yield return new WaitForSeconds(3f);
         EnemySpawner.Instance.SpawnEnemy(enemyData, movementPath);
        yield return new WaitForSeconds(3f);
         EnemySpawner.Instance.SpawnEnemy(enemyData, movementPath);

    }
}
