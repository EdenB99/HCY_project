using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI goldText; // 현재 골드 표시
    public TextMeshProUGUI levelText; // 상점 레벨 및 경험치 표시
    public Button rerollButton; // 리롤 버튼
    public Button levelUpButton; // 레벨업 버튼
    public Transform unitSlotParent; // 슬롯들이 배치될 부모 오브젝트
    public GameObject unitSlotPrefab; // 슬롯 프리팹

    private int gold = 10; // 초기 골드
    public int shopLevel = 1; // 초기 상점 레벨
    private int shopExperience = 0; // 초기 경험치
    private int maxShopLevel = 5; // 최대 레벨
    private List<UnitData> availableUnits; // 구매 가능한 유닛 리스트

    private void Start()
    {
        // 버튼 이벤트 등록
        rerollButton.onClick.AddListener(RerollUnits);
        levelUpButton.onClick.AddListener(LevelUpShop);

        UpdateShopUI();
        GenerateUnitSlots();
    }

    // 상점 UI 업데이트
    private void UpdateShopUI()
    {
        goldText.text = $"골드: {gold}";
        levelText.text = $"레벨: {shopLevel} (EXP: {shopExperience})";
    }

    // 유닛 슬롯 생성
    private void GenerateUnitSlots()
    {
        foreach (Transform child in unitSlotParent)
        {
            Destroy(child.gameObject);
        }

        // 5개의 유닛 슬롯 생성
        for (int i = 0; i < 5; i++)
        {
            GameObject slot = Instantiate(unitSlotPrefab, unitSlotParent);
            UnitData unit = GetRandomUnit();
            slot.GetComponent<UnitSlot>().Setup(unit, this);
        }
    }

    // 리롤 버튼 클릭
    private void RerollUnits()
    {
        if (gold < 2) // 리롤 비용
        {
            Debug.LogWarning("골드가 부족합니다!");
            return;
        }

        gold -= 2;
        GenerateUnitSlots();
        UpdateShopUI();
    }

    // 레벨업 버튼 클릭
    private void LevelUpShop()
    {
        if (gold < 5 || shopLevel >= maxShopLevel)
        {
            Debug.LogWarning("레벨업 불가능합니다.");
            return;
        }

        gold -= 5;
        shopExperience += 1;

        // 경험치가 일정 수치를 넘으면 레벨 증가
        if (shopExperience >= 3)
        {
            shopLevel++;
            shopExperience = 0;
        }

        UpdateShopUI();
    }

    // 유닛 리스트에서 랜덤 유닛 선택
    private UnitData GetRandomUnit()
    {
        return availableUnits[Random.Range(0, availableUnits.Count)];
    }

    // 유닛 구매
    public void PurchaseUnit(UnitData unit)
    {
        if (gold < unit.costLevel)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return;
        }

        gold -= unit.costLevel;
        Debug.Log($"유닛 {unit.name}을(를) 구매했습니다!");
        UpdateShopUI();
    }
}
