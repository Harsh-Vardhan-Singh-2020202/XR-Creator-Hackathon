using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InteractionTracker : MonoBehaviour
{
    public bool IsInteracting { get; private set; } = false;

    private void OnEnable()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    private void OnDisable()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
            grabInteractable.selectExited.RemoveListener(OnSelectExit);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        IsInteracting = true;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        IsInteracting = false;
    }
}