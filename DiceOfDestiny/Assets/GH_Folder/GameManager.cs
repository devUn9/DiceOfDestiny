using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    public ActionPointManager actionPointManager { get; private set; }

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
            actionPointManager = FindAnyObjectByType<ActionPointManager>();
            actionPointUI = canvas.GetComponentInChildren<ActionPointUI>();

            historyManager = GetComponent<HistoryManager>();
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
            selectedPieces[i].isAvailable = pieces[i].isAvailable;
            selectedPieces[i].faces = pieces[i].faces;
        }
    }
}