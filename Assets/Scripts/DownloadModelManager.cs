using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // For the Button component

public class DownloadModelManager : MonoBehaviour
{
    [Header("Asset Bundle Info")]
    public string bundleUrlTemplate = "https://drive.google.com/uc?export=download&id={0}";
    public List<string> assetBundleIds; // Google Drive file IDs for bundles
    public string assetName = "BundledObject";

    [Header("References")]
    private ObjectSpawner objectSpawner; // Reference to the ObjectSpawner
    private UnityAndGeminiV3 LLM; // Reference to the LLM
    public Button downloadButton; // Button to trigger download

    public GameObject downloadingPrompt; // Reference to the "Downloading Prompt" GameObject
    public GameObject downloadedPrompt; // Reference to the "Downloaded Prompt" GameObject

    public string promptRequest = "Give me a brief about";

    private Dictionary<string, AssetBundle> assetBundleCache = new Dictionary<string, AssetBundle>();

    private void Start()
    {
        // Dynamically find the ObjectSpawner if not assigned
        if (objectSpawner == null)
        {
            objectSpawner = FindObjectOfType<ObjectSpawner>();
            if (objectSpawner == null)
            {
                Debug.LogError("No ObjectSpawner script found in the scene.");
                return;
            }
        }

        // Dynamically find the UnityAndGeminiV3 if not assigned
        if (LLM == null)
        {
            LLM = FindObjectOfType<UnityAndGeminiV3>();
            if (LLM == null)
            {
                Debug.LogError("No UnityAndGeminiV3 script found in the scene.");
                return;
            }
        }

        // Ensure the button and object spawner are assigned
        if (downloadButton != null)
        {
            // Add listener for the button
            downloadButton.onClick.AddListener(() =>
            {
                // Example: Download the first model in the list
                if (assetBundleIds != null && assetBundleIds.Count > 0)
                {
                    downloadButton.gameObject.SetActive(false);
                    downloadingPrompt.SetActive(true);
                    downloadingPrompt.GetComponent<Animator>().enabled = true;
                    AddModelToSpawner(assetBundleIds[0]);
                }
                else
                {
                    Debug.LogError("No asset bundle IDs provided.");
                }
            });
        }
        else
        {
            Debug.LogError("Download button reference is missing.");
        }
    }

    public void AddModelToSpawner(string bundleId)
    {
        string bundleUrl = string.Format(bundleUrlTemplate, bundleId);
        StartCoroutine(DownloadAndPrepareModel(bundleUrl, assetName));
        LLM.StartChat(promptRequest);
    }

    private IEnumerator DownloadAndPrepareModel(string bundleUrl, string assetName)
    {
        // Check if the bundle is already in cache
        if (assetBundleCache.TryGetValue(bundleUrl, out var cachedBundle))
        {
            AddToSpawnerAndDropdown(cachedBundle, assetName);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error while downloading Asset Bundle: {www.error}");
                downloadButton.gameObject.SetActive(true);
                downloadingPrompt.SetActive(false);
                downloadingPrompt.GetComponent<Animator>().enabled = false;
                downloadedPrompt.SetActive(false);
                yield break;
            }

            AssetBundle downloadedBundle = DownloadHandlerAssetBundle.GetContent(www);
            if (downloadedBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                downloadButton.gameObject.SetActive(true);
                downloadingPrompt.SetActive(false);
                downloadingPrompt.GetComponent<Animator>().enabled = false;
                downloadedPrompt.SetActive(false);
                yield break;
            }

            assetBundleCache[bundleUrl] = downloadedBundle;

            if (downloadingPrompt != null)
            {
                downloadingPrompt.SetActive(false);
                downloadingPrompt.GetComponent<Animator>().enabled = false;
            }

            if (downloadedPrompt != null)
                downloadedPrompt.SetActive(true);

            AddToSpawnerAndDropdown(downloadedBundle, assetName);
        }
    }

    private void AddToSpawnerAndDropdown(AssetBundle assetBundle, string assetName)
    {
        GameObject obj = assetBundle.LoadAsset<GameObject>(assetName);
        if (obj != null)
        {
            objectSpawner.AddToSpawnerList(obj);

            Debug.Log($"Model '{obj.name}' successfully added to spawner and dropdown.");
        }
        else
        {
            Debug.LogError($"Asset '{assetName}' not found in the AssetBundle.");
        }
    }
}