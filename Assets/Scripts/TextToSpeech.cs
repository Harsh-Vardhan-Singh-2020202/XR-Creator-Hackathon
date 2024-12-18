using System.Collections;
using System.Collections.Generic;
using LMNT;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UIElements;

public class TextToSpeech : MonoBehaviour
{
    [HideInInspector] public AudioSource _audioSource;
    private string _apiKey;
    private List<Voice> _voiceList;
    private DownloadHandlerAudioClip _handler;

    public TMP_FontAsset englishFont;
    public TMP_Text Narada_Text;
    public GameObject audioButton;
    public GameObject cancelButton;
    public string voice;

    private string dialogue;

    void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _apiKey = LMNTLoader.LoadApiKey();
        _voiceList = LMNTLoader.LoadVoices();
    }

    public IEnumerator Prefetch()
    {
        if (_handler != null)
        {
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("voice", LookupByName(voice));
        form.AddField("text", dialogue);
        using (UnityWebRequest request = UnityWebRequest.Post(Constants.LMNT_SYNTHESIZE_URL, form))
        {
            _handler = new DownloadHandlerAudioClip(Constants.LMNT_SYNTHESIZE_URL, AudioType.WAV);
            request.SetRequestHeader("X-API-Key", _apiKey);
            // TODO: do not hard-code; find a clean way to get package version at runtime
            request.SetRequestHeader("X-Client", "unity/0.1.0");
            request.downloadHandler = _handler;
            yield return request.SendWebRequest();

            _audioSource.clip = _handler.audioClip;
        }
    }

    public IEnumerator Talk()
    {
        if (Narada_Text.font == englishFont)
            dialogue = Narada_Text.text;
        else
            dialogue = "Sorrry! Only english text to speech available currently.";

        dialogue = dialogue.StartsWith("Narada: ") ? dialogue.Substring("Narada: ".Length) : dialogue;

        if (_handler == null)
        {
            StartCoroutine(Prefetch());
        }
        if (_audioSource.clip == null)
        {
            yield return new WaitUntil(() => _audioSource.clip != null);
        }

        // Start playing the audio
        _audioSource.Play();
        cancelButton.SetActive(true);

        // Monitor audio playback and active state
        while (_audioSource.isPlaying)
        {
            // If the GameObject is deactivated mid-audio, stop playback and reset the button
            if (!gameObject.activeSelf)
            {
                _audioSource.Stop(); // Stop audio immediately
                break; // Exit the loop
            }
            yield return null; // Wait for the next frame
        }

        // Ensure the button is reset only if the GameObject is active
        if (gameObject.activeSelf)
        {
            audioButton.SetActive(true);
            cancelButton.SetActive(false);
        }
    }

    private string LookupByName(string name)
    {
        return _voiceList.Find(v => v.name == name).id;
    }

    public void PlayAudio()
    {
        // Stop all other active audio sources
        StopAllOtherAudios();

        // Deactivate the button and start playing this audio
        audioButton.SetActive(false);
        StartCoroutine(Talk());
    }

    public void StopAudio(TextToSpeech TTS_Aud)
    {
        // Stop any ongoing audio playback
        if (TTS_Aud._audioSource.isPlaying)
        {
            TTS_Aud._audioSource.Stop();
        }

        // Ensure the audio button is reactivated
        if (TTS_Aud.audioButton != null && TTS_Aud.cancelButton != null)
        {
            TTS_Aud.audioButton.SetActive(true);
            TTS_Aud.cancelButton.SetActive(false);
        }
    }

    private void StopAllOtherAudios()
    {
        TextToSpeech[] all_TTS_Audios = FindObjectsOfType<TextToSpeech>();

        foreach (TextToSpeech TTS_Audio in all_TTS_Audios)
        {
            // Stop any audio that is currently playing, except for this AudioSource
            if (TTS_Audio != gameObject.GetComponent<TextToSpeech>() && TTS_Audio.gameObject.GetComponent<AudioSource>().isPlaying)
            {
                TTS_Audio.StopAudio(TTS_Audio);
            }
        }
    }

    private void OnDisable()
    {
        StopAudio(gameObject.GetComponent<TextToSpeech>());
    }
}