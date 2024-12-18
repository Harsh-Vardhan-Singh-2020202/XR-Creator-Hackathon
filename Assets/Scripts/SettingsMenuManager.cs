using System.Collections;
using System.Collections.Generic;
using Christina.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for the Toggle
using UnityEngine.XR.ARFoundation; // For ARPlane and ARPlaneMeshVisualizer

public class SettingsMenuManager : MonoBehaviour
{
    private List<ARPlaneMeshVisualizer> ARPlaneMeshVisualizers = new List<ARPlaneMeshVisualizer>();

    [Header("Toggle Settings")]
    [SerializeField] private Image transitionImage;  // Image to change color
    [SerializeField] private string activeColorHex = "#00FF00";  // Active color
    [SerializeField] private string deactiveColorHex = "#FF0000";  // Deactive color
    [SerializeField] private ToggleSwitch toggleSwitch;

    [Header("Voice List")]
    [SerializeField] private TMP_Dropdown voiceDropdown;
    [SerializeField] private List<string> voiceList = new List<string> { "James", "Morgan", "Amy", "Lily" };  // List of voices

    private bool ShowPlane = true;

    private Color activeColor;
    private Color deactiveColor;

    void Start()
    {
        // Parse the hex codes into Color objects
        activeColor = HexToColor(activeColorHex);
        deactiveColor = HexToColor(deactiveColorHex);

        // Initial population of ARPlaneMeshVisualizers
        UpdateARPlaneMeshVisualizers();

        // Set the toggle to active initially and perform actions
        toggleSwitch.ToggleByGroupManager(true);  // Ensure toggle is ON

        // Enable the plane visualizer and trigger color change to active
        PlaneVisualizerOn();
    }

    void Update()
    {
        // Keep updating the list of ARPlaneMeshVisualizers dynamically
        UpdateARPlaneMeshVisualizers();

        foreach (var visualizer in ARPlaneMeshVisualizers)
        {
            visualizer.enabled = ShowPlane;
        }

        // Assign voice based on dropdown selection
        AssignVoiceToTextToSpeech();
    }

    private void UpdateARPlaneMeshVisualizers()
    {
        // Find all objects with the ARPlane script
        ARPlaneMeshVisualizers.Clear(); // Clear the list to refresh it

        ARPlane[] planes = FindObjectsOfType<ARPlane>();
        foreach (var plane in planes)
        {
            ARPlaneMeshVisualizer visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (visualizer != null && !ARPlaneMeshVisualizers.Contains(visualizer))
            {
                ARPlaneMeshVisualizers.Add(visualizer);
            }
        }
    }

    public void PlaneVisualizerOn()
    {
        ShowPlane = true;

        // Start the color transition to the active color
        StartCoroutine(TransitionColor(activeColor));
    }

    public void PlaneVisualizerOff()
    {
        ShowPlane = false;

        // Start the color transition to the deactive color
        StartCoroutine(TransitionColor(deactiveColor));
    }

    private IEnumerator TransitionColor(Color targetColor)
    {
        // Ensure the image is not null
        if (transitionImage == null) yield break;

        // Get the starting color
        Color startColor = transitionImage.color;

        float elapsedTime = 0f;
        while (elapsedTime < toggleSwitch.animationDuration)
        {
            // Lerp the color from the start color to the target color over time
            transitionImage.color = Color.Lerp(startColor, targetColor, elapsedTime / toggleSwitch.animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final color is set to the target color
        transitionImage.color = targetColor;
    }

    // Helper function to convert hex color code to Unity Color
    private Color HexToColor(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1); // Remove the "#" if present
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    private void AssignVoiceToTextToSpeech()
    {
        // Get all TextToSpeech components in the scene
        TextToSpeech[] textToSpeeches = FindObjectsOfType<TextToSpeech>();

        // Loop through each TextToSpeech object
        foreach (var textToSpeech in textToSpeeches)
        {
            // Get the selected voice based on the dropdown value
            string selectedVoice = GetVoiceByDropdownSelection();

            // Assign the selected voice to the TextToSpeech component
            textToSpeech.voice = selectedVoice;
        }
    }

    private string GetVoiceByDropdownSelection()
    {
        // Get the selected voice from the voice list based on the dropdown index
        int selectedIndex = voiceDropdown.value;

        // Ensure the index is within the bounds of the voice list
        if (selectedIndex >= 0 && selectedIndex < voiceList.Count)
        {
            return voiceList[selectedIndex];
        }
        else
        {
            // Default to "James" if the index is out of bounds
            return "James";
        }
    }
}