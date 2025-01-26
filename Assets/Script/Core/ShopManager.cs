using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ShopInfo
{

    public int gold;
    public int shopExp;
    public int turnGold;    // 턴당 얻는 골드
    public int turnExp;     // 턴당 얻는 경험치
    public int shopLevel;   // 상점 레벨
    public int maxShopLevel; // 최대 레벨
    public int expCost;     // 경험치 구매 비용
    public int rerollCost;  // 리롤  구매 비용
    public int gainExp;     //구매시 얻는 경험치
    public int UnitSlotNum; //유닛 슬롯 개수

    [Header("Unit Appearance Rates")]
    public List<UnitAppearanceRate> unitRatesByLevel; // 레벨별 유닛 등장 확률

    [System.Serializable]
    public struct UnitAppearanceRate
    {
        public int shopLevel;               // 상점 레벨
        public int expRequirement;          // 요구 경험치
        public List<CostLevelRate> rates;   // 유닛 비용에 따른 등장 확률
    }
    [System.Serializable]
    public struct CostLevelRate
    {
        public int costLevel;               // 유닛의 비용
        public float spawnRate;             // 해당 비용의 등장 확률 (0~1)
    }
    public List<UnitData> availableUnits;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    //현재 사용되는 데이터
    [Header("Shop Data")]
    public ShopInfo shopData;
    [Header("Component")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI expCostText;
    public TextMeshProUGUI rerollCostText;
    public Button rerollButton;
    public Button expButton;
    public Transform unitSlotParent;
    public GameObject unitSlotPrefab;
    public TextMeshProUGUI oneCostText;
    public TextMeshProUGUI twoCostText;
    public TextMeshProUGUI threeCostText;
    public TextMeshProUGUI fourCostText;
    public TextMeshProUGUI FiveCostText;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Debug.LogError("ShopManager가 두 개 이상 존재합니다.");
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        rerollButton.onClick.AddListener(RerollUnits);
        expButton.onClick.AddListener(LevelUpShop);
        UpdateShopUI();
    }
    /// <summary>
    /// ShopManager 초기화
    /// </summary>
    /// <param name="shopInfo">GameConfig에서 받은 ShopInfo 데이터</param>
    public void InitializeShop(ShopInfo shopInfo)
    {
        shopData = shopInfo;
        UpdateShopUI();
        GenerateUnitSlots();
    }
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateShopUI()
    {
        //현재 레벨에 맞는 Data값 호출
        var currentLevelData =
            shopData.unitRatesByLevel.Find(r => r.shopLevel == shopData.shopLevel);
        if (currentLevelData.shopLevel == 0) return;

        goldText.text = $"{shopData.gold}";
        levelText.text = $"{shopData.shopLevel}";
        expText.text = $"{shopData.shopExp}/{currentLevelData.expRequirement}";
        expCostText.text = $"{shopData.expCost}";
        rerollCostText.text = $"{shopData.rerollCost}";
        UpdateCostTexts(currentLevelData.rates);
    }
    /// <summary>
    /// 비용별 확률 텍스트 업데이트
    /// </summary>
    /// <param name="rates">현재 레벨의 비용별 확률 리스트</param>
    private void UpdateCostTexts(List<ShopInfo.CostLevelRate> rates)
    {
        // 비용별 텍스트 초기화
        oneCostText.text = "0%";
        twoCostText.text = "0%";
        threeCostText.text = "0%";
        fourCostText.text = "0%";
        FiveCostText.text = "0%";

        // 각 비용별 확률을 텍스트에 반영
        foreach (var rate in rates)
        {
            string percentage = $"{rate.spawnRate * 100f:F0}%"; // 확률을 %로 변환

            switch (rate.costLevel)
            {
                case 1:
                    oneCostText.text = percentage;
                    break;
                case 2:
                    twoCostText.text = percentage;
                    break;
                case 3:
                    threeCostText.text = percentage;
                    break;
                case 4:
                    fourCostText.text = percentage;
                    break;
                case 5:
                    FiveCostText.text = percentage;
                    break;
            }
        }
    }

    /// <summary>
    /// 유닛슬롯 생성
    /// </summary>
    private void GenerateUnitSlots()
    {
        foreach (Transform child in unitSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < shopData.UnitSlotNum; i++)
        {
            GameObject slot = Instantiate(unitSlotPrefab, unitSlotParent);
            UnitData unit = GetRandomUnitByLevel();
            slot.GetComponent<UnitSlot>().Setup(unit, this);
        }
    }
    private UnitData GetRandomUnitByLevel()
    {
        var levelData = shopData.unitRatesByLevel.Find(r => r.shopLevel == shopData.shopLevel);
        if (levelData.rates.Count == 0) return null;

        float randomValue = Random.value;
        float cumulativeProbability = 0;

        foreach (var rate in levelData.rates)
        {
            cumulativeProbability += rate.spawnRate;
            if (randomValue <= cumulativeProbability)
            {
                // Filter available units based on active synergies
                var units = shopData.availableUnits.FindAll(u =>
                    u.costLevel == rate.costLevel &&
                    u.synergyList.Exists(synergy => SynergieManager.Instance.synergies.Contains(synergy))
                );

                if (units.Count > 0)
                {
                    return units[Random.Range(0, units.Count)];
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 상점 리롤
    /// </summary>
    private void RerollUnits()
    {
        if (shopData.gold < shopData.rerollCost)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return;
        }

        shopData.gold -= shopData.rerollCost;
        GenerateUnitSlots();
        UpdateShopUI();
    }
    /// <summary>
    /// 경험치 구매
    /// </summary>
    private void LevelUpShop()
    {
        var currentLevelData = shopData.unitRatesByLevel.Find(r => r.shopLevel == shopData.shopLevel);
        if (currentLevelData.shopLevel == 0 || shopData.gold < shopData.expCost)
        {
            Debug.LogWarning("레벨업 불가능합니다.");
            return;
        }

        shopData.gold -= shopData.expCost;
        shopData.shopExp += shopData.gainExp;

        if (shopData.shopExp >= currentLevelData.expRequirement)
        {
            shopData.shopLevel++;
            shopData.shopExp -= currentLevelData.expRequirement;
        }

        UpdateShopUI();
    }
    /// <summary>
    /// 유닛 구매
    /// </summary>
    /// <param name="clickedSlot"></param>
    public void PurchaseUnit(UnitSlot clickedSlot)
    {
        UnitData unit = clickedSlot.unitData;
        if (shopData.gold < unit.costLevel)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return;
        }

        shopData.gold -= unit.costLevel;
        Debug.Log($"유닛 {unit.name}을(를) 구매했습니다!");

        clickedSlot.gameObject.SetActive(false);
        UpdateShopUI();
    }
}
