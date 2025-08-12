using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiceCustomizeManager : Singletone<DiceCustomizeManager>
{
<<<<<<< Updated upstream
    [Header("UI Panels")]
    [SerializeField] private GameObject cutomizePanel;
    [SerializeField] private GameObject carouselUIPanel;
    [SerializeField] private GameObject pieceCarouselUI;
    [SerializeField] private GameObject pieceNetCarouselUI;
=======
    [SerializeField] private GameObject diceCustomizeUIPrefab;
    private DiceCustomizeUI diceCustomizeUI;

    private GameObject carouselUIPanel;

    private GameObject pieceCarouselUI;
    private GameObject pieceNetCarouselUI;

    private GameObject showPieceButton;
    private GameObject showPieceNestButton;

    private GameObject piecesContent;
    private GameObject pieceNetContent;
>>>>>>> Stashed changes

    [SerializeField] private GameObject piecePreviewButtonPrefab;
    [SerializeField] private GameObject pieceNetPreviewButtonPrefab;

<<<<<<< Updated upstream
    List<PiecePreviewButton> piecePreviewButtonList = new List<PiecePreviewButton>();
    List<PieceNetPreviewButton> pieceNetPreviewButtonList = new List<PieceNetPreviewButton>();

    [Header("Sticker Drawer")]
    public GameObject stickerDrawer;
    public GameObject stickerSourcePrefab;

    [Header("Customize Controller")]
    public CustomizePieceController customizePieceContoller;  
    public GameObject backToMainButton;
=======
    private GameObject customizePanel;

    List<PiecePreviewButton> piecePreviewButtonList;
    List<PieceNetPreviewButton> pieceNetPreviewButtonList;
>>>>>>> Stashed changes

    [HideInInspector] public bool isFolded;

<<<<<<< Updated upstream
=======
    [SerializeField] private GameObject stickerSourcePrefab;

    CustomizePieceController customizePieceContoller;
    GameObject stickerDrawer;

    private GameObject backToMainButton;

    [HideInInspector] public bool isFolded;

>>>>>>> Stashed changes
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

        piecesContent = pieceCarouselUI.GetComponent<ScrollRect>().content.gameObject;
        pieceNetContent = pieceNetCarouselUI.GetComponent<ScrollRect>().content.gameObject;

        customizePanel = diceCustomizeUI.customizePanel;
        customizePieceContoller = diceCustomizeUI.customizePiece.GetComponent<CustomizePieceController>();

        stickerDrawer = customizePanel.GetComponentInChildren<StickerDrawer>().gameObject;

        backToMainButton = diceCustomizeUI.backToMainButton;
        backToMainButton.GetComponent<Button>().onClick.AddListener(OnClickBackToMainButton);

        piecePreviewButtonList = new List<PiecePreviewButton>();
        pieceNetPreviewButtonList = new List<PieceNetPreviewButton>();

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
        Debug.Log(InventoryManager.Instance);
        Debug.Log(InventoryManager.Instance.pieces);
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
        customizePanel.SetActive(true);
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
        customizePanel.SetActive(false);
        carouselUIPanel.SetActive(true);
    }

    public void OnClickBackToMainButton()
    {
        SceneManager.LoadScene("Main");
    }
}
