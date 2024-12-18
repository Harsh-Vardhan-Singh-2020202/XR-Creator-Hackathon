using UnityEngine;
using UnityEngine.UI;

public class ToggleMuteButton : MonoBehaviour
{
    [Header("Mute Button Settings")]
    [SerializeField, Tooltip("Button for toggling mute")]
    private Button muteButton;

    [SerializeField, Tooltip("Image to change color based on mute state")]
    private Image muteButtonImage;

    [SerializeField, Tooltip("Color for mute state")]
    private Color muteColor = Color.red;

    [SerializeField, Tooltip("Color for unmute state")]
    private Color unmuteColor = Color.white;

    [Header("Restart Button Settings")]
    [SerializeField, Tooltip("Button for restarting audio")]
    private Button restartButton;

    [SerializeField, Tooltip("Audio Source to control")]
    private AudioSource audioSource;

    private bool isMuted = false;

    private void Start()
    {
        // Ensure buttons, image, and audio source are assigned
        if (muteButton == null || muteButtonImage == null || restartButton == null || audioSource == null)
        {
            Debug.LogError("MuteButton, MuteButtonImage, RestartButton, or AudioSource not assigned!");
            return;
        }

        // Set initial states
        UpdateMuteState();

        // Add listeners for button clicks
        muteButton.onClick.AddListener(ToggleMute);
        restartButton.onClick.AddListener(RestartAudio);
    }

    private void ToggleMute()
    {
        // Toggle the mute state
        isMuted = !isMuted;

        // Update the audio and visual states
        UpdateMuteState();
    }

    private void UpdateMuteState()
    {
        // Mute/unmute the audio
        AudioListener.volume = isMuted ? 0 : 1;

        // Change button color based on mute state
        muteButtonImage.color = isMuted ? muteColor : unmuteColor;
    }

    private void RestartAudio()
    {
        // Restart the audio from the beginning
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.Play();
    }
}