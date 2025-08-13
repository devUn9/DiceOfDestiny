using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public sealed class StageBannerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform bannerRect;
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdDuration = 1.25f;
    [Header("Slide Offset")]
    [SerializeField] private Vector2 initPos;
    [SerializeField] private Vector2 slideOffset = new(0f, 100f);

    private CanvasGroup cg;
    private LayoutGroup layoutGroup;

    private void Awake()
    {
        Canvas.ForceUpdateCanvases();
        cg = GetComponent<CanvasGroup>();
        layoutGroup = GetComponentInParent<LayoutGroup>();
        cg.alpha = 0f;
        initPos = bannerRect.anchoredPosition;
    }

    public void Show(int stageNumber, string stageTitle)
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        stageText.text =
            $"STAGE {stageNumber}\n" +
            $"{stageTitle}";

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (layoutGroup != null) layoutGroup.enabled = false;

        Vector2 inStart = initPos + slideOffset;
        Vector2 inEnd = initPos;

        Vector2 outStart = initPos;
        Vector2 outEnd = initPos + slideOffset;

        gameObject.SetActive(true);
        bannerRect.anchoredPosition = inStart;
        cg.alpha = 0f;

        yield return null;
        // Fade-in
        yield return SlideAndFade(inStart, 0f, inEnd, 1f, fadeDuration);
        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Slide + Fade-out
        yield return SlideAndFade(outStart, 1f, outEnd, 0f, 0.5f);

        // Reset
        bannerRect.anchoredPosition = initPos;
        cg.alpha = 0f;

        // Deactivate the banner
        gameObject.SetActive(false);
        if (layoutGroup != null) layoutGroup.enabled = true;
    }

    private IEnumerator SlideAndFade(Vector2 startPos, float startAlpha, Vector2 endPos, float endAlpha, float duration)
    {
        bannerRect.anchoredPosition = startPos;
        cg.alpha = startAlpha;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float lerp = t / duration;
            bannerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, lerp);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, lerp);
            yield return null;
        }

        bannerRect.anchoredPosition = endPos;
        cg.alpha = endAlpha;
    }
}
