using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI.Models;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.IO;

public class IterPrompting : MonoBehaviour
{
    public GameObject input;
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

        //cts.CancelAfter(10000);
    }


    public async Task TestChatStream(CancellationToken token)
    {
        Debug.Log("Sending a chat request: \n" + input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new ChatPrompt("user", input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + input.GetComponent<TextMeshPro>().text + "\n\n";

        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        string fullResult = "";
        History.GetComponent<TextMeshPro>().text += "assistant: \n";
        Output.GetComponent<TextMeshPro>().text = "";


        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            Output.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            History.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            token.ThrowIfCancellationRequested();
        },
        token
        );

        ChatHistory.Add(new ChatPrompt("assistant", fullResult));
        Debug.Log("ChatHistoryCount: "+ ChatHistory.Count.ToString());

        History.GetComponent<TextMeshPro>().text +=  "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }

    public async Task ReloadWidget(CancellationToken token, string widget)
    {
        Debug.Log("Sending a chat request: \n");
        var api = new OpenAIClient();

        ChatHistory.Add(new ChatPrompt("user", widget));

        History.GetComponent<TextMeshPro>().text += "user: \n" + widget + "\n\n";

        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        string fullResult = "";
        History.GetComponent<TextMeshPro>().text += "assistant: \n";
        Output.GetComponent<TextMeshPro>().text = "";


        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            Output.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            History.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            token.ThrowIfCancellationRequested();
        },
        token
        );

        ChatHistory.Add(new ChatPrompt("assistant", fullResult));
        Debug.Log("ChatHistoryCount: " + ChatHistory.Count.ToString());

        History.GetComponent<TextMeshPro>().text += "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }


    public void Undo()
    {
        // remove last chat prompt
        ChatHistory.RemoveAt(ChatHistory.Count - 1); //assistant
        ChatHistory.RemoveAt(ChatHistory.Count - 1); //user
        Debug.Log("ChatHistoryCount: " + ChatHistory.Count.ToString());
        History.GetComponent<TextMeshPro>().text += "most recent chat prompt removed \n\n";
    }


  

    public void ClearChatHistory()
    {
        History.GetComponent<TextMeshPro>().text = "";
        Output.GetComponent<TextMeshPro>().text = "";
        ChatHistory = new List<ChatPrompt>();
        // add back the metaprompt
        ChatHistory.Add(new ChatPrompt("system", SystemContext.text));
    }

    

}
