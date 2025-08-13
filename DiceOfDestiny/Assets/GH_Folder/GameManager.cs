using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    public Canvas mainCanvas { get; private set; }
    public UIManager UIManager { get; private set; }
    public DiceCustomizeManager DiceCustomizeManager { get; private set; }
    public BoardManager BoardManager { get; private set; }
    public StageManager StageManager { get; private set; }

    public Piece[] selectedPieces = new Piece[3];

    bool isPaused = false;

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "GameScene" || SceneManager.GetActiveScene().name == "GameScene_2.0.1" || SceneManager.GetActiveScene().name == "GameScene_2.0.1hk")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    Pause();
                }
                else
                {
                    UnPause();
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[GameManager] Active scene changed from {oldScene.name} to {newScene.name}");

        if(newScene.name == "Main")
        {
            if(UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }
            UIManager.InitializeMainUI();
        }

        if (newScene.name == "CustomizeScene")
        {
            if(DiceCustomizeManager == null)
            {
                DiceCustomizeManager = FindFirstObjectByType<DiceCustomizeManager>();
            }

            DiceCustomizeManager.Initialize();
        }

        if (newScene.name == "GameScene" || newScene.name == "GameScene_2.0.1" || newScene.name == "GameScene_2.0.1hk")
        {
            if(UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }
            UIManager.InitializeGameUI();

            if (BoardManager == null)
            {
                BoardManager = FindFirstObjectByType<BoardManager>();
            }
            BoardManager.Initialize();

            if(StageManager == null)
            {
                StageManager = FindFirstObjectByType<StageManager>();
            }
            StageManager.StartStage();

        }
    }

    public void SetPieces(Piece[] pieces)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            selectedPieces[i].faces = pieces[i].faces;
        }
    }

    public void Pause()
    {
        UIManager.TogglePauseMenu();
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void UnPause()
    {
        UIManager.TogglePauseMenu();
        isPaused = false;
        Time.timeScale = 1f;
    }
}