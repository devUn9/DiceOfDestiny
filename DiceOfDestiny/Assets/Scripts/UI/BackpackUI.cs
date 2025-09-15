using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button backpackOpenCloseButton;
    [SerializeField] private Image backpackButtonImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clickedSprite;

    private bool isClicked = false;
    private bool oneChance = true;

    [Header("Choice Piece")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Choice Top Face")]
    [SerializeField] private GameObject choiceTopFaceWindow;

    [SerializeField] private Image spawnPieceColorImage;
    [SerializeField] private GameObject spawnPieceObject;
    [SerializeField] private Image nextSpawnPieceImage;
    [SerializeField] private GameObject nextSpawnPieceObject;


    [SerializeField] private Image[] choicePieceImageColorImage;
    [SerializeField] private Image[] choicePieceClassImage;

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
    //private bool isChoice = false;

    private void Start()
    {
        backpackOpenCloseButton.onClick.AddListener(onClickBackpackOpenCloseButton);
        spawnPieceObject.GetComponent<Button>().onClick.AddListener(onClickSpawnPieceButton);

        // Refresh 함수 구독
        EventManager.Instance.AddListener("Refresh", _ => Refresh());

        // 게임 시작 시 가방 열기
        //if (StageManager.Instance.currentStage.stageNumber == 1)
        //{
        //    onClickBackpackOpenCloseButton();
        //}

        // 기물 선택 UI의 기물 윗면 새로고침
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < choicePieceImageColorImage.Length; i++)
        {
            Debug.Log("Enter Refresh");

            currentPiece = PieceManager.Instance.pieceDatas[i];
            if (currentPiece == null)
            {
                spawnPieceColorImage.color = Color.white;
                spawnPieceObject.GetComponent<Image>().sprite = null;
                continue;
            }

            choicePieceImageColorImage[i].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
            choicePieceClassImage[i].sprite = currentPiece.faces[2].classData.sprite;
        }
    }

    public void onClickBackpackOpenCloseButton()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        isClicked = !isClicked;
        backpackButtonImage.sprite = isClicked ? clickedSprite : defaultSprite;


        if (choiceTopFaceWindow.activeSelf)
            choiceTopFaceWindow.SetActive(false);
    }

    public void onClickPieceAppearButton(int index)
    {
        if (PieceManager.Instance.pieceDatas[index] == null)
        {
            Debug.Log("해당 슬롯에 기물이 존재하지 않습니다.");
            return;
        }

        // 같은 피스를 다시 클릭한 경우 창을 닫음
        if (currentIndex == index && choiceTopFaceWindow.activeSelf)
        {
            choiceTopFaceWindow.SetActive(false);
            return;
        }

        currentIndex = index;
        currentPiece = PieceManager.Instance.pieceDatas[currentIndex];

        // 윗면 선택창 On
        if (!choiceTopFaceWindow.activeSelf)
            choiceTopFaceWindow.SetActive(true);

        // 기물 선택 UI의 기물 윗면 새로고침
        choicePieceImageColorImage[currentIndex].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        choicePieceClassImage[currentIndex].sprite = currentPiece.faces[2].classData.sprite;

        // 기물 윗면 선택 UI 의 기물 윗면 초기화
        spawnPieceColorImage.color = BoardManager.Instance.GetColor(currentPiece.faces[2].color);
        spawnPieceObject.GetComponent<Image>().sprite = currentPiece.faces[2].classData.sprite;

        BoardSelectManager.Instance.restrictYBoundaries = false;
    }

    IEnumerator SpawnPiece()
    {
        // 타일 선택 이미지 띄우기
        BoardSelectManager.Instance.StartHighlightTiles();

        // 클릭 기다림
        yield return BoardSelectManager.Instance.WaitForTileClick();

        if (BoardSelectManager.Instance.lastClickedPosition == new Vector2Int(-1, -1))
            yield break;

        // =====================[ 생성 시작 ]=====================
        if (oneChance)
        {
            BoardSelectManager.Instance.restrictYBoundaries = true;
            oneChance = false;
        }

        else if (ActionPointManager.Instance.CanUse(1))
        {
            ActionPointManager.Instance.RemoveAP(1);
            BoardSelectManager.Instance.restrictYBoundaries = true;
        }

        else
        {
            //UI 처리
            yield break;

        }

        // 위치 불러오기
        Vector2Int gridPos = BoardSelectManager.Instance.lastClickedPosition;

        if (gridPos.y > 0)
        {
            Debug.Log("기물은 첫번째 줄에만 배치 가능합니다.");
            yield break;
        }

        // 피스 선택 UI의 기물 윗면 새로고침
        choiceTopFaceWindow.SetActive(false);
        choicePieceClassImage[currentIndex].sprite = null;

        // 피스 생성
        PieceManager.Instance.GeneratePiece(currentIndex, gridPos);

        // 슬롯에 있는 피스 제거
        //Debug.Log(currentIndex + "번 피스 제거");
        PieceManager.Instance.pieceDatas[currentIndex] = null;

        FindAnyObjectByType<TutorialS1Director>()?.NextStep();
    }

    // 기물 스폰 윗면 누른걸로 설치
    public void onClickSpawnPieceButton()
    {
        StopCoroutine(SpawnPiece());

        if (PieceManager.Instance.pieceDatas[currentIndex] == null)
        {
            Debug.Log("해당 슬롯에 기물이 존재하지 않습니다.");
            return;
        }

        if (BoardSelectManager.Instance.isWaitingForClick)
        {
            Debug.Log("클릭 대기 종료");
            BoardSelectManager.Instance.ClearAllEffects();
            BoardSelectManager.Instance.isWaitingForClick = false;
            return;
        }

        // 피스 스폰
        StartCoroutine(SpawnPiece());
    }

    public void onClickUpdateTopFace(int dir)
    {
        if (isMove)
            return;

        Face[] newFaces = new Face[6];

        // 이동 방향에 따라 faces 배열 재배치
        if (dir == 0)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[upTransition[i]];
            currentPiece.faces = newFaces;
            RotateToTopFace(Vector2Int.up);
        }
        else if (dir == 1)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[downTransition[i]];
            currentPiece.faces = newFaces;
            RotateToTopFace(Vector2Int.down);
        }
        else if (dir == 2)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[leftTransition[i]];
            currentPiece.faces = newFaces;
            RotateToTopFace(Vector2Int.left);
        }
        else if (dir == 3)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = currentPiece.faces[rightTransition[i]];
            currentPiece.faces = newFaces;
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


    }

    public void RotateToTopFace(Vector2Int moveDirection)
    {
        StartCoroutine(RotateToTopFaceCoroutine(moveDirection));
    }

    IEnumerator RotateToTopFaceCoroutine(Vector2Int moveDirection)
    {
        if (isMove) yield break;
        isMove = true;

        // 2. 방향 및 기본 변수 설정
        bool isHorizontalMove = (moveDirection == Vector2Int.left || moveDirection == Vector2Int.right);
        Vector3 moveDir = new Vector3(-moveDirection.x, -moveDirection.y, 0f);

        Transform parentTransform = spawnPieceColorImage.transform.parent;
        Vector3 parentStartPos = parentTransform.localPosition;

        Vector3 contractStartPos = spawnPieceColorImage.transform.localPosition;
        Vector3 expandStartPos = nextSpawnPieceImage.transform.localPosition;

        // 3. 다음 면 스프라이트 & 색상 세팅 (expand 쪽)        
        nextSpawnPieceImage.color = BoardManager.Instance.GetColor(currentPiece.faces[2].color);
        nextSpawnPieceObject.GetComponent<Image>().sprite = currentPiece.faces[2].classData.sprite;

        // 4. 초기 스케일 설정
        spawnPieceColorImage.transform.localScale = Vector3.one;
        nextSpawnPieceImage.transform.localScale = isHorizontalMove ? new Vector3(0f, 1f, 1f) : new Vector3(1f, 0f, 1f);

        float duration = 0.3f;
        float time = 0f;
        float inflatedamount = 0.2f;

        while (time < duration)
        {
            float t = time / duration;

            float totalScale = 1f + inflatedamount * Mathf.Sin(t * Mathf.PI); // 1 ~ 1.2 ~ 1 사이 변동

            float scaleContract = Mathf.Lerp(1f, 0f, t);
            float scaleExpand = totalScale - scaleContract;

            // 5. 스케일 적용
            if (isHorizontalMove)
            {
                nextSpawnPieceImage.transform.localScale = new Vector3(scaleExpand, 1f, 1f);
                spawnPieceColorImage.transform.localScale = new Vector3(scaleContract, 1f, 1f);
            }
            else
            {
                nextSpawnPieceImage.transform.localScale = new Vector3(1f, scaleExpand, 1f);
                spawnPieceColorImage.transform.localScale = new Vector3(1f, scaleContract, 1f);
            }

            // 6. 각 면 크기 반영한 반지름 계산
            float expandHalfSize = (isHorizontalMove
                ? nextSpawnPieceImage.rectTransform.rect.width
                : nextSpawnPieceImage.rectTransform.rect.height) * 0.5f * scaleExpand;

            float contractHalfSize = (isHorizontalMove
                ? spawnPieceColorImage.rectTransform.rect.width
                : spawnPieceColorImage.rectTransform.rect.height) * 0.5f * scaleContract;

            // 7. 두 면 중심 간격
            float separation = expandHalfSize + contractHalfSize;

            // 8. 면 위치 조정 — contract는 moveDir 반대 방향으로, expand는 moveDir 방향으로 이동
            nextSpawnPieceImage.transform.localPosition = expandStartPos + moveDir * separation * 0.5f;
            spawnPieceColorImage.transform.localPosition = contractStartPos - moveDir * separation * 0.5f;

            // 9. 부모는 반대 방향으로 이동해 전체 제자리 회전 효과
            Vector3 targetParentPos = parentStartPos - moveDir; // 이동 목표 위치
            parentTransform.localPosition = Vector3.Lerp(parentStartPos, targetParentPos, t);

            time += Time.deltaTime;
            yield return null;
        }

        // 10. 최종 상태 정리
        nextSpawnPieceImage.transform.localScale = Vector3.one;
        nextSpawnPieceImage.transform.localPosition = expandStartPos;

        spawnPieceColorImage.transform.localScale = isHorizontalMove ? new Vector3(0f, 1f, 1f) : new Vector3(1f, 0f, 1f);
        spawnPieceColorImage.transform.localPosition = contractStartPos;

        parentTransform.localPosition = parentStartPos;

        spawnPieceObject.GetComponent<Image>().sprite = nextSpawnPieceObject.GetComponent<Image>().sprite;
        spawnPieceColorImage.color = nextSpawnPieceImage.color;
        spawnPieceColorImage.transform.localPosition = nextSpawnPieceImage.transform.localPosition;
        spawnPieceColorImage.transform.localScale = nextSpawnPieceImage.transform.localScale;

        nextSpawnPieceImage.transform.localScale = Vector3.zero;

        // 기물 선택 UI의 기물 윗면 새로고침
        choicePieceImageColorImage[currentIndex].color = BoardManager.Instance.tileColors[(int)currentPiece.faces[2].color];
        choicePieceClassImage[currentIndex].sprite = currentPiece.faces[2].classData.sprite;

        isMove = false;
    }
}
