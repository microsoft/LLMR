using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggerGPT : ChatBot
{
    public int max_debugging_count;

    public string ParseDebuggerResult()
    {
        return ".";
    }

    public string ParseDebuggerResultSimple(string generated_code, string err_msg)
    {
        string ret = "";
        ret += "The code you just wrote has some compiler errors.";
        //ret += "Code: " + generated_code + '\n';
        ret += "Compiler error message: " + err_msg + '\n';
        ret += "Please modify the code so that the compiler error is no longer present.";

        return ret;
    }

    public string ParseDebuggerResultSimple(string generated_code, string err_msg, bool builder_is_memoryless)
    {
        string ret = "";

        if (builder_is_memoryless)
        {
            ret += "This code has some compiler errors.";
            ret += "Code: " + generated_code + '\n';
        }
        else
        {
            ret += "The code you just wrote has some compiler errors.";
        }

        ret += "Compiler error message: " + err_msg + '\n';
        ret += "Please modify the code so that the compiler error is no longer present.";

        return ret;
    }
}
