using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    public Canvas mainCanvas { get; private set; }
    public ActionPointManager ActionPointManager { get; private set; }
    public ActionPointUI ActionPointUI { get; private set; }
    public DiceCustomizeManager DiceCustomizeManager { get; private set; }
    public BoardManager BoardManager { get; private set; }

    public Piece[] selectedPieces = new Piece[3];

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[GameManager] Active scene changed from {oldScene.name} to {newScene.name}");

        if(newScene.name == "CustomizeScene")
        {
            if(DiceCustomizeManager == null)
            {
                DiceCustomizeManager = FindFirstObjectByType<DiceCustomizeManager>();
            }

            DiceCustomizeManager.Initialize();
        }

        if (newScene.name == "GameScene" || newScene.name == "GameScene_2.0.1")
        {
            mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            ActionPointManager = mainCanvas.GetComponentInChildren<ActionPointManager>();
            ActionPointUI = mainCanvas.GetComponentInChildren<ActionPointUI>();

            if(BoardManager == null)
            {
                BoardManager = FindFirstObjectByType<BoardManager>();
            }
            BoardManager.Initialize();

        }
    }

    public void SetPieces(Piece[] pieces)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            selectedPieces[i].faces = pieces[i].faces;
        }
    }
}