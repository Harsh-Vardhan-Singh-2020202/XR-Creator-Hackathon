using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class SkyboxManager : MonoBehaviour
{
    [Header("Source Toggle")]
    [Tooltip("If true, downloads skybox materials from a remote Asset Bundle (Google Drive). If false, uses locally assigned materials only.")]
    public bool useRemoteBundle = false;

    [Header("UI")]
    public TMP_Text inputField;
    public Button[] addButtons;
    public Button clearButton;
    public Button copyButton;
    public GameObject downloading;

    [Header("Asset Bundle Info (used when useRemoteBundle = true)")]
    public string bundleUrlTemplate = "https://drive.google.com/uc?export=download&id={0}";

    [Header("Dropdown")]
    public TMP_Dropdown skyboxDropdown;

    [Header("Skybox content")]
    public Material defaultSkybox;
    public List<Material> skyboxes; // Local skyboxes: assign extra materials here directly in the Inspector

    private Dictionary<string, AssetBundle> assetBundleCache = new Dictionary<string, AssetBundle>();

    private void Start()
    {
        skyboxes.Add(defaultSkybox);
        skyboxDropdown.options.Add(new TMP_Dropdown.OptionData(defaultSkybox.name));

        // If not using remote bundles, populate dropdown with all locally-assigned skyboxes upfront
        if (!useRemoteBundle)
        {
            for (int i = 0; i < skyboxes.Count; i++)
            {
                if (skyboxes[i] == defaultSkybox) continue; // already added above
                skyboxDropdown.options.Add(new TMP_Dropdown.OptionData(skyboxes[i].name));
            }

            // No need for the download UI in local mode
            if (downloading != null) downloading.SetActive(false);
            foreach (Button addButton in addButtons)
                if (addButton != null) addButton.gameObject.SetActive(false);
        }

        skyboxDropdown.RefreshShownValue();

        // Add listener to handle dropdown value changes
        skyboxDropdown.onValueChanged.AddListener(OnSkyboxChanged);

        // Add listener to handle adding skyboxes via remote bundle (only relevant if useRemoteBundle = true)
        if (useRemoteBundle)
        {
            foreach (Button addButton in addButtons)
            {
                addButton.onClick.AddListener(() =>
                {
                    string name_id = inputField.text;
                    Debug.Log("Link added " + name_id);
                    AddMatToList(name_id);
                });
            }
        }

        // Set the initial skybox (optional)
        OnSkyboxChanged(skyboxDropdown.value);
    }

    // ---------- Remote (Drive) path — unchanged ----------

    public void AddMatToList(string bundleId)
    {
        if (!useRemoteBundle)
        {
            Debug.LogWarning("useRemoteBundle is false; ignoring remote add request.");
            return;
        }

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

        if (assetBundleCache.TryGetValue(bundleUrl, out var cachedBundle))
        {
            Debug.Log("Bundle already present in local cache");
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
        string[] assetNames = assetBundle.GetAllAssetNames();
        string cleanAssetName = Path.GetFileNameWithoutExtension(assetNames[0]);

        Debug.Log("Bundle name: " + cleanAssetName);

        GameObject skyboxObject = assetBundle.LoadAsset<GameObject>(cleanAssetName);

        if (skyboxObject != null)
        {
            Debug.Log("Loaded GameObject: " + skyboxObject.name);

            Skybox skyboxComponent = skyboxObject.GetComponent<Skybox>();

            if (skyboxComponent != null)
            {
                Material customSkybox = skyboxComponent.material;

                if (customSkybox != null)
                {
                    customSkybox.shader = Shader.Find("Skybox/Cubemap");

                    inputField.text = "";
                    downloading.SetActive(false);
                    downloading.GetComponent<Animator>().enabled = false;
                    foreach (Button addButton in addButtons)
                        addButton.gameObject.SetActive(true);
                    clearButton.gameObject.SetActive(true);
                    copyButton.gameObject.SetActive(true);

                    Debug.Log("Custom Skybox Material: " + customSkybox.name);

                    if (!skyboxes.Contains(customSkybox))
                    {
                        skyboxes.Add(customSkybox);
                        skyboxDropdown.options.Add(new TMP_Dropdown.OptionData(customSkybox.name));
                        skyboxDropdown.RefreshShownValue();
                        skyboxDropdown.value = skyboxes.Count - 1;
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

    // ---------- Shared ----------

    private void OnSkyboxChanged(int index)
    {
        if (index >= 0 && index < skyboxes.Count)
        {
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

    public void NextSkybox()
    {
        int currentIndex = skyboxDropdown.value;
        int nextIndex = (currentIndex + 1) % skyboxes.Count;
        skyboxDropdown.value = nextIndex;
        OnSkyboxChanged(nextIndex);
    }

    public void PreviousSkybox()
    {
        int currentIndex = skyboxDropdown.value;
        int prevIndex = (currentIndex - 1 + skyboxes.Count) % skyboxes.Count;
        skyboxDropdown.value = prevIndex;
        OnSkyboxChanged(prevIndex);
    }
}