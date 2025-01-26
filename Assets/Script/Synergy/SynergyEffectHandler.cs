using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyEffectHandler
{
    public static void ApplyEffect(Unit unit, string synergyName, int level)
    {
        switch (synergyName)
        {
            case "empire_Phlemonia":
                PhlemoniaEffect(unit, level);
                break;
            case "Wolfswacht_kingdom":
                WolfswachtEffect(unit, level);
                break;
                // 추가적인 시너지 효과 처리...
        }
    }

    private static void PhlemoniaEffect(Unit unit, int level)
    {
        // Phlemonia 시너지 로직...
    }

    private static void WolfswachtEffect(Unit unit, int level)
    {
        // Wolfswacht 시너지 로직...
    }
}
