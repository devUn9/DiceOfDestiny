using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PieceNetPreviewButton : MonoBehaviour
{
    public Image frontFace;
    public Image backFace;
    public Image leftFace;
    public Image rightFace;
    public Image topFace;
    public Image bottomFace;

    public Button button;

    public void InitializePieceNetPreviewButton(PieceNet pieceNet, UnityAction onClick)
    {
        frontFace.color = BoardManager.Instance.GetColor(pieceNet.faces[2].color);
        backFace.color = BoardManager.Instance.GetColor(pieceNet.faces[0].color);
        leftFace.color = BoardManager.Instance.GetColor(pieceNet.faces[1].color);
        rightFace.color = BoardManager.Instance.GetColor(pieceNet.faces[3].color);
        topFace.color = BoardManager.Instance.GetColor(pieceNet.faces[5].color);
        bottomFace.color = BoardManager.Instance.GetColor(pieceNet.faces[4].color);

        button.onClick.AddListener(onClick);
    }
}
