using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSlot : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI costText;
    public Image unitImage;
    public Button purchaseButton;

    private UnitData unit;
    private ShopManager shopManager;

    // 슬롯 초기화
    public void Setup(UnitData unitData, ShopManager manager)
    {
        unit = unitData;
        shopManager = manager;

        unitNameText.text = unit.name;
        costText.text = $"{unit.costLevel} 골드";
        unitImage.sprite = unit.image;
        purchaseButton.onClick.AddListener(() => shopManager.PurchaseUnit(unit));
    }
}
