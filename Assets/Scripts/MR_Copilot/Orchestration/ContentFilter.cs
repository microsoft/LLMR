using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using OpenAI.Chat;
using TiktokenSharp;

public class ContentFilter : ChatBot
{
    // if the Builder also receives scene summary, we use a separate metaprompt that contains
    // examples with scene summaries in them

    public TMP_InputField input_TMP;
    public MemoryOption memory_option;

    public bool receive_scene_summary = false;//placeholder for some scripts that are no longer used (Architect.cs and .. ChatCompilationManagerInput)


    // Start is called before the first frame update
    //protected override void Start()
    //{

    //    ChatHistory.Add(new ChatPrompt("system", metaprompt));

    //    // for counting tokens
    //    tokenizer = TikToken.EncodingForModel(model_name);
    //    //base.Start();
    //}

    // Update is called once per frame
    void Update()
    {
        
    }

    //public async Task SetMetaPrompt(string metaP)
    //{

    //    metaprompt = metaP;
    //    base.Start();
    //    Debug.Log("Done setting meta prompt");
    //}

    public enum MemoryOption
    {
        // The underlying values are ints. Ex: Full_Memory = 0
        Full_Memory, Last_Chat, Memoryless
    }

    // basically, SendChat(), but with slight variations depending on
    // how much memory the Builder has.
    public async Task FilterInput()
    {
        print("Filtering content");
        //input = input_TMP.text;
        input = input_TMP.text;
        if (memory_option == MemoryOption.Full_Memory) // full memory
        {
            await SendChat();
        }
        else if (memory_option == MemoryOption.Last_Chat) // limited memory
        {
            ClearAllButLastChatMemory();
            await SendChat();
        }
        else // no memory
        {
            await SendNewChat();
        }
        
        //output_TMP.text = output;
    }

   

    public void DisplayProcessingStatusText()
    {
        input_TMP.text = processing_status_text;
    }

    public void DisplayProcessingFinishedStatusText()
    {
        input_TMP.text = processing_finished_status_text;
    }

    public bool IsMemoryless()
    {
        if (memory_option == MemoryOption.Memoryless)
        {
            return true;
        }
        return false;
    }


}
