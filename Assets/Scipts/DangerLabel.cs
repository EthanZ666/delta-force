using TMPro;
using UnityEngine;

public class DangerLabel2D : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);

    private RectTransform rect;
    private Transform target;
    private Camera cam;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (tmp == null) tmp = GetComponentInChildren<TMP_Text>(true);
        cam = Camera.main;
    }

    public void AttachTo(Transform newTarget, Vector3 offset)
    {
        target = newTarget;
        worldOffset = offset;
        gameObject.SetActive(true);
        LateUpdate();
    }

    public void SetValue(int dangerLevel)
    {
        if (tmp != null) tmp.text = $"Danger lvl: {dangerLevel.ToString()}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        target = null;
    }

    private void LateUpdate()
    {
        if (!target) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        rect.position = screenPos;
    }
}
