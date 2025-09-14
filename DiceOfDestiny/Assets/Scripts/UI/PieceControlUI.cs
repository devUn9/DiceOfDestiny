using UnityEngine;
using UnityEngine.UI;

public enum Directions { Up, Down, Left, Right }

public class PieceControlUI : MonoBehaviour
{
    [SerializeField] private Directions dir;
    [SerializeField] private GameObject faceColor;
    [SerializeField] private GameObject faceClass;
    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponentInChildren<Image>();
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0f);

        faceClass.SetActive(false);
        faceColor.SetActive(false);
    }

    // 버튼에 마우스가 올라갔을 때, 기물의 예상되는 이미지 보여주기
    public void OnButtonEnter()
    {
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);

        faceClass.SetActive(true);
        faceColor.SetActive(true);

        Image faceClassImage = faceClass.GetComponent<Image>();
        Image faceColorImage = faceColor.GetComponent<Image>();

        int faceIndex;

        switch (dir)
        {
            case Directions.Up:
                faceIndex = 3;
                break;
            case Directions.Down:
                faceIndex = 1;
                break;
            case Directions.Left:
                faceIndex = 5;
                break;
            case Directions.Right:
                faceIndex = 4;
                break;
            default:
                Debug.Log("<color=#00aeff>이미지 인덱스가 뭔가 오류남</color>");
                faceIndex = 2;
                break;
        }
        var pieceController = GetComponentInParent<PieceController>();
        if (pieceController == null)
        {
            Debug.LogWarning("PieceControlUI: PieceController is null.");
            return;
        }
        Face gettedFace = pieceController.GetFace(faceIndex);
        if (gettedFace.classData == null)
        {
            Debug.LogWarning($"PieceControlUI: gettedFace.classData is null for faceIndex {faceIndex}.");
            return;
        }
        faceClassImage.sprite = gettedFace.classData.sprite;
        Color gettedColor = BoardManager.Instance.tileColors[(int)(gettedFace.color)];
        faceColorImage.color = gettedColor;
    }

    // Move Skill UI용 (도둑, 아기)
    public void OnButtonEnterUI()
    {
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0f);

        faceClass.SetActive(true);
        faceColor.SetActive(true);

        Image faceClassImage = faceClass.GetComponent<Image>();
        Image faceColorImage = faceColor.GetComponent<Image>();

        int faceIndex = 2;

        PieceController currentPiece = PieceManager.Instance.currentPiece;

        Face gettedFace = currentPiece.GetFace(faceIndex);
        faceClassImage.sprite = gettedFace.classData.sprite;
        Color gettedColor = BoardManager.Instance.tileColors[(int)(gettedFace).color];
        faceColorImage.color = gettedColor;

        Debug.Log($"faceColorImage.color: {faceColorImage.color}");
        Debug.Log(gettedFace.classData.sprite.name);
    }

    public void OnButtonExit()
    {
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0f);

        faceClass.SetActive(false);
        faceColor.SetActive(false);
    }

    public void OnClick()
    {
        switch (dir)
        {
            case Directions.Up:
                //Debug.Log("위버튼눌림");
                PieceManager.Instance.currentPiece.MoveUp();
                break;

            case Directions.Down:
                //Debug.Log("밑버튼눌림");
                PieceManager.Instance.currentPiece.MoveDown();
                break;

            case Directions.Left:
                //Debug.Log("왼버튼눌림");
                PieceManager.Instance.currentPiece.MoveLeft();
                break;

            case Directions.Right:
                //Debug.Log("오른버튼눌림");
                PieceManager.Instance.currentPiece.MoveRight();
                break;

            default:
                Debug.Log("<color=#00aeff>UI에 방향 할당되지 않음</color>");
                break;
        }
        ResetUI();
    }

    private void ResetUI()
    {
        buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0f);
        faceClass.SetActive(false);
        faceColor.SetActive(false);
    }
}
