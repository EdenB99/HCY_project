using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISynergyEffect
{
    void Apply(Unit unit, int level);
}
public class EmpirePhlemoniaEffect : ISynergyEffect
{
    public void Apply(Unit unit, int level)
    {
        // 레벨별 효과 적용
        switch (level)
        {
            case 3:
                break;
            case 6:
                break;
            case 9:
                break;
        }
    }
}

public class DunkelwaldEffect : ISynergyEffect
{
    public void Apply(Unit unit, int level)
    {
    }
}
