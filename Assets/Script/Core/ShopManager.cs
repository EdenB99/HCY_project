using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ShopInfo
{
    public int gold;        // 보유 골드
    public int shopExp;     // 현재 경험치
    public int turnGold;    // 턴당 얻는 골드
    public int turnExp;     // 턴당 얻는 경험치
    public int shopLevel;   // 상점 레벨
    public int maxShopLevel; // 최대 레벨
    public int expCost;     // 경험치 구매 비용
    public int rerollCost;  // 리롤  구매 비용
}

public class ShopManager : MonoBehaviour
{
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

    [Header("Shop Data")]
    public ShopInfo shopData; // 구조체로 상점 데이터 관리

    [Header("Experience Requirements")]
    public List<int> expRequirements = new List<int>
    {
        2, 6, 10, 20, 36, 48, 76, 84, 120, 150 // 요구 경험치
    };

    [Header("UnitList")]
    public List<UnitData> availableUnits;

    private void Start()
    {
        rerollButton.onClick.AddListener(RerollUnits);
        expButton.onClick.AddListener(LevelUpShop);

        UpdateShopUI();
        GenerateUnitSlots();
    }

    private void UpdateShopUI()
    {
        goldText.text = $"{shopData.gold}";
        levelText.text = $"{shopData.shopLevel}";

        int requiredExp = shopData.shopLevel <= shopData.maxShopLevel ? expRequirements[shopData.shopLevel - 1] : 0;
        expText.text = $"{shopData.shopExp}/{requiredExp}";

        expCostText.text = $"{shopData.expCost}";
        rerollCostText.text = $"{shopData.rerollCost}";
    }

    private void GenerateUnitSlots()
    {
        foreach (Transform child in unitSlotParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 5; i++)
        {
            GameObject slot = Instantiate(unitSlotPrefab, unitSlotParent);
            UnitData unit = GetRandomUnit();
            slot.GetComponent<UnitSlot>().Setup(unit, this);
        }
    }

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

    private void LevelUpShop()
    {
        if (shopData.gold < shopData.expCost || shopData.shopLevel >= shopData.maxShopLevel)
        {
            Debug.LogWarning("레벨업 불가능합니다.");
            return;
        }

        shopData.gold -= shopData.expCost;
        shopData.shopExp += 1;

        int requiredExp = expRequirements[shopData.shopLevel - 1];
        if (shopData.shopExp >= requiredExp)
        {
            shopData.shopLevel++;
            shopData.shopExp -= requiredExp;
        }

        UpdateShopUI();
    }

    private UnitData GetRandomUnit()
    {
        return availableUnits[Random.Range(0, availableUnits.Count)];
    }

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
