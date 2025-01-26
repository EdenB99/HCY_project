using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager Instance { get; private set; }

    [Header("Game Configuration")]
    public GameConfig gameConfig; 

    [Header("Managers")]
    public GridManager gridManager;
    public SelectionManager selectionManager;
    public ShopManager shopManager;
    public SynergieManager synergyManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogError("Gamemanager가 두 개 이상 존재한다.");
            Destroy(gameObject);
            return;
        }

        //씬 전환 시 삭제되지 않도록
        DontDestroyOnLoad(gameObject);
    }

    //매니저를 추가할때 마다 하단에 추가
    private void Start()
    {
        InitializeManagers();
        InitializeConfig();
    }
    /// <summary>
    /// 매니저 초기화
    /// </summary>
    private void InitializeManagers()
    {

        if (gridManager == null)
            Debug.LogError("GridManager가 미설정");
        if (selectionManager == null)
            Debug.LogError("selectionManager가 미설정");
        if (shopManager == null)
            Debug.LogError("ShopManager가 미설정되었습니다.");
        if (synergyManager == null)
            Debug.LogError("SynergyManager가 미설정되었습니다.");
        Debug.Log("GameManager 초기화 완료");
    }

    public void InitializeConfig()
    {
        if (gameConfig == null)
        {
            Debug.Log("저장된 설정없음");
            return;
        }
        synergyManager.InitializeSynergies(gameConfig.synergies);
        shopManager.InitializeShop(gameConfig.ShopInfo);
        Debug.Log("Initialize complete");
    }
    public void InitializeConfig(GameConfig NewConfig)
    {
        gameConfig = NewConfig;
        if (gameConfig == null)
        {
            Debug.Log("저장된 설정없음");
            return;
        }
        shopManager.InitializeShop(gameConfig.ShopInfo);
        synergyManager.InitializeSynergies(gameConfig.synergies);
        Debug.Log("Initialize complete");
    }
}

