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


public class ChatTest : MonoBehaviour
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

    List<Message> ChatHistory = new List<Message>();


    void Start()
    {
        stopValues.Add("/*");
        stopValues.Add("</");

        ChatHistory.Add(new Message(Role.System, SystemContext.text));

        //cts.CancelAfter(10000);
    }


    public void ChatCompletion()
    {
        var testChat = TestChat();
    }

    public async Task TestChat()
    {
        Debug.Log("Sending a chat request: \n" + input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new Message(Role.User, input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + input.GetComponent<TextMeshPro>().text + "\n\n";

        // chatPrompts = new List<ChatPrompt>
        //{
        //   new ChatPrompt("system", SystemContext.text),
        //   new ChatPrompt("user", input.GetComponent<TextMeshPro>().text)
        //};
        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        Debug.Log(result.FirstChoice);
        Output.GetComponent<TextMeshPro>().text = result.FirstChoice.ToString();
        ChatHistory.Add(new Message(Role.Assistant, result.FirstChoice));

        History.GetComponent<TextMeshPro>().text += "assistant: \n" + result.FirstChoice + "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }



    public async Task TestChatStream()
    {
        Debug.Log("Sending a chat request: \n" + input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new Message(Role.User, input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + input.GetComponent<TextMeshPro>().text + "\n\n";

        // chatPrompts = new List<ChatPrompt>
        //{
        //   new ChatPrompt("system", SystemContext.text),
        //   new ChatPrompt("user", input.GetComponent<TextMeshPro>().text)
        //};
        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        //var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        string fullResult = "";
        History.GetComponent<TextMeshPro>().text += "assistant: \n";
        Output.GetComponent<TextMeshPro>().text = "";
        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            //Debug.Log(result.FirstChoice);
            Output.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            History.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
        });

        ChatHistory.Add(new Message(Role.Assistant, fullResult));

        History.GetComponent<TextMeshPro>().text += "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());

    }



    public async Task TestChatStream(CancellationToken token)
    {
        Debug.Log("Sending a chat request: \n" + input.GetComponent<TextMeshPro>().text);
        var api = new OpenAIClient();

        ChatHistory.Add(new Message(Role.User, input.GetComponent<TextMeshPro>().text));

        History.GetComponent<TextMeshPro>().text += "user: \n" + input.GetComponent<TextMeshPro>().text + "\n\n";

        // chatPrompts = new List<ChatPrompt>
        //{
        //   new ChatPrompt("system", SystemContext.text),
        //   new ChatPrompt("user", input.GetComponent<TextMeshPro>().text)
        //};
        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        //var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        string fullResult = "";
        History.GetComponent<TextMeshPro>().text += "assistant: \n";
        Output.GetComponent<TextMeshPro>().text = "";


        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            //Debug.Log(result.FirstChoice);
            Output.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            History.GetComponent<TextMeshPro>().text += result.FirstChoice.ToString();
            token.ThrowIfCancellationRequested();
        },
        token
        );

        ChatHistory.Add(new Message(Role.Assistant, fullResult));
        Debug.Log("ChatHistoryCount: "+ ChatHistory.Count.ToString());

        History.GetComponent<TextMeshPro>().text +=  "\n\n";
        Debug.Log("ChatHistory: " + ChatHistory.ToString());
    }

    //public async void SendChatViaButton()
    //{
    //    await TestChatStream();
    //}

    //public void ClearChatHistory()
    //{
    //    History.GetComponent<TextMeshPro>().text = "";
    //    ChatHistory = new List<ChatPrompt>();
    //}
    public async Task ReloadWidget(CancellationToken token, string widget)
    {
        Debug.Log("Sending a chat request: \n");
        var api = new OpenAIClient();

        ChatHistory.Add(new Message(Role.User, widget));

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

        ChatHistory.Add(new Message(Role.Assistant, fullResult));
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task test()
    {
        Debug.Log("About to load api");
        var auth = new OpenAIAuthentication("API KEY");
        var api = new OpenAIClient(auth);
        var models = await api.ModelsEndpoint.GetModelsAsync();

        foreach (var model in models)
        {
            Debug.Log(model.ToString());
        }

    }

    public async Task completionTest()
    {

        string prompt =  input.GetComponent<TextMeshPro>().text;

        var auth = new OpenAIAuthentication("API KEY", "organization");
        var api = new OpenAIClient(auth);
        var result = await api.CompletionsEndpoint.CreateCompletionAsync("One Two Three One Two", temperature: 0.1, model: Model.Davinci);
        Debug.Log(result);
        Output.GetComponent<TextMeshPro>().text = result.ToString();
        //History.text += "\n" + input.GetComponent<TextMeshPro>().text + "\n" + Output.GetComponent<TextMeshPro>().text;
    }

}
