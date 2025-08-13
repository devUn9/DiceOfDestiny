using System;
using UnityEngine;
using UnityEngine.UI;

public class PieceSelectButton : MonoBehaviour
{

    public Image buttonImage;
    public Image buttonSprite;
    Button button;

    Piece piece;
    PieceSelectPanelUI pieceSelectPanel;

    bool isSelected = false;
    int selectedIndex = -1;

    public void Initialize(Piece piece, PieceSelectPanelUI pieceSelectPanel)
    {
        this.piece = piece;
        this.pieceSelectPanel = pieceSelectPanel;
        buttonSprite.sprite = piece.faces[2].classData.sprite;
        buttonImage.color = BoardManager.Instance.GetColor(piece.faces[2].color);
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.5f);
        buttonSprite.color = new Color(buttonSprite.color.r, buttonSprite.color.g, buttonSprite.color.b, 0.5f);

        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickPieceSelectButton);
    }

    public void OnClickPieceSelectButton()
    {
        Debug.Log($"[PieceSelectButton] OnClickPieceSelectButton() - {piece.name} isSelected: {isSelected}");

        if (isSelected)
        {
            DeselectPiece();
        }
        else
        {
            SelectPiece();
        }
    }

    public void SelectPiece()
    {
        for (int i = 0; i < pieceSelectPanel.tempSelectedPieces.Length; i++)
        {
            if (pieceSelectPanel.tempSelectedPieces[i] == null)
            {
                pieceSelectPanel.tempSelectedPieces[i] = piece;
                pieceSelectPanel.tempSelectedPieceCount++;
                pieceSelectPanel.UpdateSelectText();
                selectedIndex = i;
                isSelected = true;
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
                buttonSprite.color = new Color(buttonSprite.color.r, buttonSprite.color.g, buttonSprite.color.b, 1f);
                return;
            }
        }
    }

    public void DeselectPiece()
    {
        isSelected = false;
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.5f);
        buttonSprite.color = new Color(buttonSprite.color.r, buttonSprite.color.g, buttonSprite.color.b, 0.5f);
        pieceSelectPanel.tempSelectedPieces[selectedIndex] = null;
        pieceSelectPanel.tempSelectedPieceCount--;
        pieceSelectPanel.UpdateSelectText();
        selectedIndex = -1;
    }
}
