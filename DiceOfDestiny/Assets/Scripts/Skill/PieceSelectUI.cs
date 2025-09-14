using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject pieceSelectButton; // MoveSkill용 버튼 프리팹

    [SerializeField] private Canvas canvas;

    private Dictionary<GameObject, PieceController> buttonToPieceMap = new Dictionary<GameObject, PieceController>();
    private List<GameObject> buttons = new List<GameObject>();
    private PieceController selectedPiece;
    private MoveSkillUI moveSkillUI;
    private CameraController cameraController;
    private Camera mainCamera;
    private Vector3 baseUIScale;

    public PieceController firstSelectedPiece = null;
    public bool isPieceSelected = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        moveSkillUI = GetComponentInParent<MoveSkillUI>();
        cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Start()
    {
        baseUIScale = pieceSelectButton.transform.localScale; // Move 버튼 기준으로 스케일 초기화
    }

    private void LateUpdate()
    {
        UpdateUIScale();
        UpdateButtonPositions();
    }

    private void UpdateUIScale()
    {
        if (cameraController == null || mainCamera == null) return;

        float baseZoom = cameraController.GetZoomLevels()[0];
        float currentZoom = mainCamera.orthographicSize;
        float scaleFactor = baseZoom / currentZoom;

        foreach (var button in buttons)
        {
            button.transform.localScale = baseUIScale * scaleFactor;
        }
    }

    private void UpdateButtonPositions()
    {
        if (mainCamera == null) return;

        foreach (var button in buttons)
        {
            if (buttonToPieceMap.TryGetValue(button, out PieceController piece))
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(piece.transform.position);
                button.transform.position = screenPos;
            }
        }
    }

    // MoveSkill용 버튼 생성
    public void CreateButtonsForMoveSkill()
    {
        ClearButtons();

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            if (piece == PieceManager.Instance.currentPiece) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(piece.transform.position);
            GameObject button = Instantiate(pieceSelectButton, screenPos, Quaternion.identity, canvas.transform);
            buttons.Add(button);
            buttonToPieceMap.Add(button, piece);

            Button uiButton = button.GetComponent<Button>();
            uiButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            uiButton.onClick.AddListener(() => OnPieceButtonClickMove(piece));
        }
    }

    // SwapSkill용 버튼 생성
    public void CreateButtonsForSwapSkill()
    {
        ClearButtons();

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            //if (piece == PieceManager.Instance.currentPiece) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(piece.transform.position);
            GameObject button = Instantiate(pieceSelectButton, screenPos, Quaternion.identity, canvas.transform);
            buttons.Add(button);
            buttonToPieceMap.Add(button, piece);

            Button uiButton = button.GetComponent<Button>();
            uiButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            uiButton.onClick.AddListener(() => OnPieceButtonClickSwap(piece));
        }
    }

    // MoveSkill용 버튼 클릭 처리
    public void OnPieceButtonClickMove(PieceController piece)
    {
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        BoardSelectManager.Instance.PieceHighLightTilesMulty(piece.gridPosition);
        selectedPiece = piece;
        PieceManager.Instance.currentPiece = piece;

        // MoveSkill 관련 초기화
        moveSkillUI.Initialize(selectedPiece);
    }

    // SwapSkill용 버튼 클릭 처리
    public void OnPieceButtonClickSwap(PieceController piece)
    {
        //BoardSelectManager.Instance.DestroyPieceHighlightTile();
        //BoardSelectManager.Instance.PieceHighLightTilesMulty(piece.gridPosition);
        selectedPiece = piece;
        PieceManager.Instance.currentPiece = piece;

        if (firstSelectedPiece == null)
        {
            // 첫 번째 기물 선택
            firstSelectedPiece = selectedPiece;
            Debug.Log($"First piece selected: {selectedPiece.name}");
        }
        else if (firstSelectedPiece != selectedPiece)
        {
            // 두 번째 기물 선택
            PieceManager.Instance.currentPiece = selectedPiece;
            isPieceSelected = true;
            Debug.Log($"Second piece selected: {selectedPiece.name}");
        }
    }

    // SwapSkill용 대기 코루틴
    public IEnumerator WaitForArrowClick(PieceController originalPiece)
    {
        isPieceSelected = false;
        firstSelectedPiece = null;

        while (!isPieceSelected)
        {
            yield return null;
        }
    }

    public void ClearButtons()
    {
        foreach (var button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
        buttonToPieceMap.Clear();
    }

    private void OnDisable()
    {
        ClearButtons();
    }
}