using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class TutorialBootstrap : MonoBehaviour
{
    [SerializeField] private Transform boardPos;
    [SerializeField] private MonoBehaviour boardManager;   // 실제 BoardManager 타입으로 드래그
    [SerializeField] private MonoBehaviour stageManager;   // 실제 StageManager 타입으로 드래그
    [SerializeField] private MonoBehaviour pieceManager;   // 실제 PieceManager 타입으로 드래그

    private void Awake()
    {
        Time.timeScale = 1f;

        if (boardPos == null)
        {
            var go = GameObject.Find("BoardPos");
            if (go != null) boardPos = go.transform;
            else Debug.LogError("[Tutorial] BoardPos not found.");
        }

        if (boardManager == null) boardManager = FindAnyObjectByType<BoardManager>();

        if (boardManager == null) Debug.LogWarning("[Tutorial] BoardManager missing.");
        if (stageManager == null) Debug.LogWarning("[Tutorial] StageManager missing.");
        if (pieceManager == null) Debug.LogWarning("[Tutorial] PieceManager missing.");
    }
}