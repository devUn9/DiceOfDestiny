using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceCustomizeManager : Singletone<DiceCustomizeManager>
{
    [SerializeField] private GameObject cutomizePanel;
    [SerializeField] private GameObject carouselUIPanel;

    [SerializeField] private GameObject pieceCarouselUI;
    [SerializeField] private GameObject pieceNetCarouselUI;

    [SerializeField] private GameObject piecesContent;
    [SerializeField] private GameObject piecePreviewButtonPrefab;

    [SerializeField] private GameObject pieceNetPreviewButtonPrefab;
    [SerializeField] private GameObject pieceNetContent;

    List<PiecePreviewButton> piecePreviewButtonList = new List<PiecePreviewButton>();

    List<PieceNetPreviewButton> pieceNetPreviewButtonList = new List<PieceNetPreviewButton>();

    public GameObject stickerDrawer;

    public GameObject stickerSourcePrefab;

    public CustomizePieceController customizePieceContoller;

    public bool isFolded;

    private void Start()
    {
        InitializePiecesCaruselUI();
        InitializePieceNetCaruselUI();

        InitializeStickerDrawer();
    }

    public void UpdateCaruselUI()
    {
        foreach (var button in piecePreviewButtonList)
        {
            Destroy(button.gameObject);
        }
        piecePreviewButtonList.Clear();
        foreach (var button in pieceNetPreviewButtonList)
        {
            Destroy(button.gameObject);
        }
        pieceNetPreviewButtonList.Clear();
        InitializePiecesCaruselUI();
        InitializePieceNetCaruselUI();
    }

    public void InitializePiecesCaruselUI()
    {
        for (int i = 0; i < InventoryManager.Instance.pieces.Count; i++)
        {
            Piece piece = InventoryManager.Instance.pieces[i];
            if(!piece.isAvailable) continue; // 사용 가능한 조각만 표시
            PiecePreviewButton button = Instantiate(piecePreviewButtonPrefab, piecesContent.transform).GetComponent<PiecePreviewButton>();
            button.InitializePiecePreviewButton(BoardManager.Instance.GetColor(piece.faces[2].color), piece.faces[2].classData.sprite, () => OnClickPiecePreviewButton(piece));
            piecePreviewButtonList.Add(button);
        }
    }
    public void InitializePieceNetCaruselUI()
    {
        for (int i = 0; i < InventoryManager.Instance.pieceNets.Count; i++)
        {
            PieceNet pieceNet = InventoryManager.Instance.pieceNets[i];
            PieceNetPreviewButton button = Instantiate(pieceNetPreviewButtonPrefab, pieceNetContent.transform).GetComponent<PieceNetPreviewButton>();
            button.InitializePieceNetPreviewButton(pieceNet, () => OnClickPieceNetPreviewButton(pieceNet));
            pieceNetPreviewButtonList.Add(button);
        }
    }

    private void InitializeStickerDrawer()
    {
        foreach (var sticker in InventoryManager.Instance.classStickers)
        {
            GameObject stickerSource = Instantiate(stickerSourcePrefab, stickerDrawer.GetComponent<ScrollRect>().content.transform);
            stickerSource.GetComponent<StickerSource>().classSticker = new ClassSticker();
            stickerSource.GetComponent<StickerSource>().classSticker.classData = sticker.Key;
            stickerSource.GetComponent<Image>().sprite = sticker.Key.sprite;
            stickerSource.GetComponent<StickerSource>().stickerCount.text = "x " + sticker.Value.ToString();
        }
    }

    public void UpdateStickerDrawer()
    {
        foreach (Transform child in stickerDrawer.GetComponent<ScrollRect>().content)
        {
            Destroy(child.gameObject);
        }
        InitializeStickerDrawer();
    }
    

    public void OnClickPiecePreviewButton(Piece piece)
    {
        customizePieceContoller.InitializeCustomizePieceMode(piece);
        ChangeToCustomizePanel();
    }

    public void OnClickPieceNetPreviewButton(PieceNet pieceNet)
    {
        customizePieceContoller.InitializeCustomizePieceNetMode(pieceNet);
        ChangeToCustomizePanel();
    }

    void ChangeToCustomizePanel()
    {
        cutomizePanel.SetActive(true);
        carouselUIPanel.SetActive(false);
    }

    public void OnClickPieceCaruselUIButton()
    {
        if (pieceCarouselUI.activeSelf == false)
        {
            pieceNetCarouselUI.SetActive(false);
            pieceCarouselUI.SetActive(true);
        }
    }

    public void OnClickPieceNetCaruselUIButton()
    {
        if (pieceNetCarouselUI.activeSelf == false)
        {
            pieceCarouselUI.SetActive(false);
            pieceNetCarouselUI.SetActive(true);
        }
    }

    public void OnClickBackToSelectPanelButton()
    {
        cutomizePanel.SetActive(false);
        carouselUIPanel.SetActive(true);
    }
}
