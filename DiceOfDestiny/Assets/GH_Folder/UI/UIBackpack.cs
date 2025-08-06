using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBackpack : MonoBehaviour
{
    [Header("Choice Piece")]
    [SerializeField] private Button BackpackOpenCloseButton;
    [SerializeField] private GameObject UIPieceGroup;

    [Header("Choice Top Face")]
    [SerializeField] private GameObject ChoiceTopFaceWindow;
    [SerializeField] private Image SpawnPieceColorImage;
    [SerializeField] private GameObject SpawnPieceObject;


    [SerializeField] private Image[] ChoicePieceImageColorImage;
    [SerializeField] private Image[] ChoicePieceClassImage;

    private Piece currentPiece;
    private int currentIndex;

    // 전개도 데이터 (십자형: 0:바닥, 1:앞, 2:위, 3:뒤, 4:왼쪽, 5:오른쪽)
    private readonly int[] upTransition = new int[] { 1, 2, 3, 0, 4, 5 }; // 위로 이동
    private readonly int[] downTransition = new int[] { 3, 0, 1, 2, 4, 5 }; // 아래로 이동
    private readonly int[] leftTransition = new int[] { 4, 1, 5, 3, 2, 0 }; // 왼쪽으로 이동
    private readonly int[] rightTransition = new int[] { 5, 1, 4, 3, 0, 2 }; // 오른쪽으로 이동
    private readonly int[] leftRotateTransition = new int[] { 0, 5, 2, 4, 1, 3 }; // 왼쪽으로 회전
    private readonly int[] rightRotateTransition = new int[] { 0, 4, 2, 5, 3, 1 }; // 오른쪽으로 회전

    private bool isMove = false;

    private void Start()
    {
        BackpackOpenCloseButton.onClick.AddListener(onClickBackpackOpenCloseButton);

        SpawnPieceObject.GetComponent<Button>().onClick.AddListener(onClickSpawnPieceButton);

        // Refresh 함수 구독
        EventManager.Instance.AddListener("Refresh", _ => Refresh());

        // 기물 선택 UI의 기물 윗면 새로고침
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < ChoicePieceImageColorImage.Length; i++)
        {
            Debug.Log("Enter Refresh");

            currentPiece = PieceManager.Instance.pieceInventory.slots[i].GetPiece();
            if (currentPiece == null)
                return;
            Debug.Log("piece no null");
            ChoicePieceImageColorImage[i].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
            ChoicePieceClassImage[i].sprite = currentPiece.faces[2].classData.sprite;
        }
    }

    public void onClickBackpackOpenCloseButton()
    {
        UIPieceGroup.SetActive(!UIPieceGroup.activeSelf);

        if (ChoiceTopFaceWindow.activeSelf)
            ChoiceTopFaceWindow.SetActive(false);
    }

    public void onClickPieceAppearButton(int index)
    {
        Debug.Log(index + "번 피스 선택");

        // 같은 피스를 다시 클릭한 경우 창을 닫음
        if (currentIndex == index && ChoiceTopFaceWindow.activeSelf)
        {
            ChoiceTopFaceWindow.SetActive(false);
            return;
        }

        currentIndex = index;

        // 선택 피스 저장
        PieceManager.Instance.pieceInventory.selectedSlot = PieceManager.Instance.pieceInventory.slots[currentIndex];

        // 윗면 선택창 On
        if (!ChoiceTopFaceWindow.activeSelf)
            ChoiceTopFaceWindow.SetActive(true);

        currentPiece = PieceManager.Instance.pieceInventory.selectedSlot.GetPiece();

        if (currentPiece == null)
            return;

        // 기물 선택 UI의 기물 윗면 새로고침
        ChoicePieceImageColorImage[currentIndex].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        ChoicePieceClassImage[currentIndex].sprite = currentPiece.faces[2].classData.sprite;

        // 기물 윗면 선택 UI 의 기물 윗면 초기화
        SpawnPieceColorImage.color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        SpawnPieceObject.GetComponent<Image>().sprite = currentPiece.faces[2].classData.sprite;
    }

    IEnumerator SpawnPiece()
    {
        // 타일 선택 이미지 띄우기
        BoardSelectManager.Instance.StartHighlightTiles();

        // 클릭 기다림
        yield return BoardSelectManager.Instance.WaitForTileClick();

        // =====================[ 생성 시작 ]=====================

        // 위치 불러오기
        Vector2Int gridPos = BoardSelectManager.Instance.lastClickedPosition;

        if (gridPos.y > 0)
        {
            Debug.Log("기물은 첫번째 줄에만 배치 가능합니다.");
            yield break;
        }

        // 피스 생성
        GameObject piece = Instantiate(PieceManager.Instance.piecePrefabs[currentIndex],
        new Vector2(BoardManager.Instance.boardTransform.position.x + gridPos.x,
                    BoardManager.Instance.boardTransform.position.y + gridPos.y),
        Quaternion.identity);

        ChoiceTopFaceWindow.SetActive(false);

        // 현재 조작중인 기물로 초기화
        PieceController currentPieceController = piece.GetComponent<PieceController>();

        // 보드 판 내부 좌표 초기화
        currentPieceController.gridPosition = gridPos;

        // 생성된 기물 윗면 초기화
        currentPieceController.SetTopFace();

        // 생성된 위치의 타일의 피스정보를 저장
        BoardManager.Instance.Board[gridPos.x, gridPos.y].SetPiece(currentPieceController);

        // 피스 리스트에 추가
        PieceManager.Instance.Pieces.Add(currentPieceController);

        // 현재 선택 피스
        PieceManager.Instance.SetCurrentPiece(currentPieceController);

        // 피스 선택 테두리 생성
        BoardSelectManager.Instance.PieceHighlightTiles(currentPieceController.gridPosition);

        // =====================[ 생성 종료 ]=====================

        ChoicePieceClassImage[currentIndex].sprite = null;

        // 슬롯에 있는 피스 제거
        Debug.Log(currentIndex + "번 피스 제거");
        PieceManager.Instance.pieceInventory.slots[currentIndex].RemovePiece();
    }


    public void onClickSpawnPieceButton()
    {
        StopAllCoroutines();

        if (!PieceManager.Instance.pieceInventory.slots[currentIndex].IsActivePiece())
        {
            Debug.Log("해당 슬롯에 기물이 존재하지 않습니다.");
            return;
        }

        // 피스 스폰
        StartCoroutine(SpawnPiece());
    }

    public void onClickUpdateTopFace(int dir)
    {
        Face[] newFaces = new Face[6];

        // 이동 방향에 따라 faces 배열 재배치
        if (dir == 0)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[upTransition[i]];
            RotateToTopFace(Vector2Int.up);
        }
        else if (dir == 1)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[downTransition[i]];
            RotateToTopFace(Vector2Int.down);
        }
        else if (dir == 2)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[leftTransition[i]];
            RotateToTopFace(Vector2Int.left);
        }
        else if (dir == 3)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[rightTransition[i]];
            RotateToTopFace(Vector2Int.right);
        }
        else if (dir == 4)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[leftRotateTransition[i]];
        }
        else if (dir == 5)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[rightRotateTransition[i]];
        }
        else
        {
            Debug.LogWarning($"Invalid move direction: {dir}");
            return;
        }

        currentPiece.faces = newFaces;

        // 기물 선택 UI의 기물 윗면 새로고침
        ChoicePieceImageColorImage[currentIndex].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        ChoicePieceClassImage[currentIndex].sprite = currentPiece.faces[2].classData.sprite;

        // 기물 윗면 선택 UI 의 기물 윗면 초기화
        SpawnPieceColorImage.color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        SpawnPieceObject.GetComponent<Image>().sprite = currentPiece.faces[2].classData.sprite;
    }

    public void RotateToTopFace(Vector2Int moveDirection)
    {
        StartCoroutine(RotateToTopFaceCoroutine(moveDirection, SpawnPieceObject.GetComponent<Image>()));
    }

    IEnumerator RotateToTopFaceCoroutine(Vector2Int moveDirection, Image pieceImage)
    {
        if (isMove)
            yield break;

        isMove = true;

        // 1. 이동 방향에 따라 faces 배열 재배치
        int[] transition = null;
        if (moveDirection == Vector2Int.up) transition = upTransition;
        else if (moveDirection == Vector2Int.down) transition = downTransition;
        else if (moveDirection == Vector2Int.left) transition = leftTransition;
        else if (moveDirection == Vector2Int.right) transition = rightTransition;
        else yield break;

        Face[] newFaces = new Face[6];
        for (int i = 0; i < 6; i++)
            newFaces[i] = currentPiece.faces[transition[i]];
        currentPiece.faces = newFaces;

        // 2. 애니메이션 (스케일/위치 변화)
        RectTransform rect = pieceImage.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 moveVec = moveDirection * 20; // 픽셀 단위, 필요시 조정
        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            // 위치와 스케일 변화
            rect.anchoredPosition = Vector2.Lerp(startPos, startPos + moveVec, ease);
            float inflate = Mathf.Sin(ease * Mathf.PI) * 0.2f;
            rect.localScale = Vector3.one * (1f + inflate);

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        // 3. 최종 위치/스케일/이미지 적용
        rect.anchoredPosition = startPos;
        rect.localScale = Vector3.one;
        pieceImage.sprite = currentPiece.faces[2].classData.sprite;

        isMove = false;
    }
}
