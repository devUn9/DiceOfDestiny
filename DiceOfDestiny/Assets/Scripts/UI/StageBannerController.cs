using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public sealed class StageBannerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform bannerRect = null!;
    [SerializeField] private TextMeshProUGUI stageText = null!;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = .3f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private Vector2 slideOffset = new(0f, 200f);

    private CanvasGroup cg = null!;
    private Vector2 _initPos;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        _initPos = bannerRect.anchoredPosition;
    }

    public void Show(int stageNumber, string stageTitle)
    {
        stageText.text = $"STAGE {stageNumber} – {stageTitle}";

        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        cg.alpha = 0f;

        // Fade-in
        yield return Fade(0f, 1f, fadeDuration);

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Slide + Fade-out
        var targetPos = _initPos + slideOffset;
        yield return SlideAndFade(targetPos, 0f, fadeDuration);

        // Reset
        bannerRect.anchoredPosition = _initPos;

        // Deactivate the banner
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator SlideAndFade(Vector2 endPos, float endAlpha, float duration)
    {
        var startPos = bannerRect.anchoredPosition;
        var startAlpha = cg.alpha;

        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            var lerp = t / duration;
            bannerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, lerp);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, lerp);
            yield return null;
        }
        bannerRect.anchoredPosition = endPos;
        cg.alpha = endAlpha;
    }
}
