using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiceCustomizeManager : Singletone<DiceCustomizeManager>
{
    [SerializeField] private GameObject diceCustomizeUIPrefab;
    private DiceCustomizeUI diceCustomizeUI;

    private GameObject carouselUIPanel;

    private GameObject pieceCarouselUI;
    private GameObject pieceNetCarouselUI;
    private GameObject stickerUI;

    private GameObject showPieceButton;
    private GameObject showPieceNestButton;
    private GameObject showStickerButton;

    private GameObject piecesContent;
    private GameObject pieceNetContent;

    private GameObject stickerPanelContent;

    private GameObject stickerDetail;

    [SerializeField] private GameObject piecePreviewButtonPrefab;
    [SerializeField] private GameObject pieceNetPreviewButtonPrefab;
    [SerializeField] private GameObject stickerPreviewButtonUI;

    List<PiecePreviewButton> piecePreviewButtonList;
    List<PieceNetPreviewButton> pieceNetPreviewButtonList;
    List<StickerPreviewButtonUI> stickerPreviewButtonUIList;

    private GameObject customizePanel;
    CustomizePieceController customizePieceContoller;  

    [SerializeField] private GameObject stickerSourcePrefab;
    GameObject stickerDrawer;

    private GameObject backToMainButton;
    private GameObject optionButton;
    [HideInInspector] public bool isFolded;


    public void Initialize()
    {
        GameObject go = Instantiate(diceCustomizeUIPrefab, GameObject.Find("Canvas").transform);
        diceCustomizeUI = go.GetComponent<DiceCustomizeUI>();

        carouselUIPanel = diceCustomizeUI.selectPanel;
        pieceCarouselUI = diceCustomizeUI.pieceScrollView;
        pieceNetCarouselUI = diceCustomizeUI.pieceNetScrollView;

        showPieceButton = diceCustomizeUI.showPieceButton;
        showPieceNestButton = diceCustomizeUI.showPieceNetButton;
        showPieceButton.GetComponent<Button>().onClick.AddListener(OnClickPieceCaruselUIButton);
        showPieceNestButton.GetComponent<Button>().onClick.AddListener(OnClickPieceNetCaruselUIButton);

        stickerUI = diceCustomizeUI.stickerPanel;
        showStickerButton = diceCustomizeUI.showStickerPanelButton;
        showStickerButton.GetComponent<Button>().onClick.AddListener(OnClickStickerUIButton);

        piecesContent = pieceCarouselUI.GetComponent<ScrollRect>().content.gameObject;
        pieceNetContent = pieceNetCarouselUI.GetComponent<ScrollRect>().content.gameObject;

        stickerPanelContent = diceCustomizeUI.stickerPanel.GetComponentInChildren<GridLayoutGroup>().gameObject;

        stickerDetail = stickerUI.GetComponentInChildren<StickerDetailUI>().gameObject;

        customizePanel = diceCustomizeUI.customizePanel;
        customizePieceContoller = diceCustomizeUI.customizePiece.GetComponent<CustomizePieceController>();

        stickerDrawer = customizePanel.GetComponentInChildren<StickerDrawer>().gameObject;

        backToMainButton = diceCustomizeUI.backToMainButton;
        backToMainButton.GetComponent<Button>().onClick.AddListener(OnClickBackToMainButton);

        optionButton = diceCustomizeUI.optionButton;
        optionButton.GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.ToggleSettings(true));

        piecePreviewButtonList = new List<PiecePreviewButton>();
        pieceNetPreviewButtonList = new List<PieceNetPreviewButton>();
        stickerPreviewButtonUIList = new List<StickerPreviewButtonUI>();

        InitializePiecesCaruselUI();
        InitializePieceNetCaruselUI();
        InitializeStickerPanel();
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
        InitializePiecesCaruselUI();
        InitializePieceNetCaruselUI();
        UpdateStickerPanel();
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
    public void InitializeStickerPanel()
    {
        for (int i = 0; i < InventoryManager.Instance.classDataList.Count; i++)
        {
            ClassData classData = InventoryManager.Instance.classDataList[i];
            StickerPreviewButtonUI button = Instantiate(stickerPreviewButtonUI, stickerPanelContent.transform).GetComponent<StickerPreviewButtonUI>();

            bool isUnlock = InventoryManager.Instance.classUnlockStatus.TryGetValue(classData, out bool status) && status;
            ClassSticker classSticker = new ClassSticker();
            classSticker.classData = classData;
            button.Initialize(isUnlock, classSticker, () => ShowStickerDetail(classSticker));
        }
    }

    public void UpdateStickerPanel()
    {
        for(int i = 0; i< stickerPreviewButtonUIList.Count; i++)
        {
            Destroy(stickerPreviewButtonUIList[i].gameObject);
        }
    }



    public void ShowStickerDetail(ClassSticker classSticker)
    {
        bool isUnlock = InventoryManager.Instance.classUnlockStatus.TryGetValue(classSticker.classData, out bool status) && status;
        if (isUnlock)
        {
            stickerDetail.GetComponent<StickerDetailUI>().SetDetail(classSticker);
        }
        else
        {
            stickerDetail.GetComponent<StickerDetailUI>().SetLocked();
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
        customizePanel.SetActive(true);
        carouselUIPanel.SetActive(false);
        backToMainButton.SetActive(false);
    }

    public void OnClickPieceCaruselUIButton()
    {
        if (pieceCarouselUI.activeSelf == false)
        {
            pieceNetCarouselUI.SetActive(false);
            pieceCarouselUI.SetActive(true);
            stickerUI.SetActive(false);
        }
    }

    public void OnClickPieceNetCaruselUIButton()
    {
        if (pieceNetCarouselUI.activeSelf == false)
        {
            pieceCarouselUI.SetActive(false);
            pieceNetCarouselUI.SetActive(true);
            stickerUI.SetActive(false);
        }
    }

    public void OnClickStickerUIButton()
    {
        if (diceCustomizeUI.stickerPanel.activeSelf == false)
        {
            pieceCarouselUI.SetActive(false);
            pieceNetCarouselUI.SetActive(false);
            diceCustomizeUI.stickerPanel.SetActive(true);
        }
    }

    public void OnClickBackToSelectPanelButton()
    {
        customizePanel.SetActive(false);
        carouselUIPanel.SetActive(true);
        backToMainButton.SetActive(true);
    }

    public void OnClickBackToMainButton()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnClickToggleSettingUI()
    {
        UIManager.Instance.ToggleSettings(true);
    }
}
