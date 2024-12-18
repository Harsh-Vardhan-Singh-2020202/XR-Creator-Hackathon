using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Reference to the camera
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera for better performance
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;

        // Make the button face the camera
        transform.LookAt(mainCamera.transform);

        // Adjust rotation to keep the button upright
        // Flip around to face correctly
        transform.Rotate(0, 180, 0);
    }
}