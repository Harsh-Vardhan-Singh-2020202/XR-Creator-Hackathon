using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

public class DeselectUIElements : MonoBehaviour
{
    [Header("UI Elements to Monitor")]
    [SerializeField] private List<TMP_Dropdown> dropdowns = new List<TMP_Dropdown>();
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private List<TMP_InputField> inputFields = new List<TMP_InputField>();

    /// <summary>
    /// Deselects the currently selected UI button or selectable element.
    /// </summary>
    public void Deselect()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Deselect the current UI element
        }
    }

    private void Start()
    {
        AddDeselectListeners();
    }

    /// <summary>
    /// Adds the Deselect() function as a listener to all UI elements in the lists.
    /// </summary>
    private void AddDeselectListeners()
    {
        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            if (dropdown != null)
            {
                dropdown.onValueChanged.AddListener(delegate { Deselect(); });
            }
        }

        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.onClick.AddListener(Deselect);
            }
        }

        foreach (TMP_InputField inputField in inputFields)
        {
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(delegate { Deselect(); });
            }
        }
    }

    /// <summary>
    /// Adds a TMP_Dropdown to the list and attaches the Deselect listener.
    /// </summary>
    public void AddDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown != null && !dropdowns.Contains(dropdown))
        {
            dropdowns.Add(dropdown);
            dropdown.onValueChanged.AddListener(delegate { Deselect(); });
        }
    }

    /// <summary>
    /// Adds a Button to the list and attaches the Deselect listener.
    /// </summary>
    public void AddButton(Button button)
    {
        if (button != null && !buttons.Contains(button))
        {
            buttons.Add(button);
            button.onClick.AddListener(Deselect);
        }
    }

    /// <summary>
    /// Adds a TMP_InputField to the list and attaches the Deselect listener.
    /// </summary>
    public void AddInputField(TMP_InputField inputField)
    {
        if (inputField != null && !inputFields.Contains(inputField))
        {
            inputFields.Add(inputField);
            inputField.onEndEdit.AddListener(delegate { Deselect(); });
        }
    }
}