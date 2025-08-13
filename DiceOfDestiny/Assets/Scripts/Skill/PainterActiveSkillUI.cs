using UnityEngine;
using UnityEngine.UI;

public class PainterActiveSkillUI : MonoBehaviour
{
    [Header("팔레트 색 버튼 6개")]
    [SerializeField] private Button redButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button purpleButton;
    [SerializeField] private Button brownButton;

    [Header("팔레트 이미지")]
    [SerializeField] private Image paletteImage;

    private Vector2 offset = new Vector2(0f, 0f); // 타겟 기준 UI 오프셋 (화면 좌표)

    private TileColor selectedColor = TileColor.None;

    private Transform target; // 따라갈 타겟(마지막 클릭 타일)의 Transform
    private Camera mainCamera;
    private Canvas canvas;
    private CameraController cameraController;
    private Vector3 baseUIScale;

    // 선택된 색상을 외부에서 가져갈 수 있는 getter
    public TileColor SelectedColor
    {
        get { return selectedColor; }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        canvas = paletteImage.GetComponentInParent<Canvas>();
        cameraController = mainCamera.GetComponent<CameraController>();
    }
    void Start()
    {
        // UI 요소의 기본 스케일 저장
        baseUIScale = paletteImage.transform.localScale;

        AssignButtonColors();

        // 버튼에 리스너 추가
        if (redButton != null) redButton.onClick.AddListener(OnRedButtonClicked);
        if (greenButton != null) greenButton.onClick.AddListener(OnGreenButtonClicked);
        if (blueButton != null) blueButton.onClick.AddListener(OnBlueButtonClicked);
        if (yellowButton != null) yellowButton.onClick.AddListener(OnYellowButtonClicked);
        if (purpleButton != null) purpleButton.onClick.AddListener(OnPurpleButtonClicked);
        if (brownButton != null) brownButton.onClick.AddListener(OnBrownButtonClicked);
    }

    void LateUpdate()
    {
        if (target == null || paletteImage == null || !paletteImage.gameObject.activeSelf) return;

        // 타겟의 월드 좌표를 화면 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // 캔버스 스케일 고려
        Vector2 canvasScale = canvas.GetComponent<RectTransform>().localScale;
        Vector2 adjustedPos = new Vector2(screenPos.x / canvasScale.x, screenPos.y / canvasScale.y);

        // 오프셋 적용
        adjustedPos += offset;

        // UI 위치 업데이트
        paletteImage.GetComponent<RectTransform>().position = adjustedPos;

        // UI 스케일 업데이트
        UpdateUIScale();
    }
    private void UpdateUIScale()
    {
        if (cameraController == null) return;

        // 카메라의 현재 orthographicSize를 기준으로 스케일 계산
        float baseZoom = cameraController.GetZoomLevels()[0]; // 기본 줌 레벨 (예: 7f)
        float currentZoom = mainCamera.orthographicSize; // 현재 줌 레벨
        float scaleFactor = baseZoom / currentZoom; // 기본 줌 대비 스케일 비율

        // UI 요소의 스케일 조정
        paletteImage.transform.localScale = baseUIScale * scaleFactor;
    }

    private void AssignButtonColors()
    {
        if (redButton != null && BoardManager.Instance.tileColors.Length > 0)
            redButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[0]; // 빨강
        if (greenButton != null && BoardManager.Instance.tileColors.Length > 1)
            greenButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[1]; // 초록
        if (blueButton != null && BoardManager.Instance.tileColors.Length > 2)
            blueButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[2]; // 파랑
        if (yellowButton != null && BoardManager.Instance.tileColors.Length > 3)
            yellowButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[3]; // 노랑
        if (purpleButton != null && BoardManager.Instance.tileColors.Length > 4)
            purpleButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[4]; // 보라
        if (brownButton != null && BoardManager.Instance.tileColors.Length > 5)
            brownButton.GetComponent<Image>().color = BoardManager.Instance.tileColors[5]; // 회색
    }

    public void ShowPalette()
    {
        // 마지막 클릭한 타일의 Transform을 타겟으로 설정
        Vector2Int selectPos = BoardSelectManager.Instance.lastClickedPosition;
        target = BoardSelectManager.Instance.GetClickedTileTransform(); // BoardManager에 타일 Transform을 반환하는 메서드 필요
        if (target == null) return;

        paletteImage.gameObject.SetActive(true); // UI 활성화
    }

    // 각 버튼 클릭 시 호출되는 public 함수
    public void OnRedButtonClicked()
    {
        selectedColor = TileColor.Red;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    public void OnGreenButtonClicked()
    {
        selectedColor = TileColor.Green;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    public void OnBlueButtonClicked()
    {
        selectedColor = TileColor.Blue;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    public void OnYellowButtonClicked()
    {
        selectedColor = TileColor.Yellow;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    public void OnPurpleButtonClicked()
    {
        selectedColor = TileColor.Purple;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    public void OnBrownButtonClicked()
    {
        selectedColor = TileColor.Brown;
        paletteImage.gameObject.SetActive(false); // UI 비활성화
    }

    // UI가 비활성화될 때 선택된 색상 초기화
    public void OnDisable()
    {
        selectedColor = TileColor.None;
    }
}