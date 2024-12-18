using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceholderVisibility : MonoBehaviour
{
    [SerializeField] private TMP_Text PlaceHolderField;
    [SerializeField] private TMP_Text InputField;

    // Update is called once per frame
    void Update()
    {
        if (InputField.text == "")
            PlaceHolderField.gameObject.SetActive(true);
        else
            PlaceHolderField.gameObject.SetActive(false);
    }
}
