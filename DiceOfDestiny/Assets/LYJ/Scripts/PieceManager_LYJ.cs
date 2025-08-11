using System.Collections.Generic;
using UnityEngine;

public class PieceManager_LYJ : Singletone<PieceManager_LYJ>
{
    private const string PIECE_DATA_PATH = "PieceData";

    private List<Piece> allPieceDatas = new List<Piece>();
    List<PieceController> pieces = new List<PieceController>();
    public List<PieceController> Pieces
    {
        get => pieces;
        private set
        {
            pieces = value;
            EventManager.Instance.TriggerEvent(AllEventNames.PIECE_COUNT_CHANGED);
        }
    }
    private List<PieceState> pieceStates = new();
    public GameObject piecePrefab;
    private PieceController currentPiece; // 현재 내가 조종중인 말

    void Awake()
    {
        LoadAllPieceDatas();
        UpdatePieceManagerLists();
    }

    void Start()
    {
        EventManager.Instance.AddListener(AllEventNames.PIECE_COUNT_CHANGED, UpdatePieceManagerLists);
    }

    public void DrawAllPieceUIs()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            PieceUIManager.Instance.CreatePieceUI(pieceStates[i].CurrentState, pieces[i].gameObject);
        }
    }

    private void UpdatePieceManagerLists(object data = null)
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

    //

    public void AddPiece(PieceController newPiece, int pieceNumber)
    {
        Piece targetData = null;
        if (pieceNumber < allPieceDatas.Count)
        {
            // targetData = allPieceDatas.Find(data => data.PieceNumber == pieceNumber);
        }
        if (targetData == null)
        {
            Debug.Log($"<color=#30ffae>Can not found Piece with Number {pieceNumber}</color>");
            Debug.Log($"<color=#30ffae>Can not add Piece</color>");
            return;
        }

        pieces.Add(newPiece);
        newPiece.Init(targetData);
        Pieces = pieces;
        newPiece.SetInGame(true);
    }

    public void RemovePiece(PieceController targetPiece)
    {
        pieces.Remove(targetPiece);
        Pieces = pieces;
        targetPiece.SetInGame(false);
    }

    private void LoadAllPieceDatas()
    {
        Piece[] loadedDatas = Resources.LoadAll<Piece>(PIECE_DATA_PATH);

        if (loadedDatas.Length == 0)
        {
            Debug.Log($"<color=#30ffae>Can not found Piece data in \'Resources/{PIECE_DATA_PATH}\'.</color>");
        }

        allPieceDatas = new List<Piece>(loadedDatas);

        Debug.Log($"<color=#30ffae>Loaded Pieces : {allPieceDatas.Count}");
    }


}