using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

[System.Serializable]
public class Response
{
    public Candidate[] candidates;
}

public class ChatRequest
{
    public Content[] contents;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

[System.Serializable]
public class Message
{
    public string text;
    public TMP_Text textObject;
}

public class UnityAndGeminiV3 : MonoBehaviour
{
    [Header("JSON API Configuration (legacy - unused if user key is set)")]
    public TextAsset jsonApi; // kept for reference, no longer read by default
    private string apiKey = "";
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

    [Header("User API Key Input")]
    [Tooltip("TMP_InputField where the user pastes their own Gemini API key.")]
    public TMP_InputField apiKeyInputField;
    [Tooltip("Button to confirm/apply the pasted key.")]
    public Button setKeyButton;
    [Tooltip("Optional: shown when no key has been set yet, hidden once one is.")]
    public GameObject noKeyWarning;

    private bool keyIsSet = false;

    [Header("ChatBot Function")]
    public TMP_Text inputField;
    public GameObject chatPanel, textObj_LLM, textObj_User;

    // Dropdown for language selection
    public TMP_Dropdown languageDropdown;

    // Fonts for English and Hindi
    public TMP_FontAsset englishFont;
    public TMP_FontAsset hindiFont;

    private Content[] chatHistory;
    List<Message> messageList = new List<Message>();

    void Start()
    {
        chatHistory = new Content[] { };

        if (setKeyButton != null)
        {
            setKeyButton.onClick.AddListener(ApplyUserApiKey);
        }

        if (noKeyWarning != null)
        {
            noKeyWarning.SetActive(true);
        }
    }

    public void ApplyUserApiKey()
    {
        if (apiKeyInputField == null || string.IsNullOrWhiteSpace(apiKeyInputField.text))
        {
            Debug.LogError("No API key entered.");
            return;
        }

        apiKey = apiKeyInputField.text.Trim();
        keyIsSet = true;

        if (noKeyWarning != null)
            noKeyWarning.SetActive(false);

        Debug.Log("Gemini API key set from user input.");
    }

    public void SendChat()
    {
        if (!keyIsSet)
        {
            Debug.LogError("No API key set. Please enter your Gemini API key first.");
            return;
        }

        if (inputField.text == "")
            return;

        string userMessage = inputField.text;

        // Append restrictions
        string restriction = "Check whether this prompt is related to history, historical monuments, or historical artifacts. If yes, type nothing and then answer their question. Otherwise politely declien citing that you are a history guide.";
        string restrictedUserMessage = userMessage + " " + restriction;

        // Append the language request to the user's input
        string languageRequest = GetLanguageRequest();
        string promptMessage = restrictedUserMessage + " " + languageRequest;

        // Send the user's message to the chat
        SendMessageToChat("User: " + userMessage, textObj_User, GetSelectedFont());
        StartCoroutine(SendChatRequestToGemini(promptMessage));
    }

    public void StartChat(string userMessage)
    {
        if (!keyIsSet)
        {
            Debug.LogError("No API key set. Please enter your Gemini API key first.");
            return;
        }

        // Append restrictions
        string restriction = "Check whether this prompt is related to history, historical monuments, or historical artifacts. If yes, type nothing and then answer their question. Otherwise politely declien citing that you are a history guide.";
        string restrictedUserMessage = userMessage + " " + restriction;

        // Append the language request to the user's input
        string languageRequest = GetLanguageRequest();
        string promptMessage = restrictedUserMessage + " " + languageRequest;

        // Send the user's message to the chat
        SendMessageToChat("User: " + userMessage, textObj_User, GetSelectedFont());
        StartCoroutine(SendChatRequestToGemini(promptMessage));
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        inputField.text = "";

        string url = $"{apiEndpoint}?key={apiKey}";

        Content userContent = new Content
        {
            role = "user",
            parts = new Part[]
            {
                new Part { text = newMessage }
            }
        };

        List<Content> contentsList = new List<Content>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray();

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

        string jsonData = JsonUtility.ToJson(chatRequest);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    Content botContent = new Content
                    {
                        role = "model",
                        parts = new Part[]
                        {
                            new Part { text = reply }
                        }
                    };
                    Debug.Log(reply);

                    SendMessageToChat("Narada: " + reply, textObj_LLM, GetSelectedFont());

                    contentsList.Add(botContent);
                    chatHistory = contentsList.ToArray();
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }

    public void SendMessageToChat(string text, GameObject text_Object, TMP_FontAsset font)
    {
        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(text_Object, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.font = font;

        messageList.Add(newMessage);
    }

    private string GetLanguageRequest()
    {
        switch (languageDropdown.value)
        {
            case 0:
                return "Please respond in English.";
            case 1:
                return "कृपया हिंदी में जवाब दें।";
            default:
                return "Please respond in English.";
        }
    }

    private TMP_FontAsset GetSelectedFont()
    {
        switch (languageDropdown.value)
        {
            case 0:
                return englishFont;
            case 1:
                return hindiFont;
            default:
                return englishFont;
        }
    }

    public void ClearChat()
    {
        for (int i = 0; i < messageList.Count; i++)
        {
            Destroy(messageList[i].textObject.gameObject);
        }
        messageList.Clear();
        chatHistory = new Content[] { };
    }

    public void ClearAllLetters()
    {
        if (inputField != null)
        {
            if (inputField.text.Length != 0)
            {
                inputField.text = "";
            }
        }
    }

    public void PasteFromClipboard()
    {
        if (inputField != null)
        {
            inputField.text = GUIUtility.systemCopyBuffer;
        }
    }
}