using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager Instance { get; private set; }

    //관리할 매니저
    public GridManager gridManager;
    public SelectionManager selectionManager;
    public ShopManager shopManager;

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
        if (gridManager == null)
            Debug.LogError("GridManager가 미설정");
        if (selectionManager == null)
            Debug.LogError("selectionManager가 미설정");
        if (shopManager == null)
            Debug.LogError("ShopManager가 미설정되었습니다.");
        Debug.Log("GameManager 초기화 완료");

    }
}

