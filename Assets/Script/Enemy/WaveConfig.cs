using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveType
{
    Normal,      // 일반 웨이브
    Boss,        // 보스 웨이브
    Mission,     // 미션 웨이브 (특정 목표 클리어)
    Benediction  // 특수 버프 웨이브
}
[System.Serializable]
public struct EnemyGroupData
{
    public EnemyData enemy;  // 등장할 적 유형
    public int count;        // 등장할 개체 수
}
[System.Serializable]
public struct EnemyWaveData
{
    public WaveType waveType;               // 웨이브 유형 (Normal, Boss 등)
    public List<EnemyGroupData> enemyGroups;
}


[CreateAssetMenu(fileName = "WaveConfig", menuName = "EnemyWave/Create New WaveConfig")]
public class WaveConfig : ScriptableObject
{
    public List<EnemyWaveData> waves; // 여러 웨이브를 저장하는 리스트
}

