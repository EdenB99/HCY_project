using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyPanel : MonoBehaviour
{
    public Image synergyIconmask;
    public Image synergyIcon;
    public TextMeshProUGUI synergyLevelText;
    public TextMeshProUGUI synergyNameText;
    public TextMeshProUGUI synergythresholdsText;

    public SynergyEntry synergyData;

    public void SetSynergyData(SynergyEntry setData, int level)
    {
        synergyData = setData;
        synergyIcon.sprite = synergyData.synergyData.icon;
        synergyLevelText.text = $"{setData.units.Count}";
        synergyNameText.text = synergyData.synergyData.synergyName;
        string formattedThresholds = "";
        for (int i = 0; i < synergyData.synergyData.thresholds.Count; i++)
        {
            if (i > 0) formattedThresholds += " / "; // 구분자 추가

            if (i + 1 == level) // 현재 레벨에 해당하는 숫자 강조
                formattedThresholds += $"<color=#FFFFFF>{synergyData.synergyData.thresholds[i]}</color>";
            else
                formattedThresholds += synergyData.synergyData.thresholds[i].ToString();
        }
        synergythresholdsText.text = formattedThresholds;
    }
}
