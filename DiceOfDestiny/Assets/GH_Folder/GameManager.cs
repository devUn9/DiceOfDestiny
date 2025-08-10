using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    private ActionPointManager _actionPointManager;
    public ActionPointManager actionPointManager
    {
        get
        {
            if (_actionPointManager == null)
            {
                var canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    _actionPointManager = canvas.GetComponentInChildren<ActionPointManager>();
                }
                else
                {
                    Debug.LogError("[GameManager] Canvas 오브젝트를 찾을 수 없습니다.");
                }
            }
            return _actionPointManager;
        }
        private set
        {
            _actionPointManager = value;
        }
    }

    public HistoryManager historyManager { get; private set; }

    public ActionPointUI actionPointUI { get; private set; }

    public Piece[] selectedPieces = new Piece[3];

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[GameManager] Active scene changed from {oldScene.name} to {newScene.name}");
        if (SceneManager.GetActiveScene().name == "GameScene_2.0.1")
        {
            Debug.Log("[GameManager] GameScene_2.0.1 scene is loaded, initializing components...");
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogError("[GameManager] Canvas 오브젝트를 찾을 수 없습니다.");
                return;
            }
            _actionPointManager = canvas.GetComponentInChildren<ActionPointManager>();
            actionPointUI = canvas.GetComponentInChildren<ActionPointUI>();

            // if (_actionPointManager == null) Debug.LogError("[GameManager] ActionPointManager를 찾을 수 없습니다.");
            // if (actionPointUI == null) Debug.LogError("[GameManager] actionPointUI를 찾을 수 없습니다.");

            historyManager = GetComponent<HistoryManager>();
            // if (historyManager == null) Debug.LogWarning("[GameManager] HistoryManager가 붙어있지 않습니다.");
        }
    }



    public void SetPieces(Piece[] pieces)
    {
        if (pieces.Length != 3)
        {
            Debug.LogError("[GameManager] SetPieces: pieces 배열의 길이는 3이어야 합니다.");
            return;
        }
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                Debug.LogError($"[GameManager] SetPieces: pieces[{i}]는 null입니다.");
                return;
            }
            selectedPieces[i] = pieces[i];
        }
    }
}