using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI.Models;
using UnityEngine.UI;
using TMPro;

public class ChatRelationalGPT : MonoBehaviour
{
    public GameObject Input;
    public GameObject Output;
    public GameObject History;
    public Text SystemContext;


    public int MaxTokens = 512;
    private List<string> stopValues = new List<string>();

    [Tooltip("Temperature value has to be between 0 and 1.")]
    public double Temperature = 0.7;

    [Tooltip("Frequency penalty value has to be between 0 and 2.")]
    public double FrequencyPenalty;

    List<ChatPrompt> ChatHistory = new List<ChatPrompt>();



    void Start()
    {
        stopValues.Add("/*");
        stopValues.Add("</");

        ChatHistory.Add(new ChatPrompt("system", SystemContext.text));

        //Debug.Log("running test");
        //var testChat = TestChat();
        //var teststream = TestChatStream();
    }

    public void ChatCompletion()
    {
        var testChat = TestChat();
    }

    public void LetOrchestraChangeInput(string GPTorchestratorstring)
    {
        Input.GetComponent<TextMeshPro>().text = GPTorchestratorstring;
    }



    public async Task TestChat()
    {
        Debug.Log("Sending a chat request: \n" + Input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new ChatPrompt("user", Input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + Input.GetComponent<TextMeshPro>().text + "\n\n";

        // chatPrompts = new List<ChatPrompt>
        //{
        //   new ChatPrompt("system", SystemContext.text),
        //   new ChatPrompt("user", Input.GetComponent<TextMeshPro>().text)
        //};
        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        Debug.Log(result.FirstChoice);
        Output.GetComponent<TextMeshPro>().text = result.FirstChoice.ToString();
        ChatHistory.Add(new ChatPrompt("assistant", result.FirstChoice));

        History.GetComponent<TextMeshPro>().text += "assistant: \n" + result.FirstChoice + "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }

    public async Task TestChatStream()
    {
        Debug.Log("Sending a chat request: \n" + Input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new ChatPrompt("user", Input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + Input.GetComponent<TextMeshPro>().text + "\n\n";

        // chatPrompts = new List<ChatPrompt>
        //{
        //   new ChatPrompt("system", SystemContext.text),
        //   new ChatPrompt("user", Input.GetComponent<TextMeshPro>().text)
        //};
        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        //var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        string fullResult = "";
        History.GetComponent<TextMeshPro>().text += "assistant: \n";
        Output.GetComponent<TextMeshPro>().text = "";
        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            Debug.Log(result.FirstChoice);
            Output.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            History.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
        });

        ChatHistory.Add(new ChatPrompt("assistant", fullResult));

        History.GetComponent<TextMeshPro>().text += "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }

    

}
