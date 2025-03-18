using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    public UnitData unitData;
    private UnitStats stats;

    [Header("Runtime Data")]
    private bool isDead = false;
    public Vector2Int currentGridTile; // 현재 타일 그리드 좌표
    // 유닛의 위치 및 좌표 데이터
    [SerializeField]
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Highlight(value);
        }
    }

    [Header("Components")]
    private UnitAnimatorController unitAnimatorController;
    private Renderer unitRenderer;
    public Material transparencyMaterial; // 투명 머티리얼
    private Material originalMaterial; // 원래 머티리얼 저장

    
    private void Awake()
    {
        unitAnimatorController = GetComponentInChildren<UnitAnimatorController>();
        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer != null)
            originalMaterial = unitRenderer.material; // 초기 머티리얼 저장
    }
    private void OnEnable()
    {
        if (unitData != null)
            InitializeUnit(unitData);
    }



    public void InitializeUnit(UnitData data)
    {
        unitData = data;
        stats = new UnitStats(unitData);
        if (stats.onStarChanged == null)
            stats.onStarChanged += ApplyStarLevelScaling;
        ApplyStarLevelScaling();
    }

    /// <summary>
    /// 유닛을 특정 타일로 이동
    /// </summary>
    /// <param name="TileGridPos">그리드 좌표</param>
    /// <param name="TileWorldPos">월드 좌표</param>
    public void MoveToTile(Vector2Int TileGridPos, Vector3 TileWorldPos)
    {
        currentGridTile = TileGridPos;
        transform.position = TileWorldPos;
    }

    private void OnMouseDown()
    {
         SelectionManager.Instance.SelectUnit(this);
         Debug.Log($"{unitData.unitName}/{unitData.starLevel}");
    }
    /// <summary>
    /// 유닛 강조 표시
    /// </summary>
    /// <param name="highlight">선택 여부</param>
    public void Highlight(bool highlight)
    {
        if (unitRenderer != null)
            unitRenderer.material = highlight ? transparencyMaterial : originalMaterial;
    }


    /// <summary>
    /// StarLevel이 변경될 때 크기 조정
    /// </summary>
    private void ApplyStarLevelScaling()
    {
        
        float scaleMultiplier = 1.0f + ((stats.starLevel - unitData.starLevel) * 0.2f);
        Transform modelTransform = transform.GetChild(0).transform;
        modelTransform.localScale = Vector3.one * scaleMultiplier;
    }
    
    /// <summary>
    /// 적 유닛이 피해를 받을 때 호출
    /// </summary>
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (isDead) return;
        stats.TakeDamage(damage, damageType);
        if (stats.currentHP <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{unitData.unitName}이(가) 사망!");
        gameObject.SetActive(false);
    }

   
}
