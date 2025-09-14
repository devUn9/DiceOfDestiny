using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public sealed class TutorialS1Director : MonoBehaviour
{
    [Header("External")]
    [SerializeField] private TutorialInputRouter inputRouter;
    [SerializeField] private TutorialHighlightController highlight;
    [SerializeField] private SceneRouter sceneRouter;

    [Header("UI")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Toggle skipToggle;

    [Header("Targets / Focus")]
    [SerializeField] private RectTransform rollDiceButton;
    [SerializeField] private RectTransform boardFocus;

    [Header("Placement UI (Inspector Optional)")]
    [SerializeField] private Button[] pieceButtons;          // 인스펙터 미지정 허용(런타임 자동 연결)
    [SerializeField] private RectTransform pointer;          // 허용 타일 위 포인터

    [Header("Board / Camera")]
    [SerializeField] private Transform boardOrigin;          // 비워두면 BoardManager.Instance.boardTransform 사용
    [SerializeField] private Camera mainCamera;              // 비워두면 Camera.main

    [Header("Placement Region (Inclusive)")]
    [SerializeField] private int minX = 0;
    [SerializeField] private int maxX = 13;
    [SerializeField] private int minY = 0;                   // 경계 회피를 위해 1 권장
    [SerializeField] private int maxY = 0;
    [SerializeField] private bool clampToBoardBounds = true;

    [Header("Auto Progress")]
    [SerializeField] private bool autoNextOnPlacement = true;
    [SerializeField] private bool autoNextOnDice = true;
    [SerializeField] private bool autoNextOnMove = true;

    [Header("Runtime UI Resolve")]
    [SerializeField] private bool resolveUIAtRuntime = true;
    [SerializeField] private string gameUiRootName = "GameUI(Clone)";
    [SerializeField] private string backpackPath = "BackpackUI/InventoryPanel/ChoiceTopFace/TopFace/SpawnPieceColor";
    [SerializeField] private string diceButtonName = "DiceRollButton";

    private int stepIndex = -1;
    private int selectedPieceIndex = -1;
    private bool isPlaced;
    private bool uiResolved;
    private bool wiringDone;
    private Transform gameUiRoot;
    private Vector2Int hoverGridPos;

    // Guard to prevent NextStep running multiple times from duplicate listeners
    private bool isAdvancingStep = false;

    private void Awake()
    {
        if (boardOrigin == null && BoardManager.Instance != null)
        {
            boardOrigin = BoardManager.Instance.boardTransform;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        AllocateStageData();
    }


    private void AllocateStageData()
    {
        StageData[] loadedStages = Resources.LoadAll<StageData>("ScriptableObjects/Stageinfo/Tutorial");
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

    private void OnEnable()
    {
        if (nextButton != null)
        {
            // Remove before add to avoid duplicate bindings (inspector + runtime)
            nextButton.onClick.RemoveListener(NextStep);
            nextButton.onClick.AddListener(NextStep);
        }

        stepIndex = -1;
        StartCoroutine(ResolveGameUIRoutine()); // GameUI(clone) 생성 대기 + 자동 바인딩
        NextStep();                             // 0단계(인트로) 진입
    }

    private void OnDisable()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(NextStep);
        UnwirePieceButtons();
    }

    private void Update()
    {
        // 배치 단계에서만 포인터/클릭 처리
        if (stepIndex == 1 && selectedPieceIndex >= 0 && !isPlaced)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (TryScreenToGrid(Input.mousePosition, out var gridPos))
            {
                hoverGridPos = gridPos;
                UpdatePointerPosition(gridPos);

                if (Input.GetMouseButtonDown(0))
                {
                    TryPlaceAt(gridPos);
                }
            }
            else
            {
                SetPointerActive(false);
            }
        }
    }

    public void NextStep()
    {
        // Prevent duplicate/reentrant calls (e.g., duplicated button listeners)
        if (isAdvancingStep) return;
        isAdvancingStep = true;
        StartCoroutine(ResetAdvancingFlag());

        // 튜토리얼 종료/씬 이동 시 보드와 기물 클리어
        if (stepIndex == 4 || stepIndex > 4)
        {
            ClearTutorialBoardAndPieces();
        }

        // 공통 리셋
        highlight?.Clear();
        if (nextButton != null) nextButton.interactable = true;
        inputRouter?.SetBlock(false);
        SetPointerActive(false);

        stepIndex++;

        switch (stepIndex)
        {
            case 0:
                inputRouter?.SetBlock(true);
                SetText("튜토리얼을 시작합니다.\n다음을 눌러 진행해 주세요.");
                break;

            case 1: // 배치 단계
                if (resolveUIAtRuntime && !uiResolved)
                {
                    SetText("인벤토리를 준비하는 중입니다...");
                    if (nextButton != null) nextButton.interactable = false;
                    StartCoroutine(WaitUntilUIResolvedThen(() =>
                    {
                        PreparePlacementPhase();             // UI 준비 끝나면 바로 배치 단계 구성
                        if (nextButton != null) nextButton.interactable = true;
                    }));
                }
                else
                {
                    PreparePlacementPhase();
                }
                break;

            case 2: // 주사위 단계
                SetText("주사위를 굴려 행동력을 얻으세요.");
                EnsureDiceRollButton();
                highlight?.Focus(rollDiceButton);
                break;

            case 3: // 이동 단계
                SetText("기물을 선택하여 이동하세요.");
                highlight?.Focus(boardFocus);
                break;

            case 4: // 완료
                inputRouter?.SetBlock(true);
                SetText("잘하셨습니다.\n이것으로 튜토리얼을 마칩니다.");
                highlight?.Clear();
                GameObject n = nextButton.gameObject; 
                    n.SetActive(true);
                break;

            default:
                sceneRouter?.LoadNext();
                break;
        }
    }

    private IEnumerator ResetAdvancingFlag()
    {
        // reset next frame to avoid reentrancy from same event
        yield return null;
        isAdvancingStep = false;
    }

    // ===== OnClick(Inspector 연결용) =====
    public void OnClick_Next()
    {
        NextStep();
    }

    public void OnClick_SelectPiece(int index)
    {
        SelectPiece(index);
    }

    public void OnClick_RollDice()
    {
        if (nextButton != null) nextButton.interactable = false;
        // 실제 TryRoll()은 주사위 버튼의 onClick에 연결하십시오.
    }

    // ===== 진행 콜백 =====
    public void OnDiceRolled()
    {
        if (stepIndex == 2)
            if (autoNextOnDice) NextStep();
        else if (nextButton != null) nextButton.interactable = true;
    }

    public void OnTileMoved()
    {
        if (stepIndex == 3)
            if (autoNextOnMove) NextStep();
        else if (nextButton != null) nextButton.interactable = true;
    }

    // ===== 배치 단계 구성 =====
    private void PreparePlacementPhase()
    {
        selectedPieceIndex = -1;
        isPlaced = false;
        SetPointerActive(false);
        GameObject g = nextButton.gameObject; 
            g.SetActive(false);

        if (clampToBoardBounds) ClampPlacementRectToBoard();
        SetText("가방을 열어 배치할 기물을 선택한 뒤, 타일 첫번째 줄을 클릭해 배치해 주세요.");
        highlight?.Focus(boardFocus);

        // 런타임 슬롯 버튼으로 교체 와이어링
        // if (resolveUIAtRuntime && uiResolved && !wiringDone)
        //{
        //    var slots = TryGetInventorySlotButtons();
        //    if (slots != null && slots.Count > 0)
        //    {
        //        ReplacePieceButtons(slots.ToArray());
        //        wiringDone = true;
        //    }
        //}
    }

    private void ReplacePieceButtons(Button[] newButtons)
    {
        UnwirePieceButtons();
        pieceButtons = newButtons;
        WirePieceButtons();
    }

    private void WirePieceButtons()
    {
        if (pieceButtons == null || pieceButtons.Length == 0) return;
        for (int i = 0; i < pieceButtons.Length; i++)
        {
            var btn = pieceButtons[i];
            if (btn == null) continue;
            int captured = i;
            //btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectPiece(captured));
        }
    }

    private void UnwirePieceButtons()
    {
        if (pieceButtons == null || pieceButtons.Length == 0) return;
        for (int i = 0; i < pieceButtons.Length; i++)
        {
            var btn = pieceButtons[i];
            if (btn == null) continue;
            //btn.onClick.RemoveAllListeners();
        }
    }

    private void SelectPiece(int index)
    {
        if (PieceManager.Instance == null || PieceManager.Instance.piecePrefabs == null)
        {
            Debug.LogWarning("[Tutorial] PieceManager 또는 piecePrefabs을 찾을 수 없습니다.");
            return;
        }

        if (index < 0 || index >= PieceManager.Instance.piecePrefabs.Length)
        {
            Debug.LogWarning("[Tutorial] 유효하지 않은 piece index.");
            return;
        }

        selectedPieceIndex = index;
        SetText("타일 첫번째 줄을 클릭해 배치해 주세요.");
        //SetPointerActive(true);
    }

    private void TryPlaceAt(Vector2Int gridPos)
    {
        if (!IsWithinAllowedRect(gridPos)) return;
        if (!IsWithinBoardBounds(gridPos)) return;
        if (IsOccupied(gridPos)) return;

        if (PieceManager.Instance == null)
        {
            Debug.LogError("[Tutorial] PieceManager.Instance is null.");
            return;
        }

        PieceManager.Instance.GeneratePiece(selectedPieceIndex, gridPos);
        isPlaced = true;
        SetText("배치가 완료되었습니다. 다음으로 진행해 주세요.");
        SetPointerActive(false);
    }

    // ===== 좌표/포인터 =====
    private bool TryScreenToGrid(Vector3 screenPos, out Vector2Int gridPos)
    {
        gridPos = default;
        if (mainCamera == null || boardOrigin == null) return false;

        var world = mainCamera.ScreenToWorldPoint(screenPos);
        var local = world - boardOrigin.position;
        int gx = Mathf.RoundToInt(local.x);
        int gy = Mathf.RoundToInt(local.y);
        gridPos = new Vector2Int(gx, gy);
        return true;
    }

    private void UpdatePointerPosition(Vector2Int gridPos)
    {
        if (pointer == null || mainCamera == null || boardOrigin == null)
        {
            SetPointerActive(false);
            return;
        }

        var world = (Vector2)boardOrigin.position + (Vector2)gridPos;
        var screen = mainCamera.WorldToScreenPoint(world);
        pointer.position = screen;

        bool active = IsWithinBoardBounds(gridPos) && IsWithinAllowedRect(gridPos) && !IsOccupied(gridPos);
        SetPointerActive(active);
    }

    private void SetPointerActive(bool active)
    {
        if (pointer == null) return;
        if (pointer.gameObject.activeSelf != active) pointer.gameObject.SetActive(active);
    }

    // ===== 유효성 검사 =====
    private void SetText(string msg)
    {
        if (tutorialText != null) tutorialText.text = msg;
    }

    private bool IsWithinAllowedRect(Vector2Int pos)
    {
        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }

    private bool IsWithinBoardBounds(Vector2Int pos)
    {
        if (BoardManager.Instance == null || BoardManager.Instance.Board == null) return true;

        int w = BoardManager.Instance.Board.GetLength(0);
        int h = BoardManager.Instance.Board.GetLength(1);
        return pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h;
    }

    private bool IsOccupied(Vector2Int pos)
    {
        if (PieceManager.Instance == null) return false;
        var list = PieceManager.Instance.Pieces;
        if (list == null || list.Count == 0) return false;
        return list.Any(p => p != null && p.gridPosition == pos);
    }

    private void ClampPlacementRectToBoard()
    {
        if (BoardManager.Instance == null || BoardManager.Instance.Board == null) return;

        int w = BoardManager.Instance.Board.GetLength(0);
        int h = BoardManager.Instance.Board.GetLength(1);

        minX = Mathf.Clamp(minX, 0, w - 1);
        maxX = Mathf.Clamp(maxX, 0, w - 1);
        minY = Mathf.Clamp(minY, 0, h - 1);
        maxY = Mathf.Clamp(maxY, 0, h - 1);

        if (minX > maxX) (minX, maxX) = (maxX, minX);
        if (minY > maxY) (minY, maxY) = (maxY, minY);
    }

    // ===== 런타임 UI 해석 =====
    private IEnumerator ResolveGameUIRoutine()
    {
        if (!resolveUIAtRuntime)
        {
            uiResolved = true;
            yield break;
        }

        // 1) GameUI 루트 탐색
        while (gameUiRoot == null)
        {
            var go = GameObject.Find(gameUiRootName);
            if (go != null) gameUiRoot = go.transform;
            if (gameUiRoot == null) yield return null;
        }

        // 2) 인벤토리 슬롯 버튼 수집
        var slots = TryGetInventorySlotButtons();
        if (slots != null && slots.Count > 0)
        {
            ReplacePieceButtons(slots.ToArray());
            wiringDone = true;
        }

        // 3) 주사위 버튼 보정
        EnsureDiceRollButton();

        uiResolved = true;
    }

    private List<Button> TryGetInventorySlotButtons()
    {
        if (gameUiRoot == null) return null;

        var group = FindDeepChild(gameUiRoot, backpackPath);
        if (group == null) return null;

        var buttons = new List<Button>();
        for (int i = 0; i < group.childCount; i++)
        {
            var child = group.GetChild(i);
            var btn = child.GetComponentInChildren<Button>(true);
            if (btn != null) buttons.Add(btn);
        }
        return buttons;
    }

    private void EnsureDiceRollButton()
    {
        if (rollDiceButton != null) return;

        if (gameUiRoot != null)
        {
            var t = FindDeepChild(gameUiRoot, diceButtonName);
            if (t != null) rollDiceButton = t.GetComponent<RectTransform>();
        }
        if (rollDiceButton == null)
        {
            var obj = GameObject.Find(diceButtonName);
            if (obj != null) rollDiceButton = obj.GetComponent<RectTransform>();
        }
    }

    private static Transform FindDeepChild(Transform root, string pathOrName)
    {
        // "A/B/C" 경로 우선
        if (pathOrName.Contains("/"))
        {
            var current = root;
            var parts = pathOrName.Split('/');
            foreach (var p in parts)
            {
                current = current.Find(p);
                if (current == null) return null;
            }
            return current;
        }

        // 이름 단일 검색(BFS)
        var q = new Queue<Transform>();
        q.Enqueue(root);
        while (q.Count > 0)
        {
            var t = q.Dequeue();
            if (t.name == pathOrName) return t;
            for (int i = 0; i < t.childCount; i++) q.Enqueue(t.GetChild(i));
        }
        return null;
    }

    private IEnumerator WaitUntilUIResolvedThen(System.Action onReady)
    {
        while (!uiResolved) yield return null;
        onReady?.Invoke();
    }

    public void ClearTutorialBoardAndPieces()
    {
        // 튜토리얼 종료/씬 이동 시 보드와 기물 모두 클리어
        BoardManager.Instance?.ClearBoard();
        PieceManager.Instance?.ClearPieces();
        ObstacleManager.Instance?.RemoveAllObstacle();
        BoardSelectManager.Instance?.ClearAllEffects();
        ToastManager.Instance?.ClearAllToasts(); // 토스트 UI도 클리어
    }
}
