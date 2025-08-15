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
    public ObstacleManager ObstacleManager { get; private set; }

    public Piece[] selectedPieces = new Piece[4];
    public bool isPaused { get; private set; }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene_2.1")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    if (UIManager.Instance.IsSettingUIOpen())
                    {
                        UIManager.Instance.ToggleSettings(false); // 설정창 닫기
                    }
                    else
                    {
                        UnPause();
                    }                    
                }
                else
                {
                    Pause();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Z)) // 행동력 100 추가
        {
            ActionPointManager.Instance.AddAP(100);
        }

        if (Input.GetKeyDown(KeyCode.X)) // 보드 다시 칠?하기?
        {
            StageManager.Instance.StartStage();
        }

        if (Input.GetKeyDown(KeyCode.R)) // 주사위 굴리기
        {
            StageManager.Instance.RollDice();
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[GameManager] Active scene changed from {oldScene.name} to {newScene.name}");

        if(newScene.name == "MainScene")
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

        if (newScene.name == "GameScene_2.1")
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

            if(ObstacleManager == null)
            {
                ObstacleManager = FindFirstObjectByType<ObstacleManager>();
            }
            ObstacleManager.Initialize();

            if (StageManager == null)
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