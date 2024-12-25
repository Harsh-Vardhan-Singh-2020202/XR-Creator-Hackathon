using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CycleMenu : MonoBehaviour
{
    public Button nextButton; // Reference to the Next button
    public Button backButton; // Reference to the Back button
    public GameObject parentObject; // The parent object containing the 3D child objects
    public TMP_Text infoText; // Reference to the text element for displaying information

    public List<string> objectName; // List containing the names of the objects

    private int currentIndex = 0; // To keep track of the currently selected object
    private int totalObjects; // Total number of 3D objects in the parent object

    void Start()
    {
        // Initialize totalObjects with the number of children in the parent object
        totalObjects = parentObject.transform.childCount;

        // Add listeners for the buttons
        nextButton.onClick.AddListener(OnNextButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Initialize by setting the first object active
        SetActiveObject(currentIndex);
    }

    void OnNextButtonClicked()
    {
        // Increment the current index and wrap around if necessary
        currentIndex = (currentIndex + 1) % totalObjects;
        SetActiveObject(currentIndex);
    }

    void OnBackButtonClicked()
    {
        // Decrement the current index and wrap around if necessary
        currentIndex = (currentIndex - 1 + totalObjects) % totalObjects;
        SetActiveObject(currentIndex);
    }

    void SetActiveObject(int index)
    {
        // Deactivate all children first
        foreach (Transform child in parentObject.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Activate the object at the current index
        parentObject.transform.GetChild(index).gameObject.SetActive(true);

        // Update the infoText with the name from the objectName list at the current index
        if (objectName != null && objectName.Count > index)
        {
            infoText.text = objectName[index]; // Update the text with the name
        }
        else
        {
            infoText.text = "Unknown Object"; // Fallback in case the list is not filled properly
        }
    }
}