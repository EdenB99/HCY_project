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

    }
}
