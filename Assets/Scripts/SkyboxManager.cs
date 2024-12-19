using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class SkyboxManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text inputField;
    public Button[] addButtons;
    public Button clearButton;
    public Button copyButton;
    public GameObject downloading;

    [Header("Asset Bundle Info")]
    public string bundleUrlTemplate = "https://drive.google.com/uc?export=download&id={0}";

    [Header("Dropdown")]
    public TMP_Dropdown skyboxDropdown;

    [Header("Skybox content")]
    public Material defaultSkybox;
    public List<Material> skyboxes;

    private Dictionary<string, AssetBundle> assetBundleCache = new Dictionary<string, AssetBundle>();

    private void Start()
    {
        skyboxes.Add(defaultSkybox);
        skyboxDropdown.options.Add(new TMP_Dropdown.OptionData(defaultSkybox.name));
        skyboxDropdown.RefreshShownValue();

        // Add listener to handle dropdown value changes
        skyboxDropdown.onValueChanged.AddListener(OnSkyboxChanged);

        // Add listener to handle adding skyboxes   

        foreach (Button addButton in addButtons)
        {
            addButton.onClick.AddListener(() =>
            {
                string name_id = inputField.text;
                Debug.Log("Link added " + name_id);
                AddMatToList(name_id);
            });
        }

        // Set the initial skybox (optional)
        OnSkyboxChanged(skyboxDropdown.value);
    }

    public void AddMatToList(string bundleId)
    {
        string bundleUrl = string.Format(bundleUrlTemplate, bundleId);
        Debug.Log("Accessed url: " + bundleUrl);
        StartCoroutine(DownloadMaterial(bundleUrl));
    }

    private IEnumerator DownloadMaterial(string bundleUrl)
    {
        downloading.SetActive(true);
        downloading.GetComponent<Animator>().enabled = true;
        foreach (Button addButton in addButtons)
            addButton.gameObject.SetActive(false);
        clearButton.gameObject.SetActive(false);
        copyButton.gameObject.SetActive(false);

        // Check if the bundle is already in cache
        if (assetBundleCache.TryGetValue(bundleUrl, out var cachedBundle))
        {
            Debug.Log("Bundle already present in local cache");
            //AddToListAndDropdown(cachedBundle);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            Debug.Log("Bundle not present in local cache");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error while downloading Asset Bundle: {www.error}");
                downloading.SetActive(false);
                downloading.GetComponent<Animator>().enabled = false;
                foreach (Button addButton in addButtons)
                    addButton.gameObject.SetActive(true);
                clearButton.gameObject.SetActive(true);
                copyButton.gameObject.SetActive(true);
                yield break;
            }

            AssetBundle downloadedBundle = DownloadHandlerAssetBundle.GetContent(www);
            if (downloadedBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                downloading.SetActive(false);
                downloading.GetComponent<Animator>().enabled = false;
                foreach (Button addButton in addButtons)
                    addButton.gameObject.SetActive(true);
                clearButton.gameObject.SetActive(true);
                copyButton.gameObject.SetActive(true);
                yield break;
            }

            assetBundleCache[bundleUrl] = downloadedBundle;

            Debug.Log("Bundle successfully added to local cache");

            AddToListAndDropdown(downloadedBundle);
        }
    }

    private void AddToListAndDropdown(AssetBundle assetBundle)
    {
        // Get all asset names in the bundle
        string[] assetNames = assetBundle.GetAllAssetNames();
        string cleanAssetName = Path.GetFileNameWithoutExtension(assetNames[0]);

        Debug.Log("Bundle name: " + cleanAssetName);

        // Load the GameObject (which has the Skybox component) from the asset bundle
        GameObject skyboxObject = assetBundle.LoadAsset<GameObject>(cleanAssetName);

        if (skyboxObject != null)
        {
            Debug.Log("Loaded GameObject: " + skyboxObject.name);

            // Get the Skybox component attached to the GameObject
            Skybox skyboxComponent = skyboxObject.GetComponent<Skybox>();

            if (skyboxComponent != null)
            {
                // Access the custom skybox material from the Skybox component
                Material customSkybox = skyboxComponent.material;

                if (customSkybox != null)
                {
                    // Reassign the shader to "Skybox/Cubemap" to ensure the material renders correctly
                    customSkybox.shader = Shader.Find("Skybox/Cubemap");

                    inputField.text = "";
                    downloading.SetActive(false);
                    downloading.GetComponent<Animator>().enabled = false;
                    foreach (Button addButton in addButtons)
                        addButton.gameObject.SetActive(true);
                    clearButton.gameObject.SetActive(true);
                    copyButton.gameObject.SetActive(true);

                    Debug.Log("Custom Skybox Material: " + customSkybox.name);

                    // Add the material to the list if it isn't already present
                    if (!skyboxes.Contains(customSkybox))
                    {
                        skyboxes.Add(customSkybox);

                        // Add the material's name to the dropdown options
                        skyboxDropdown.options.Add(new TMP_Dropdown.OptionData(customSkybox.name));
                        skyboxDropdown.RefreshShownValue();

                        // Set the dropdown value to the newly added skybox
                        skyboxDropdown.value = skyboxes.Count - 1;

                        // Change the skybox to the recently added skybox
                        OnSkyboxChanged(skyboxes.Count - 1);
                    }
                }
                else
                {
                    Debug.LogError("Skybox component does not have a valid material assigned.");
                    downloading.SetActive(false);
                    downloading.GetComponent<Animator>().enabled = false;
                    foreach (Button addButton in addButtons)
                        addButton.gameObject.SetActive(true);
                    clearButton.gameObject.SetActive(true);
                    copyButton.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("The loaded GameObject does not have a Skybox component.");
                downloading.SetActive(false);
                downloading.GetComponent<Animator>().enabled = false;
                foreach (Button addButton in addButtons)
                    addButton.gameObject.SetActive(true);
                clearButton.gameObject.SetActive(true);
                copyButton.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Failed to load the GameObject from the AssetBundle.");
            downloading.SetActive(false);
            downloading.GetComponent<Animator>().enabled = false;
            foreach (Button addButton in addButtons)
                addButton.gameObject.SetActive(true);
            clearButton.gameObject.SetActive(true);
            copyButton.gameObject.SetActive(true);
        }
    }

    private void OnSkyboxChanged(int index)
    {
        if (index >= 0 && index < skyboxes.Count)
        {
            // Change the skybox to the selected material
            RenderSettings.skybox = skyboxes[index];
            Debug.Log("Skybox changed to: " + skyboxes[index].name);
        }
    }

    public void ClearAllLetters()
    {
        if (inputField != null)
        {
            if (inputField.text.Length != 0)
            {
                inputField.text = "";
            }
        }
    }

    public void PasteFromClipboard()
    {
        if (inputField != null)
        {
            inputField.text = GUIUtility.systemCopyBuffer;
        }
    }

    // Move to the next skybox in the list (with wrapping)
    public void NextSkybox()
    {
        int currentIndex = skyboxDropdown.value;
        int nextIndex = (currentIndex + 1) % skyboxes.Count;  // Wrap around to the first if at the last
        skyboxDropdown.value = nextIndex;
        OnSkyboxChanged(nextIndex);
    }

    // Move to the previous skybox in the list (with wrapping)
    public void PreviousSkybox()
    {
        int currentIndex = skyboxDropdown.value;
        int prevIndex = (currentIndex - 1 + skyboxes.Count) % skyboxes.Count;  // Wrap around to the last if at the first
        skyboxDropdown.value = prevIndex;
        OnSkyboxChanged(prevIndex);
    }
}