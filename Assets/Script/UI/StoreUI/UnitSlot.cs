using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    [Header("Component")]
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI costText;
    public Image unitImage;
    public Button purchaseButton;
    public Transform synergySlotfirst;
    public Transform synergySlotsecond;
    public Transform synergySlotthird;

    [Header("Data")]
    public UnitData unitData;
    private ShopManager shopManager;

    // 슬롯 초기화
    public void Setup(UnitData unitdata, ShopManager manager)
    {
        unitData = unitdata;
        shopManager = manager;

        unitNameText.text = unitData.unitName;
        costText.text = $"{unitData.costLevel}";
        unitImage.sprite = unitData.image;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() => shopManager.PurchaseUnit(this));

        UpdateSynergySlots();
    }
    private void UpdateSynergySlots()
    {
        // 모든 시너지 슬롯을 비활성화
        synergySlotfirst.gameObject.SetActive(false);
        synergySlotsecond.gameObject.SetActive(false);
        synergySlotthird.gameObject.SetActive(false);

        // 유닛의 시너지 리스트 순회
        for (int i = 0; i < unitData.synergyList.Count; i++)
        {
            Transform currentSlot = null;
            // 현재 시너지 슬롯 선택
            switch (i)
            {
                case 0:
                    currentSlot = synergySlotfirst;
                    break;
                case 1:
                    currentSlot = synergySlotsecond;
                    break;
                case 2:
                    currentSlot = synergySlotthird;
                    break;
                default:
                    // 슬롯을 초과하면 무시
                    continue;
            }

            // 슬롯 활성화
            currentSlot.gameObject.SetActive(true);

            // 아이콘 및 텍스트 업데이트
            var synergyIcon = currentSlot.GetComponentInChildren<Image>();
            var synergyText = currentSlot.GetComponentInChildren<TextMeshProUGUI>();

            SynergyDatabase synergyData = unitData.synergyList[i];
            if (synergyData != null)
            {
                synergyIcon.sprite = synergyData.icon;
                synergyText.text = synergyData.synergyName;
            }
        }
    }


}
