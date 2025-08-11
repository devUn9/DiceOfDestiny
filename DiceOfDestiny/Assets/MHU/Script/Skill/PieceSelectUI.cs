using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PieceSelectUI : MonoBehaviour
{
    // 투명 버튼 프리팹 (에디터에서 지정)
    [SerializeField] private GameObject buttonPrefab;
    // 생성된 버튼들을 저장하는 리스트
    private List<GameObject> buttons = new List<GameObject>();
    // 현재 선택된 기물
    private PieceController selectedPiece;
    // 캔버스 (버튼을 생성할 부모 객체, 에디터에서 지정)
    [SerializeField] private Canvas canvas;

    private MoveSkillUI moveSkillUI;

    private bool isCompleted = false;

    private void Awake()
    {
        moveSkillUI = GetComponentInParent<MoveSkillUI>();
    }

    // 버튼 생성 및 초기화
    public void CreateButtonsForPieces()
    {
        // 기존 버튼 제거
        ClearButtons();

        // 본인을 제외한 모든 기물에 대해 버튼 생성
        foreach (var piece in PieceManager.Instance.Pieces)
        {
            if (piece == PieceManager.Instance.currentPiece) // 본인(아기) 제외
                continue;

            // 기물의 화면 좌표로 버튼 위치 계산
            Vector3 screenPos = Camera.main.WorldToScreenPoint(piece.transform.position);
            // 버튼 생성
            GameObject button = Instantiate(buttonPrefab, screenPos, Quaternion.identity, canvas.transform);
            buttons.Add(button);

            // 버튼의 PieceController 참조 저장
            Button uiButton = button.GetComponent<Button>();
            uiButton.GetComponent<Image>().color = new Color(1, 1, 1, 0f); // 투명도 설정
            uiButton.onClick.AddListener(() => OnPieceButtonClick(piece));
        }
    }

    // 버튼 클릭 시 호출
    public void OnPieceButtonClick(PieceController piece)
    {
        // 이전 하이라이트 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        // 선택된 기물만 하이라이트
        BoardSelectManager.Instance.PieceHighLightTilesMulty(piece.gridPosition);
        PieceManager.Instance.currentPiece = piece; // 현재 기물 설정
        selectedPiece = piece;

        //isCompleted = true;

        //ClearButtons();
        moveSkillUI.Initialize(selectedPiece);

    }

    // 버튼 제거
    public void ClearButtons()
    {
        foreach (var button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
    }

    // 클릭된 타일의 위치를 비동기적으로 반환
    //public IEnumerator WaitForTileClickPiece()
    //{
    //    // 완료 안됐으면 대기
    //    while (!isCompleted)
    //    {
    //        yield return null;
    //    }

    //    yield return isCompleted;
    //}

    // 스크립트 비활성화 시 정리
    private void OnDisable()
    {
        ClearButtons();
    }
}