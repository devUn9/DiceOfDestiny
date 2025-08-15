using NUnit.Framework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PieceSelectPanelUI : MonoBehaviour
{
    public GameObject pieceSelectButtonContent;
    public GameObject pieceSelectButtonPrefab;
    private List<GameObject> pieceSelectButtons = new List<GameObject>();

    public TextMeshProUGUI pieceSelectText;
    public Button startButton;
    public Button backButton;

    public Piece[] tempSelectedPieces = new Piece[4];
    public int tempSelectedPieceCount = 0;

    Action StartGame;

    public void Intitialize(System.Action StartGame)
    {
        this.StartGame = StartGame;

        for(int i = 0; i < pieceSelectButtons.Count; i++)
        {
            Destroy(pieceSelectButtons[i]);
        }
        pieceSelectButtons = new List<GameObject>();
        tempSelectedPieceCount = 0;
        tempSelectedPieces = new Piece[4];
        UpdateSelectText();
        startButton.interactable = false;

        for (int i = 0; i < InventoryManager.Instance.pieces.Count; i++)
        {
            if (InventoryManager.Instance.pieces[i].isAvailable)
            {
                GameObject pieceSelectButton = Instantiate(pieceSelectButtonPrefab, pieceSelectButtonContent.transform);
                pieceSelectButtons.Add(pieceSelectButton);
                PieceSelectButton pieceSelectButtonUI = pieceSelectButton.GetComponent<PieceSelectButton>();
                pieceSelectButtonUI.Initialize(InventoryManager.Instance.pieces[i], this);
            }
        }
    }
    public void OnClickStartButton()
    {
        for(int i = 0; i < tempSelectedPieces.Length; i++)
        {
            if (tempSelectedPieces[i] == null)
            {
                Debug.LogError($"[PieceSelectPanelUI] tempSelectedPieces[{i}] is null. Please select a piece.");
                return;
            }
        }
        GameManager.Instance.SetPieces(tempSelectedPieces);
        StartGame.Invoke();
    }

    public void OnClickBackButton()
    {
        this.gameObject.SetActive(false);
    }

    public void UpdateSelectText()
    {
        pieceSelectText.text = $"모험에 들고 갈 기물을 선택해 주세요!({tempSelectedPieceCount} / 4)";

        if (tempSelectedPieceCount >= 4)
        {
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
    }
}
