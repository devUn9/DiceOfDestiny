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
            //EventManager.Instance.TriggerEvent(AllEventNames.PIECE_COUNT_CHANGED);
        }
    }
    private List<PieceState> pieceStates = new();   
    public GameObject[] piecePrefabs;

    [SerializeField] public PieceController currentPiece; // 현재 내가 조종중인 말

    public Piece[] pieceDatas = new Piece[3]; // 이번 게임동안 내가 가져온 말

    protected override void Awake()
    {
        base.Awake();
        UpdatePieceManagerList();

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

    void Start()
    {
        EventManager.Instance.AddListener(AllEventNames.PIECE_COUNT_CHANGED, UpdatePieceManagerList);
    }

    public void DrawAllPieceUIs()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            PieceUIManager.Instance.CreatePieceUI(pieceStates[i].CurrentState, pieces[i].gameObject);
        }
    }

    private void UpdatePieceManagerList(object data = null)
    {
        int count = pieces.Count;
        if (pieceStates.Count < count)
        {
            PieceState newElement = new PieceState();
            newElement.ChangeState(States.Selectable);
            newElement.ChangeSelectable(true);
            pieceStates.Add(newElement);
        }
        if (pieceStates.Count > count)
        {
            pieceStates.RemoveAt(pieceStates.Count - 1);
        }
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