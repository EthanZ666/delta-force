using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MoneyManager money;
    [SerializeField] private GameObject rangeIndicatorPrefab;

    [Header("Placement Rules (2D)")]
    [SerializeField] private LayerMask placeableMask;
    [SerializeField] private LayerMask blockedMask;
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private float towerFootprintRadius = 0.45f;

    [Header("Visuals")]
    [SerializeField, Range(0.05f, 1f)] private float ghostAlpha = 0.6f;
    [SerializeField, Range(0.05f, 1f)] private float indicatorAlpha = 0.25f;

    [Header("Auto Scale (Normalize Prefab Size)")]
    [SerializeField] private bool autoNormalizeScale = true;

    [SerializeField] private float targetWorldHeight = 1.0f;

    [Header("Range Indicator Scaling")]
    [SerializeField, Range(0.1f, 2f)] private float rangeIndicatorRadiusMultiplier = 1.0f;

    private GameObject ghostTowerObj;
    private Tower ghostTower;
    private MonoBehaviour[] disabledScripts;
    private Collider2D[] disabledColliders;

    private GameObject rangeIndicatorObj;
    private SpriteRenderer rangeIndicatorSR;

    private int pendingCost;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (ghostTowerObj == null) return;

        Vector3 worldPos = GetMouseWorldPosition();
        ghostTowerObj.transform.position = worldPos;

        if (rangeIndicatorObj != null)
            rangeIndicatorObj.transform.position = worldPos;

        bool valid = IsValidPlacement(worldPos);
        SetIndicatorColour(valid);

        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool rightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        if (escapePressed || rightClick)
        {
            CancelPlacement();
            return;
        }

        bool leftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (leftClick)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (valid)
                PlaceTower();
            else
                CancelPlacement();
        }
    }

    public void BeginPlacement(TowerData towerdata)
    {
        if (towerdata.towerPrefab == null) return;

        if (ghostTowerObj != null)
            CancelPlacement();

        int cost = towerdata.price;

        if (money != null && !money.CanAfford(cost))
            return;

        pendingCost = cost;

        ghostTowerObj = Instantiate(towerdata.towerPrefab);

        if (autoNormalizeScale)
            NormalizeToTargetHeight(ghostTowerObj, targetWorldHeight);

        ghostTower = ghostTowerObj.GetComponent<Tower>();

        if (ghostTower == null)
        {
            Debug.LogError("Tower prefab must have a component that inherits from Tower.");
            Destroy(ghostTowerObj);
            ghostTowerObj = null;
            pendingCost = 0;
            return;
        }

        disabledColliders = ghostTowerObj.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in disabledColliders)
            if (c != null) c.enabled = false;

        disabledScripts = ghostTowerObj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var s in disabledScripts)
            if (s != null) s.enabled = false;

        SetAllSpriteAlpha(ghostTowerObj, ghostAlpha);

        if (rangeIndicatorPrefab != null)
        {
            rangeIndicatorObj = Instantiate(rangeIndicatorPrefab);
            rangeIndicatorSR = rangeIndicatorObj.GetComponentInChildren<SpriteRenderer>();

            float r = Mathf.Max(0.1f, ghostTower.range) * rangeIndicatorRadiusMultiplier;
            SetRangeIndicatorRadius(rangeIndicatorObj, rangeIndicatorSR, r);
        }

        SetIndicatorColour(false);
    }

    private void PlaceTower()
    {
        if (money != null)
        {
            if (!money.TrySpend(pendingCost))
            {
                CancelPlacement();
                return;
            }
        }

        if (disabledScripts != null)
            foreach (var s in disabledScripts)
                if (s != null) s.enabled = true;

        if (disabledColliders != null)
            foreach (var c in disabledColliders)
                if (c != null) c.enabled = true;

        SetAllSpriteAlpha(ghostTowerObj, 1f);

        CleanupIndicatorOnly();

        ghostTowerObj = null;
        ghostTower = null;
        disabledScripts = null;
        disabledColliders = null;
        pendingCost = 0;
    }

    private void CancelPlacement()
    {
        if (ghostTowerObj != null)
            Destroy(ghostTowerObj);

        CleanupIndicatorOnly();

        ghostTowerObj = null;
        ghostTower = null;
        disabledScripts = null;
        disabledColliders = null;
        pendingCost = 0;
    }

    private void CleanupIndicatorOnly()
    {
        if (rangeIndicatorObj != null)
            Destroy(rangeIndicatorObj);

        rangeIndicatorObj = null;
        rangeIndicatorSR = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (Mouse.current == null)
            return Vector3.zero;

        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector3 screen = new Vector3(mouse.x, mouse.y, -mainCamera.transform.position.z);
        Vector3 world = mainCamera.ScreenToWorldPoint(screen);
        world.z = 0f;
        return world;
    }

    private bool IsValidPlacement(Vector3 worldPos)
    {
        Vector2 p = worldPos;

        if (Physics2D.OverlapPoint(p, placeableMask) == null)
            return false;

        if (blockedMask.value != 0 && Physics2D.OverlapCircle(p, towerFootprintRadius, blockedMask) != null)
            return false;

        if (towerMask.value != 0 && Physics2D.OverlapCircle(p, towerFootprintRadius, towerMask) != null)
            return false;

        return true;
    }

    private void SetIndicatorColour(bool isValid)
    {
        if (rangeIndicatorSR == null) return;

        Color c = isValid
            ? new Color(0.6f, 0.6f, 0.6f, indicatorAlpha)
            : new Color(1f, 0f, 0f, indicatorAlpha);

        rangeIndicatorSR.color = c;
    }

    private void SetAllSpriteAlpha(GameObject root, float alpha)
    {
        if (root == null) return;

        var sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in sprites)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void NormalizeToTargetHeight(GameObject root, float desiredHeight)
    {
        if (root == null) return;

        var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        Vector3 originalScale = root.transform.localScale;
        root.transform.localScale = Vector3.one;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        float currentHeight = b.size.y;
        if (currentHeight <= 0.0001f)
        {
            root.transform.localScale = originalScale;
            return;
        }

        float factor = desiredHeight / currentHeight;
        root.transform.localScale = originalScale * factor;
    }


    private void SetRangeIndicatorRadius(GameObject indicatorObj, SpriteRenderer sr, float radius)
    {
        if (indicatorObj == null || sr == null) return;

        float currentDiameter = Mathf.Max(sr.bounds.size.x, sr.bounds.size.y);
        if (currentDiameter <= 0.0001f) return;

        float desiredDiameter = radius * 2f;
        float factor = desiredDiameter / currentDiameter;

        indicatorObj.transform.localScale *= factor;
    }
}
