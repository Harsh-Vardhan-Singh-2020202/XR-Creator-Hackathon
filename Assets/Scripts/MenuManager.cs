using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private XRReferenceImageLibrary newReferenceLibrary;

    void Start()
    {
        ResetARTracker();
    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ResetARTracker()
    {
        ARTrackedImageManager trackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        if (trackedImageManager == null)
        {
            Debug.Log("ARTrackedImageManager not found in the scene.");
            return;
        }

        // Set the appropriate reference library
        if (newReferenceLibrary != null)
        {
            trackedImageManager.referenceLibrary = newReferenceLibrary;
        }
        else
        {
            Debug.LogWarning("New Reference Library is not assigned.");
        }
    }
}