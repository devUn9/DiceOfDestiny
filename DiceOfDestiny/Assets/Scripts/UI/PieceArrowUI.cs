using UnityEngine;
using UnityEngine.UI;

public class PieceArrowUI : MonoBehaviour
{
    private const float TARGET_ALPHA = 0.65f;
    [SerializeField] private Image arrowImage;
    [SerializeField] private Animator arrowAnim;

    void Awake()
    {
        EventManager.Instance.AddListener("OnArrowExit", _ => OnArrowExit());

        arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, TARGET_ALPHA);
        arrowAnim.speed = 0f;
    }

    public void OnArrowOver()
    {
        arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, 1f);
        arrowAnim.speed = 1f;
    }

    public void OnArrowExit()
    {
        if (arrowAnim == null)
        {
            Debug.LogWarning("[PieceArrowUI] Arrow Animator is not assigned.");
            return;
        }

        arrowImage.color = new Color(arrowImage.color.r, arrowImage.color.g, arrowImage.color.b, TARGET_ALPHA);
        

        if(arrowAnim != null && arrowAnim.isActiveAndEnabled)
        {
            arrowAnim.speed = 0f;
            arrowAnim.Play("Animation", 0, 0f);
        }
            
    }

    public void OnDisable()
    {
        OnArrowExit();
    }
}
