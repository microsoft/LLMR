using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI.Models;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using TiktokenSharp;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class ChatBot : MonoBehaviour
{
    [TextArea(2, 20)]
    public string metaprompt_file_name;

    [TextArea(10, 100)]
    public string input;

    [TextArea(50, 100)]
    public string output;

    [TextArea(20, 100)]
    public string history;

    [TextArea(5, 20)]
    public string metaprompt;

    [TextArea(2, 20)]
    public string processing_status_text;
    [TextArea(2, 20)]
    public string processing_finished_status_text;

    public string model_name = "gpt-4";
    public int MaxTokens = 512; // max number of tokens per response
    public int context_length = 8000; // model context length
    public TokenManagementOption token_management_option = TokenManagementOption.None;
    private List<string> stopValues = new List<string>();

    [Tooltip("Temperature value has to be between 0 and 1.")]
    public double Temperature = 0.7;

    [Tooltip("Frequency penalty value has to be between 0 and 2.")]
    public double FrequencyPenalty;

    protected List<ChatPrompt> ChatHistory = new List<ChatPrompt>();

    
    protected TikToken tokenizer;

    public enum TokenManagementOption
    {
        // None: Do nothing. Throw error if the max token size is exceeded.
        // FIFO: Eliminate chat history from earliest to latest until enough tokens have been freed up for a new chat
        // Full_Reset: Clear all chat history (except metaprompt) when context window is full
        None, FIFO, Full_Reset
    }

    protected virtual void Awake()
    {
        stopValues.Add("/*");
        stopValues.Add("</");

        if (metaprompt_file_name != "")
        {
            LoadMetapromptFromFile();
        }

        ChatHistory.Add(new ChatPrompt("system", metaprompt));

        // for counting tokens
        tokenizer = TikToken.EncodingForModel(model_name);
    }

    protected void LoadMetapromptFromFile()
    {
        string path = Path.Combine("Scripts", "MetaPrompt", metaprompt_file_name + ".txt");
        // Use Application.dataPath to get the absolute path to the Assets folder
        string fullPath = Path.Combine(Application.dataPath, path);
        // Use File.ReadAllText to read the file contents
        metaprompt = File.ReadAllText(fullPath).ToString();
        // metaprompt = File.ReadAllText(@"Assets\Scripts\MetaPrompt\" + metaprompt_file_name + ".txt").ToString();
    }

    public virtual async Task SendChat()
    {
        // in this case, the new chat will exceed the model's context length
        // manage token size so that there's enough room for a new chat
        if (GetNumTokensForHistoryAndNextChat() > context_length) 
        {
            ManageMemory();
        }

        // send a chat
        OpenAIClient api = new OpenAIClient();
        ChatHistory.Add(new ChatPrompt("user", input));
        history += "user: \n" + input + "\n\n";
        ChatRequest chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        string fullResult = "";
        history += "assistant: \n";
        output = "";

        // wait for the response
        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            output += result.FirstChoice.ToString();
            fullResult += result.FirstChoice.ToString();
            history += result.FirstChoice.ToString();
        });

        ChatHistory.Add(new ChatPrompt("assistant", fullResult));
        history += "\n\n";
    }

    public async Task SendChatWithInput(string chat_input)
    {
        input = chat_input;
        await SendChat();
    }

    public async Task SendNewChat()
    {
        ClearChatMemory();
        await SendChat();
    }


    public int GetNumTokensForHistoryAndNextChat()
    {
        // assumes the next request is stored in input
        int num_tokens_history = GetCurrentChatTokenSize();
        int num_tokens_in_request = tokenizer.GetNumTokens(input);
        int num_tokens_response = MaxTokens;

        return num_tokens_history + num_tokens_in_request + num_tokens_response;
    }

    public int GetCurrentChatTokenSize()
    {
        int num_tokens = 0;
        // loop through all historical chats, add up their token count
        foreach (ChatPrompt chatPrompt in ChatHistory)
        {
            num_tokens += tokenizer.GetNumTokens(chatPrompt.Content);
        }
        return num_tokens;
    }

    public void ManageMemory()
    {
        print("Managing memory");
        if (token_management_option == TokenManagementOption.Full_Reset)
        {
            ClearChatHistory();
        }
        else if (token_management_option == TokenManagementOption.FIFO)
        {
            // ChatHistory should always contain the metaprompt, thus the base Count of 1.
            while (ChatHistory.Count > 1  && GetNumTokensForHistoryAndNextChat() > context_length)
            {
                // delete ChatHistory in pairs (user-assistant) from earliest to latest
                ChatHistory.RemoveAt(1);
                ChatHistory.RemoveAt(1);
            }
        }
    }

    // for debugging, replaces History
    public void DisplayCurrentContext()
    {
        history = "";
        foreach(ChatPrompt chatPrompt in ChatHistory)
        {
            history += chatPrompt.Role + ":" + chatPrompt.Content + '\n';
        }
        print("Current token size: " + GetCurrentChatTokenSize());
    }

    public void SaveCurrentContext(string dir)
    {
        string context = "";
        foreach (ChatPrompt chatPrompt in ChatHistory)
        {
            context += chatPrompt.Role + ":" + chatPrompt.Content + '\n';
        }

        File.WriteAllText(dir, context);
    }

    public async Task SendNewChatWithInput(string chat_input)
    {
        ClearChatMemory();
        input = chat_input;
        await SendChat();
    }

    public async void SendChatViaButton()
    {
        await SendChat();
    }

    public async void SendNewChatViaButton()
    {
        await SendNewChat();
    }

    public void SetMetapromptAndClearHistory(string metaprompt_new)
    {
        metaprompt = metaprompt_new;
        history = "";
        output = "";
        ChatHistory = new List<ChatPrompt>();
        // add back the metaprompt
        ChatHistory.Add(new ChatPrompt("system", metaprompt));
    }

    public async Task ClearChatHistory()
    {
        history = "";
        output = "";
        ChatHistory = new List<ChatPrompt>();
        // add back the metaprompt
        ChatHistory.Add(new ChatPrompt("system", metaprompt));
    }

    // clear GPT's memory, but the user can still inspect the history of all conversation that's happened.
    public async Task ClearChatMemory()
    {
        ChatHistory = new List<ChatPrompt>();
        // add back the metaprompt
        ChatHistory.Add(new ChatPrompt("system", metaprompt));
    }

    public void ClearAllButLastChatMemory()
    {
        // the first element (metaprompt) and last element (most recent chat) remains
        ChatHistory = RemoveMiddleElements(ChatHistory);
    }

    public static List<T> RemoveMiddleElements<T>(List<T> list)
    {
        // If the list is null or has less than 3 elements, return it as it is
        if (list == null || list.Count < 3)
        {
            return list;
        }

        // Otherwise, create a new list with the first and the last elements of the original list
        List<T> result = new List<T>();
        result.Add(list[0]); // add the first element
        result.Add(list[list.Count - 1]); // add the last element

        // Return the new list
        return result;
    }



}
