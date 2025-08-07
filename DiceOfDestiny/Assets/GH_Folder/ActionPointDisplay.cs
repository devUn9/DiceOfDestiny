using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ActionPointManager의 OnActionPointChanged 이벤트를 구독하여
/// 토큰 오브젝트를 풀링 방식으로 표시/숨김 처리합니다.
/// </summary>
public class ActionPointDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActionPointManager apManager;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform tokenParent;

    private readonly List<GameObject> tokenPool = new();

    private void Awake()
    {
        if (apManager == null)
            apManager = FindAnyObjectByType<ActionPointManager>();
    }

    private void OnEnable()
    {
        if (apManager != null)
            apManager.OnActionPointChanged += RefreshTokens;
    }

    private void OnDisable()
    {
        if (apManager != null)
            apManager.OnActionPointChanged -= RefreshTokens;
    }

    private void RefreshTokens(int count)
    {
        // 풀 확장
        while (tokenPool.Count < count)
        {
            var token = Instantiate(tokenPrefab, tokenParent);
            tokenPool.Add(token);
        }

        // 활성/비활성 조정
        for (int i = 0; i < tokenPool.Count; i++)
        {
            tokenPool[i].SetActive(i < count);
        }
    }
}
