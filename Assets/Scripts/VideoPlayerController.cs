using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Video Player Settings")]
    [SerializeField, Tooltip("VideoPlayer component to control")]
    private VideoPlayer videoPlayer;

    [Header("UI Elements")]
    [SerializeField, Tooltip("Slider to control video volume")]
    private Slider volumeSlider;

    [SerializeField, Tooltip("Slider to control playback speed")]
    private Slider speedSlider;

    [SerializeField, Tooltip("Button to restart the video")]
    private Button restartButton;

    [SerializeField, Tooltip("Button to toggle video play/pause")]
    private Button togglePlayButton;

    [SerializeField, Tooltip("Image to indicate play/pause state")]
    private Image togglePlayButtonImage;

    [SerializeField, Tooltip("Sprite for play state")]
    private Sprite playSprite;

    [SerializeField, Tooltip("Sprite for pause state")]
    private Sprite pauseSprite;

    private bool isPlaying = true;

    private void Start()
    {
        // Ensure all references are assigned
        if (videoPlayer == null || volumeSlider == null || speedSlider == null ||
            restartButton == null || togglePlayButton == null || togglePlayButtonImage == null)
        {
            Debug.LogError("One or more UI components or the VideoPlayer is not assigned!");
            return;
        }

        // Add listeners for UI interactions
        volumeSlider.onValueChanged.AddListener(SetVolume);
        speedSlider.onValueChanged.AddListener(SetPlaybackSpeed);
        restartButton.onClick.AddListener(RestartVideo);
        togglePlayButton.onClick.AddListener(TogglePlayPause);


        // Update the starts
        UpdatePlayPauseButton();
        SetVolume(1.0f);
        SetPlaybackSpeed(4);
    }

    private void SetVolume(float value)
    {
        videoPlayer.SetDirectAudioVolume(0, value);
    }

    private void SetPlaybackSpeed(float value)
    {
        if (value == 0)
            videoPlayer.playbackSpeed = 0.25f;
        if (value == 1)
            videoPlayer.playbackSpeed = 0.25f;
        if (value == 2)
            videoPlayer.playbackSpeed = 0.50f;
        if (value == 3)
            videoPlayer.playbackSpeed = 0.75f;
        if (value == 4)
            videoPlayer.playbackSpeed = 1.00f;
        if (value == 5)
            videoPlayer.playbackSpeed = 1.25f;
        if (value == 6)
            videoPlayer.playbackSpeed = 1.50f;
        if (value == 7)
            videoPlayer.playbackSpeed = 1.75f;
        if (value == 8)
            videoPlayer.playbackSpeed = 2.00f;
    }

    private void RestartVideo()
    {
        videoPlayer.Stop();
        videoPlayer.Play();
        isPlaying = true;
        UpdatePlayPauseButton();
    }

    private void TogglePlayPause()
    {
        if (isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }

        isPlaying = !isPlaying;
        UpdatePlayPauseButton();
    }

    private void UpdatePlayPauseButton()
    {
        togglePlayButtonImage.sprite = isPlaying ? pauseSprite : playSprite;
    }
}
