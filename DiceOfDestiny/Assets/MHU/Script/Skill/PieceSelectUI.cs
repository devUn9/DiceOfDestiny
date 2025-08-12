using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PieceSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Canvas canvas;
        
    private Dictionary<GameObject, PieceController> buttonToPieceMap = new Dictionary<GameObject, PieceController>();
    private List<GameObject> buttons = new List<GameObject>();
    private PieceController selectedPiece;
    private MoveSkillUI moveSkillUI;
    private CameraController cameraController;
    private Camera mainCamera;
    private Vector3 baseUIScale;

    private void Awake()
    {
        mainCamera = Camera.main;

        moveSkillUI = GetComponentInParent<MoveSkillUI>();
        mainCamera = Camera.main;
        cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Start()
    {
        baseUIScale = buttonPrefab.transform.localScale;
    }

    private void LateUpdate()
    {
        UpdateUIScale();
        UpdateButtonPositions(); // 버튼 위치 갱신 추가
    }

    private void UpdateUIScale()
    {
        if (cameraController == null || mainCamera == null) return;

        float baseZoom = cameraController.GetZoomLevels()[0];
        float currentZoom = mainCamera.orthographicSize;
        float scaleFactor = baseZoom / currentZoom;

        // 모든 버튼의 스케일 조정
        foreach (var button in buttons)
        {
            button.transform.localScale = baseUIScale * scaleFactor;
        }
    }

    private void UpdateButtonPositions()
    {
        if (mainCamera == null) return;

        // 각 버튼의 위치를 대응하는 기물의 화면 좌표로 갱신
        foreach (var button in buttons)
        {
            if (buttonToPieceMap.TryGetValue(button, out PieceController piece))
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(piece.transform.position);
                button.transform.position = screenPos; // 버튼 위치 업데이트
            }
        }
    }

    public void CreateButtonsForPieces()
    {
        ClearButtons();

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            if (piece == PieceManager.Instance.currentPiece) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(piece.transform.position);
            GameObject button = Instantiate(buttonPrefab, screenPos, Quaternion.identity, canvas.transform);
            buttons.Add(button);
            buttonToPieceMap.Add(button, piece); // 버튼과 기물 매핑

            Button uiButton = button.GetComponent<Button>();
            uiButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            uiButton.onClick.AddListener(() => OnPieceButtonClick(piece));
        }
    }

    public void OnPieceButtonClick(PieceController piece)
    {
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        BoardSelectManager.Instance.PieceHighLightTilesMulty(piece.gridPosition);
        PieceManager.Instance.currentPiece = piece;
        selectedPiece = piece;

        moveSkillUI.Initialize(selectedPiece);
    }

    public void ClearButtons()
    {
        foreach (var button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
        buttonToPieceMap.Clear(); // 매핑도 정리
    }

    private void OnDisable()
    {
        ClearButtons();
    }
}