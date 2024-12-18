using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager4 : MonoBehaviour
{
    [SerializeField] private Button settingsOpenButton;
    [SerializeField] private Button settingsClosedButton;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button addSkyboxOpenButton;
    [SerializeField] private Button addSkyboxClosedButton;
    [SerializeField] private GameObject addSkyboxMenu;

    // Start is called before the first frame update
    void Start()
    {
        // Assign listeners to the appropriate buttons
        settingsOpenButton.onClick.AddListener(() => OpenMenu(settingsMenu, settingsClosedButton));
        settingsClosedButton.onClick.AddListener(() => CloseMenu(settingsMenu, settingsClosedButton));

        addSkyboxOpenButton.onClick.AddListener(() => OpenMenu(addSkyboxMenu, settingsClosedButton));
        addSkyboxClosedButton.onClick.AddListener(() => CloseMenu(addSkyboxMenu, settingsClosedButton));
    }

    private void OpenMenu(GameObject targetObject, Button closeButton)
    {
        // Deactivate main UI Buttons
        settingsOpenButton.gameObject.SetActive(false);
        addSkyboxOpenButton.gameObject.SetActive(false);
        // Activate the menu
        targetObject.SetActive(true);
        // Show the close button
        closeButton.gameObject.SetActive(true);
        // Trigger the show animation
        targetObject.GetComponent<Animator>().SetTrigger("show");
    }

    private void CloseMenu(GameObject targetObject, Button closeButton)
    {
        // Hide the close button
        closeButton.gameObject.SetActive(false);
        // Trigger the hide animation
        targetObject.GetComponent<Animator>().SetTrigger("hide");
        // Delay deactivation to allow animation to finish
        StartCoroutine(DeactivateAfterAnimation(targetObject));
    }

    private IEnumerator DeactivateAfterAnimation(GameObject targetObject)
    {
        // Wait for the animation to finish (adjust duration to match your animation)
        yield return new WaitForSeconds(0.5f);
        // Deactivate the menu
        targetObject.SetActive(false);
        // Show the main UI buttons
        settingsOpenButton.gameObject.SetActive(true);
        addSkyboxOpenButton.gameObject.SetActive(true);
    }
}
