using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class GPTModelConnector : MonoBehaviour
{
    // Your OpenAI API key
    private string apiKey = "sk-proj-Jaido8hb9CKE3Afa6am3T3BlbkFJfJhAk8MdAmJEHk86a2SC";
    // The endpoint URL for your GPT model
    private string modelEndpoint = "https://api.openai.com/v1/engines/YOUR_MODEL_NAME/completions"; //https://chat.openai.com/g/g-9vtU1Fa5F-json-animation-generator

    // Function to send a prompt to the GPT model and log the response
    IEnumerator SendPromptToGPT(string prompt)
    {
        var request = new UnityWebRequest(modelEndpoint, "POST");
        string requestData = JsonUtility.ToJson(new { prompt = prompt, max_tokens = 50 });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    // Example usage
    void Start()
    {
        StartCoroutine(SendPromptToGPT("Hello, world!"));
    }
}
