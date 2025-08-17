using UnityEngine;
using UnityEngine.EventSystems;

public class MouseClickSound : MonoBehaviour
{
    [SerializeField] private string soundName = "Click_POP";

    [Header("Click Sound Settings")]
    private float lastClickTime = 0f;
    private float clickCooldown = 0.05f; // 50ms
    public bool IsClickSoundEnabled = true;

    void Update()
    {
        if (!IsClickSoundEnabled) return;

        if (Input.GetMouseButtonDown(0) && Time.unscaledTime - lastClickTime > clickCooldown)
        {
            lastClickTime = Time.unscaledTime;

            AudioManager.Instance.PlaySFX("Click_POP");
        }
    }
}
