using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[System.Serializable]
public class PieceInventory : MonoBehaviour
{
    public PieceSlot[] pieceSlots = new PieceSlot[3];
    public PieceSlot currentSlot;


   public void InitializeInventory()
    {
        // Initialize the inventory with default pieces
        for (int i = 0; i < pieceSlots.Length; i++)
        {
            Piece piece = PieceManager.Instance.piecePrefabs[i].GetComponent<PieceController>().GetPiece();
            pieceSlots[i] = new PieceSlot { piece = piece };
        }
    }

    public void ResetSlot()
    {
        for (int i = 0; i < pieceSlots.Length; i++)
        {
            Piece piece = PieceManager.Instance.piecePrefabs[i].GetComponent<PieceController>().GetPiece();

            AddSlot(piece);
        }
    }

    public void AddSlot(Piece piece)
    {
        for(int i = 0; i < pieceSlots.Length; i++)
        {
            if (pieceSlots[i] == null || pieceSlots[i].GetPiece() == null)
            {
                pieceSlots[i] = new PieceSlot{ piece = piece }; // Add the piece to the slot
                return;
            }
        }
    }

    public void RemoveSlot(int index)
    {
        pieceSlots[index] = null; // Remove the piece from the slot
    }

    [System.Serializable]
    public class PieceSlot
    {
        [SerializeField] public Piece piece;

        public Piece GetPiece()
        {
            return piece;
        }
    }



}
