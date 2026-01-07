// using UnityEngine;
// using UnityEngine.EventSystems;

// public class TowerPlacementManager : MonoBehaviour
// {
//     [Header("References")]
//     [SerializeField] private Camera mainCamera;
//     [SerializeField] private MoneyManager money;                 
//     [SerializeField] private GameObject rangeIndicatorPrefab;    

//     [Header("Placement Rules (2D)")]
//     [SerializeField] private LayerMask placeableMask;            // Buildable ground
//     [SerializeField] private LayerMask blockedMask;              // Non-placeable (path, water, rocks, etc.)
//     [SerializeField] private LayerMask towerMask;                
//     [SerializeField] private float towerFootprintRadius = 0.45f; // Placeholder tower size

//     [Header("Visuals")]
//     [SerializeField, Range(0.05f, 1f)] private float ghostAlpha = 0.6f;
//     [SerializeField, Range(0.05f, 1f)] private float indicatorAlpha = 0.25f;

//     private GameObject ghostTowerObj;
//     private Tower ghostTower;
//     private MonoBehaviour[] disabledScripts;
//     private Collider2D[] disabledColliders;

//     private GameObject rangeIndicatorObj;
//     private SpriteRenderer rangeIndicatorSR;

//     private int pendingCost;

//     void Awake()
//     {
//         if (mainCamera == null) mainCamera = Camera.main;
//     }

//     void Update()
//     {
//         if (ghostTowerObj == null) return;

//         Vector3 worldPos = GetMouseWorldPosition();
//         ghostTowerObj.transform.position = worldPos;

//         if (rangeIndicatorObj != null)
//             rangeIndicatorObj.transform.position = worldPos;

//         bool valid = IsValidPlacement(worldPos);

//         SetIndicatorColour(valid);

//         // Cancel any time (optional but nice)
//         if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
//         {
//             CancelPlacement();
//             return;
//         }

//         // Place on left click
//         if (Input.GetMouseButtonDown(0))
//         {
//             // Don't place if clicking UI
//             if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//                 return;

//             if (valid)
//                 PlaceTower();
//             else
//                 CancelPlacement(); // invalid click cancels and charges nothing
//         }
//     }

//     public void BeginPlacement(GameObject towerPrefab, int cost)
//     {
//         if (towerPrefab == null) return;

//         // If already placing something, cancel the current ghost
//         if (ghostTowerObj != null)
//             CancelPlacement();

//         // If you have money system, require affordability before even starting placement
//         if (money != null && !money.CanAfford(cost))
//             return;

//         pendingCost = cost;

//         ghostTowerObj = Instantiate(towerPrefab);
//         ghostTower = ghostTowerObj.GetComponent<Tower>();

//         if (ghostTower == null)
//         {
//             Debug.LogError("Tower prefab must have a component that inherits from Tower.");
//             Destroy(ghostTowerObj);
//             ghostTowerObj = null;
//             return;
//         }

//         // Disable colliders so the ghost doesn't block itself / interact
//         disabledColliders = ghostTowerObj.GetComponentsInChildren<Collider2D>(true);
//         foreach (var c in disabledColliders)
//             c.enabled = false;

//         // Disable scripts so it doesn't shoot while placing
//         disabledScripts = ghostTowerObj.GetComponentsInChildren<MonoBehaviour>(true);
//         foreach (var s in disabledScripts)
//             s.enabled = false;

//         // Make tower semi-transparent
//         SetAllSpriteAlpha(ghostTowerObj, ghostAlpha);

//         // Create range indicator
//         if (rangeIndicatorPrefab != null)
//         {
//             rangeIndicatorObj = Instantiate(rangeIndicatorPrefab);
//             rangeIndicatorSR = rangeIndicatorObj.GetComponentInChildren<SpriteRenderer>();

//             float r = Mathf.Max(0.1f, ghostTower.range);
//             rangeIndicatorObj.transform.localScale = new Vector3(r * 2f, r * 2f, 1f);
//         }

//         // Force initial colour
//         SetIndicatorColour(false);
//     }

//     private void PlaceTower()
//     {
//         // Spend money ONLY when placement succeeds
//         if (money != null)
//         {
//             if (!money.TrySpend(pendingCost))
//             {
//                 // If money changed mid-placement cancel
//                 CancelPlacement();
//                 return;
//             }
//         }

//         // Turn ghost into a real tower
//         foreach (var s in disabledScripts)
//             if (s != null) s.enabled = true;

//         foreach (var c in disabledColliders)
//             if (c != null) c.enabled = true;

//         SetAllSpriteAlpha(ghostTowerObj, 1f);

//         CleanupIndicatorOnly();

//         ghostTowerObj = null;
//         ghostTower = null;
//         disabledScripts = null;
//         disabledColliders = null;
//         pendingCost = 0;
//     }

//     private void CancelPlacement()
//     {
//         if (ghostTowerObj != null)
//             Destroy(ghostTowerObj);

//         CleanupIndicatorOnly();

//         ghostTowerObj = null;
//         ghostTower = null;
//         disabledScripts = null;
//         disabledColliders = null;
//         pendingCost = 0;
//     }

//     private void CleanupIndicatorOnly()
//     {
//         if (rangeIndicatorObj != null)
//             Destroy(rangeIndicatorObj);

//         rangeIndicatorObj = null;
//         rangeIndicatorSR = null;
//     }

//     private Vector3 GetMouseWorldPosition()
//     {
//         Vector3 mouse = Input.mousePosition;
//         Vector3 world = mainCamera.ScreenToWorldPoint(mouse);
//         world.z = 0f;
//         return world;
//     }

//     private bool IsValidPlacement(Vector3 worldPos)
//     {
//         Vector2 p = worldPos;

//         // Must be over placeable surface
//         if (Physics2D.OverlapPoint(p, placeableMask) == null)
//             return false;

//         // Must NOT touch blocked zones (paths, water, etc.)
//         if (blockedMask.value != 0 && Physics2D.OverlapCircle(p, towerFootprintRadius, blockedMask) != null)
//             return false;

//         // Must NOT overlap another tower
//         if (towerMask.value != 0 && Physics2D.OverlapCircle(p, towerFootprintRadius, towerMask) != null)
//             return false;

//         return true;
//     }

//     private void SetIndicatorColour(bool isValid)
//     {
//         if (rangeIndicatorSR == null) return;

//         // Grey = valid, Red = invalid
//         Color c = isValid ? new Color(0.6f, 0.6f, 0.6f, indicatorAlpha)
//                           : new Color(1f, 0f, 0f, indicatorAlpha);

//         rangeIndicatorSR.color = c;
//     }

//     private void SetAllSpriteAlpha(GameObject root, float alpha)
//     {
//         var sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
//         foreach (var sr in sprites)
//         {
//             Color c = sr.color;
//             c.a = alpha;
//             sr.color = c;
//         }
//     }
// }
