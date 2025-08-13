using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("Stage Debug Settings")]
    [SerializeField] private Button reColorBoardButton;
    [SerializeField] private Button regenerateButton;


    private void Start()
    {
        reColorBoardButton.onClick.AddListener(onClickReColorBoardButton);
    }

    public void onClickReColorBoardButton()
    {
        
    }

    private void Update()
    {

    }
}
