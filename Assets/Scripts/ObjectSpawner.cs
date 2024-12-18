using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.UI;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ARRaycastManager))]
public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs and UI")]
    [SerializeField] private List<GameObject> objectPrefabs; // List of spawnable prefabs
    [SerializeField] private TMP_Dropdown dropdown; // Dropdown UI for model selection
    [SerializeField] private Button deleteAllButton;
    [SerializeField] private Button deleteCurrentButton;
    [SerializeField] private Button deselectAllButton;

    [Header("AR and Interaction")]
    [SerializeField] private Material outlineMaterial; // Outline material for selection
    [SerializeField] private LayerMask interactableLayer; // Layer for interactable objects
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private GameObject selectedPrefab;

    [Header("Instantiated Objects")]
    public static List<GameObject> instantiatedObjects = new List<GameObject>();
    private GameObject selectedObject;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (objectPrefabs.Count > 0)
            selectedPrefab = objectPrefabs[0];
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        EnhancedTouch.Touch.onFingerDown += OnTouchBegin;

        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        if (deleteAllButton != null)
        {
            deleteAllButton.onClick.AddListener(DeleteAllObjects);
        }

        if (deleteCurrentButton != null)
        {
            deleteCurrentButton.onClick.AddListener(DeleteCurrentObject);
        }

        if (deselectAllButton != null)
        {
            deselectAllButton.onClick.AddListener(DeselectAllObjects);
        }

        UpdateDropdown();
        UpdateDeleteCurrentButtonVisibility(); // Ensure correct initial state
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
        EnhancedTouch.Touch.onFingerDown -= OnTouchBegin;

        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }

        if (deleteAllButton != null)
        {
            deleteAllButton.onClick.RemoveListener(DeleteAllObjects);
        }

        if (deleteCurrentButton != null)
        {
            deleteCurrentButton.onClick.RemoveListener(DeleteCurrentObject);
        }

        if (deselectAllButton != null)
        {
            deselectAllButton.onClick.RemoveListener(DeselectAllObjects);
        }
    }

    // Add this method to dynamically add models
    public void AddToSpawnerList(GameObject newPrefab)
    {
        if (!objectPrefabs.Contains(newPrefab))
        {
            objectPrefabs.Add(newPrefab);

            // Update the dropdown with the new model
            selectedPrefab = objectPrefabs[0];
            dropdown.options.Add(new TMP_Dropdown.OptionData(newPrefab.name));
            dropdown.RefreshShownValue();

            Debug.Log($"Model '{newPrefab.name}' added to spawner list and dropdown.");
        }
        else
        {
            Debug.Log($"Model '{newPrefab.name}' already exists in the spawner list.");
        }
    }

    private void OnTouchBegin(Finger finger)
    {
        // Only process the primary touch
        if (finger.index != 0) return;

        // Check if the touch is over UI
        if (IsTouchOverUI(finger.currentTouch.screenPosition)) return;

        // Check if touch is over am interactable object
        if (IsTouchOverInteractable(finger.currentTouch.screenPosition))
        {
            Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
            {
                // Get the root parent of the hit object
                GameObject rootObject = hit.collider.gameObject.transform.root.gameObject;

                // Trigger the selection logic
                OnObjectSelected(rootObject);
            }
            // Prevent placing a new object
            return;
        }

        // Check if the touch is on an AR plane
        if (raycastManager.Raycast(finger.currentTouch.screenPosition, raycastHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;

            if (selectedPrefab != null)
            {
                GameObject placedObject = Instantiate(selectedPrefab, hitPose.position, hitPose.rotation);
                placedObject.layer = interactableLayer;

                instantiatedObjects.Add(placedObject);

                var grabInteractable = placedObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.selectEntered.AddListener(args => OnObjectSelected(placedObject));
                }
            }
        }

        // Otherwise deselect any selected object
        else
        {
            DeselectAllObjects();
        }
    }

    private void OnObjectSelected(GameObject obj)
    {
        if (selectedObject == obj)
            return;

        if (selectedObject != null)
        {
            RemoveOutlineFromObject(selectedObject);
        }

        selectedObject = obj;

        if (selectedObject != null)
        {
            ApplyOutlineToObject(selectedObject);
        }

        UpdateDeleteCurrentButtonVisibility(); // Update button visibility
    }

    private bool IsTouchOverUI(Vector2 screenPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPosition };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        return raycastResults.Count > 0;
    }

    private bool IsTouchOverInteractable(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer);
    }

    private void ApplyOutlineToObject(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var materials = new List<Material>(renderer.materials);
            if (!materials.Contains(outlineMaterial))
            {
                materials.Add(outlineMaterial);
                renderer.materials = materials.ToArray();
            }
        }
    }

    private void RemoveOutlineFromObject(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var materials = new List<Material>(renderer.materials);
            materials.RemoveAll(mat => mat != null && mat.shader == outlineMaterial.shader);
            renderer.materials = materials.ToArray();
        }
    }

    private void DeleteAllObjects()
    {
        foreach (var obj in instantiatedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        instantiatedObjects.Clear();
        selectedObject = null;
        UpdateDeleteCurrentButtonVisibility(); // Update button visibility
    }

    private void DeleteCurrentObject()
    {
        if (selectedObject != null)
        {
            instantiatedObjects.Remove(selectedObject);
            Destroy(selectedObject);
            selectedObject = null;
            UpdateDeleteCurrentButtonVisibility(); // Update button visibility
        }
    }

    private void DeselectAllObjects()
    {
        foreach (var obj in instantiatedObjects)
        {
            if (obj != null)
            {
                RemoveOutlineFromObject(obj);
            }
        }
        selectedObject = null;
        UpdateDeleteCurrentButtonVisibility(); // Update button visibility
    }

    // Update dropdown options
    private void UpdateDropdown()
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();
        List<string> prefabNames = new List<string>();

        foreach (var prefab in objectPrefabs)
        {
            prefabNames.Add(prefab.name);
        }

        dropdown.AddOptions(prefabNames);

        if (objectPrefabs.Count > 0)
            selectedPrefab = objectPrefabs[0];
    }

    private void OnDropdownValueChanged(int index)
    {
        SetSelectedPrefab(index);
    }

    public void SetSelectedPrefab(int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < objectPrefabs.Count)
        {
            selectedPrefab = objectPrefabs[prefabIndex];
        }
    }

    private void UpdateDeleteCurrentButtonVisibility()
    {
        if (deleteCurrentButton != null)
        {
            deleteCurrentButton.gameObject.SetActive(selectedObject != null);
        }
    }
}