using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager1 : MonoBehaviour
{
    [SerializeField] private Button settingsOpenButton;
    [SerializeField] private Button settingsClosedButton;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button modelsOpenButton;
    [SerializeField] private Button modelsClosedButton;
    [SerializeField] private GameObject modelsMenu;

    [SerializeField] private Button infoOpenButton;
    [SerializeField] private Button infoClosedButton;
    [SerializeField] private GameObject infoMenu;

    [SerializeField] private GameObject deleteCurrentButtonHolder;

    // Start is called before the first frame update
    void Start()
    {
        // Assign listeners to the appropriate buttons
        settingsOpenButton.onClick.AddListener(() => OpenMenu(settingsMenu, settingsClosedButton));
        settingsClosedButton.onClick.AddListener(() => CloseMenu(settingsMenu, settingsClosedButton));
        
        modelsOpenButton.onClick.AddListener(() => OpenMenu(modelsMenu, modelsClosedButton));
        modelsClosedButton.onClick.AddListener(() => CloseMenu(modelsMenu, modelsClosedButton));

        infoOpenButton.onClick.AddListener(() => OpenMenu(infoMenu, infoClosedButton));
        infoClosedButton.onClick.AddListener(() => CloseMenu(infoMenu, infoClosedButton));
    }

    private void OpenMenu(GameObject targetObject, Button closeButton)
    {
        // Deactivate main UI Buttons
        settingsOpenButton.gameObject.SetActive(false);
        modelsOpenButton.gameObject.SetActive(false);
        infoOpenButton.gameObject.SetActive(false);
        deleteCurrentButtonHolder.SetActive(false);
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
        modelsOpenButton.gameObject.SetActive(true);
        infoOpenButton.gameObject.SetActive(true);
        deleteCurrentButtonHolder.SetActive(true);
    }
}