using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    public Canvas mainCanvas { get; private set; }
    public ActionPointManager actionPointManager { get; private set; }
    public ActionPointUI actionPointUI { get; private set; }
    public DiceCustomizeManager diceCustomizeManager { get; private set; }

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
            if(diceCustomizeManager == null)
            {
                diceCustomizeManager = FindFirstObjectByType<DiceCustomizeManager>();
            }

            diceCustomizeManager.Initialize();
        }

        if (newScene.name == "GameScene")
        {
            mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            actionPointManager = mainCanvas.GetComponentInChildren<ActionPointManager>();
            actionPointUI = mainCanvas.GetComponentInChildren<ActionPointUI>();
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