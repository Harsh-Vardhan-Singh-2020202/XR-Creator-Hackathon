using System.IO;
using HuggingFace.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechRecognition : MonoBehaviour
{
    [SerializeField] private Button audioButton;
    [SerializeField] private Button[] sendButtons;
    [SerializeField] private Image audioButtonImage;
    [SerializeField] private TMP_Text inputField;
    [SerializeField] private AudioClip recordingOn;
    [SerializeField] private AudioClip recordingOff;
    [SerializeField] private AudioSource recordingIndicator;

    private string speechToText;
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Start()
    {
        recording = false;
        audioButton.onClick.AddListener(ToggleRecorder);
        audioButtonImage.color = Color.white;
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            inputField.text = "Error!";
            StopRecording();
        }
    }

    private void ToggleRecorder()
    {
        if (!recording)
        {
            audioButtonImage.color = Color.red;
            recordingIndicator.PlayOneShot(recordingOn);
            StartRecording();
        }
        else
        {
            audioButtonImage.color = Color.white;
            recordingIndicator.PlayOneShot(recordingOff);
            StopRecording();
        }
    }

    private void StartRecording()
    {
        inputField.text = "Recording...";
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
        foreach (Button sendButton in sendButtons)
        {
            sendButton.gameObject.SetActive(false);
        }
    }

    private void StopRecording()
    {
        if (!recording) return;

        int position = Microphone.GetPosition(null);
        Microphone.End(null);

        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);

        recording = false;
        SendRecording();
    }

    public void TerminateRecording()
    {
        if (!recording) return;

        // Stop microphone recording immediately
        Microphone.End(null);
        recording = false;

        // Reset UI
        audioButtonImage.color = Color.white;
        inputField.text = "";
    }

    private void SendRecording()
    {
        inputField.text = "Analysing Speech...";
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            inputField.text = response;
            foreach (Button sendButton in sendButtons)
            {
                sendButton.gameObject.SetActive(true);
            }
        }, error =>
        {
            inputField.text = "Error!";
            foreach (Button sendButton in sendButtons)
            {
                sendButton.gameObject.SetActive(true);
            }
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}