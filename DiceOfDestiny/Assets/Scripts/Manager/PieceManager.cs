using System;
using System.Collections.Generic;
using UnityEngine;

// 기물 관리하는 매니저
public class PieceManager : Singletone<PieceManager>
{
    List<PieceController> pieces = new List<PieceController>();
    public List<PieceController> Pieces
    {
        get => pieces;
        set
        {
            pieces = value;
        }
    }
    public GameObject[] piecePrefabs;

    [SerializeField] public PieceController currentPiece; // 현재 내가 조종중인 말

    public Piece[] pieceDatas = new Piece[4]; // 이번 게임동안 내가 가져온 말

    protected override void Awake()
    {
        base.Awake();
        InitializePieceDatas();
    }

    private void InitializePieceDatas()
    {
        pieceDatas = GameManager.Instance.selectedPieces;
        for (int i = 0; i < pieceDatas.Length; i++)
        {
            piecePrefabs[i].GetComponent<PieceController>().SetPiece(pieceDatas[i]);
        }
    }

    public void SetCurrentPieceControl(bool canControl)
    {
        currentPiece.canControl = canControl;
    }

    public void DecreaseDebuffAllPieces()
    {
        foreach (var piece in pieces)
        {
            piece.statusEffectController.EndTurn();
        }
    }

    public PieceController GetCurrentPiece()
    {
        return currentPiece;
    }

    public void SetCurrentPiece(PieceController pieceController)
    {
        currentPiece = pieceController;
    }

    public void GeneratePiece(int currentIndex, Vector2Int gridPos)
    {
        // 피스 생성
        GameObject piece = Instantiate(PieceManager.Instance.piecePrefabs[currentIndex],
        new Vector2(BoardManager.Instance.boardTransform.position.x + gridPos.x,
                    BoardManager.Instance.boardTransform.position.y + gridPos.y),
        Quaternion.identity, this.transform);

        // 현재 조작중인 기물로 초기화
        PieceController currentPieceController = piece.GetComponent<PieceController>();

        // 보드 판 내부 좌표 초기화
        currentPieceController.gridPosition = gridPos;

        // 생성된 기물 윗면 초기화
        currentPieceController.SetTopFace();

        // 생성된 위치의 타일의 피스정보를 저장
        BoardManager.Instance.Board[gridPos.x, gridPos.y].SetPiece(currentPieceController);

        // 피스 리스트에 추가
        Pieces.Add(currentPieceController);

        // 현재 선택 피스
        SetCurrentPiece(currentPieceController);

        // 피스 선택 테두리 생성
        BoardSelectManager.Instance.PieceHighlightTiles(currentPieceController.gridPosition);
    }

    public void ResetPieces(PieceController clearPiece = null)
    {
        var toRemove = new List<PieceController>();

        // 인게임 보드판에 있는 피스들 인벤토리로 돌아가게 하기
        foreach (var piece in Pieces)
        {
            if (piece != clearPiece)
            {
                toRemove.Add(piece);
            }
        }

        foreach (var piece in toRemove)
        {
            for (int i = 0; i < 4; i++)
            {
                if (pieceDatas[i] == null)
                {
                    pieceDatas[i] = piece.GetPiece();
                }
            }
            Destroy(piece.gameObject);
            Pieces.Remove(piece);
        }

        // 현재 선택 피스 null
        SetCurrentPiece(null);
    }

    public void ClearPieces()
    {
        // 모든 기물 오브젝트 제거 및 리스트 초기화
        foreach (var piece in new List<PieceController>(Pieces))
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }
        Pieces.Clear();
        SetCurrentPiece(null);
    }
}