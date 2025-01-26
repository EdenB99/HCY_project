using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameSettings/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Shop Configuration")]
    public ShopInfo ShopInfo;

    [Header("Synergy Configuration")]
    public List<SynergyDatabase> synergies;
}
