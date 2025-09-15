using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singletone<GameManager>
{
    public Canvas mainCanvas { get; private set; }
    public UIManager UIManager { get; private set; }
    public DiceCustomizeManager DiceCustomizeManager { get; private set; }
    public ActionPointManager ActionPointManager { get; private set; }
    public BoardManager BoardManager { get; private set; }
    public StageManager StageManager { get; private set; }
    public ObstacleManager ObstacleManager { get; private set; }
    public ToastManager ToastManager { get; private set; }

    public Piece[] selectedPieces = new Piece[4];
    public bool isPaused { get; private set; }

    [SerializeField] public bool IsLockCursor { get; set; } = false;

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

        if (oldScene.name == "GameScene_2.1" || oldScene.name == "TutorialScene")
        {
            if (ActionPointManager.Instance != null)
            {
                ActionPointManager.Instance.SetZero();
                Destroy(ActionPointManager.Instance.gameObject);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateActionPointUI();
            }

            if (PieceManager.Instance != null)
            {
                PieceManager.Instance.ClearPieces();
                PieceManager.Instance.pieceDatas = new Piece[4];
            }
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.ClearBoard();
            }
            if (ObstacleManager.Instance != null)
            {
                ObstacleManager.Instance.RemoveAllObstacle();
            }
            if (ToastManager.Instance != null)
            {
                ToastManager.Instance.ClearAllToasts();
            }
            if (PieceFaceManager.Instance != null)
            {
                // Recreate the singleton storage to remove any cached face state
            }
            if (StageManager.Instance != null)
            {
                StageManager.Instance.StageClear();
                StageManager.Instance.ClearStageState();
            }
        }

        if (newScene.name == "MainScene")
        {
            if (UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }

            var existingMainUI = FindFirstObjectByType<MainController>();
            if (existingMainUI == null)
            {
                UIManager.InitializeMainUI();
            }
            else
            {
                UIManager.AttachExistingMainUI(existingMainUI.transform.root.gameObject);
            }
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
            if (ActionPointManager == null)
            {
                ActionPointManager = FindFirstObjectByType<ActionPointManager>();
                if (ActionPointManager == null)
                {
                    var apObj = new GameObject("ActionPointManager");
                    ActionPointManager = apObj.AddComponent<ActionPointManager>();
                }
            }
            ActionPointManager.SetZero();

            if (UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }
            UIManager.InitializeGameUI();
            UIManager.UpdateActionPointUI();

            if (BoardManager == null)
            {
                BoardManager = FindFirstObjectByType<BoardManager>();
            }
            BoardManager.Initialize();

            if (ObstacleManager == null)
            {
                ObstacleManager = FindFirstObjectByType<ObstacleManager>();
            }
            ObstacleManager.Initialize();

            if (ToastManager == null)
            {
                ToastManager = FindFirstObjectByType<ToastManager>();
            }
            ToastManager.Initialize();

            if (StageManager == null)
            {
                StageManager = FindFirstObjectByType<StageManager>();
            }

            AllocateStageData();
            StageManager.StartStage();
        }

        if (newScene.name == "TutorialScene")
        {
            if (ActionPointManager == null)
            {
                ActionPointManager = FindFirstObjectByType<ActionPointManager>();
                if (ActionPointManager == null)
                {
                    var apObj = new GameObject("ActionPointManager");
                    ActionPointManager = apObj.AddComponent<ActionPointManager>();
                }
            }
            ActionPointManager.SetZero();

            if (UIManager == null)
            {
                UIManager = FindFirstObjectByType<UIManager>();
            }
            UIManager.InitializeGameUI();
            UIManager.UpdateActionPointUI();

            if (BoardManager == null)
            {
                BoardManager = FindFirstObjectByType<BoardManager>();
            }
            BoardManager.Initialize();

            if (ObstacleManager == null)
            {
                ObstacleManager = FindFirstObjectByType<ObstacleManager>();
            }
            ObstacleManager.Initialize();

            if (ToastManager == null)
            {
                ToastManager = FindFirstObjectByType<ToastManager>();
            }
            ToastManager.Initialize();

            if (StageManager == null)
            {
                StageManager = FindFirstObjectByType<StageManager>();
            }
            StageManager.StartStage();


        }
    }

    private void AllocateStageData()
    {
        StageData[] loadedStages = Resources.LoadAll<StageData>("ScriptableObjects/Stageinfo");
        if (loadedStages != null && loadedStages.Length > 0)
        {
            // 필요한 경우 stageNumber 기준으로 정렬 (정렬 기준은 StageData에 맞춰 조정)
            System.Array.Sort(loadedStages, (a, b) =>
            {
                // null 방어
                int na = a != null ? a.stageNumber : int.MaxValue;
                int nb = b != null ? b.stageNumber : int.MaxValue;
                return na.CompareTo(nb);
            });

            StageManager.Instance.stageProfiles = loadedStages;
        }
    }

    public void SetPieces(Piece[] pieces)
    {
        if (pieces == null)
        {
            Debug.LogWarning("SetPieces called with null pieces array.");
            return;
        }

        int count = Mathf.Min(selectedPieces.Length, pieces.Length);
        for (int i = 0; i < count; i++)
        {
            // Assign the selected piece reference instead of trying to access an uninitialized element.
            selectedPieces[i] = pieces[i];
        }

        // If pieces provided fewer than selectedPieces slots, clear the rest
        for (int i = count; i < selectedPieces.Length; i++)
        {
            selectedPieces[i] = null;
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