using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI.Chat;
using OpenAI;
using OpenAI.Models;
using System.Linq;

public class Planner : ChatBot
{
    public TMP_InputField input_TMP;
    public TMP_InputField output_TMP;
    public int num_plans_finalized = 0;
    public bool memoryless;

    // Start is called before the first frame update
    public async Task ConverseWithUser(string input_str)
    {
        var api = new OpenAIClient();

        ChatHistory.Add(new Message(Role.User, input_str));
        history += "user: \n" + input_str + "\n\n";

        var chatRequest = new ChatRequest(ChatHistory, Model.GPT4, temperature: Temperature, maxTokens: MaxTokens);
        string fullResult = "";
        history += "assistant: \n";
        output_TMP.text = "";
        await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {

            output_TMP.text += result.FirstChoice.Message.Content.ToString(); // display responses on the output window
            fullResult += result.FirstChoice.Message.Content.ToString();
            history += result.FirstChoice.Message.Content.ToString();
        });

        ChatHistory.Add(new Message(Role.Assistant, fullResult));
        history += "\n\n";

        // processing at the end of conversation
        if (fullResult.Trim() == "[Conversation finished]")
        {
            // ask GPT to summarize its own plan
            string input_summarization = "Present the final plan.";
            await ConverseWithUser(input_summarization);
            // reset its memory to only remember the finalized plans
            num_plans_finalized += 1;
            ResetMemoryAfterConversation(output_TMP.text);
        }
    }

    void ResetMemoryAfterConversation(string newest_plan)
    {   
        if (memoryless)
        {
            ClearChatHistory();
        }
        else
        {
            // take the metaprompt and the previously finalized plans
            List<Message> temp = ChatHistory.Take(num_plans_finalized).ToList();
            // take the newly finalized plan
            string preamble = "Here's a plan we previously discussed: \n";
            temp.Add(new Message(Role.Assistant, preamble + newest_plan));
            // reset the chat memory
            ChatHistory = temp;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
