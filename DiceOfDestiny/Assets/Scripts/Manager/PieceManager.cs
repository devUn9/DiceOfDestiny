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
}