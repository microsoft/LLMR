/*
 * Cathy Fang, 6/12/2023
 * WidgetSaveReload.cs saves widget script's name and summary and the gameobject the widget script is attached to.
 * It also and reloads widgets.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;    
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class WidgetSaveReload : MonoBehaviour
{
    private string json = "";
    public GameObject roslynCompiler;
    //public GameObject OpenAICompleter; 
    private CancellationTokenSource cts = new CancellationTokenSource();

    string widgetLib = "";
    public string widgetJson = "";
    public GameObject WidgetLibrary;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearWidget()
    {
        PlayerPrefs.SetString("widgetLib", "");

        widgetJson = "";
        Debug.Log("reset widget");
    }

        void OnEnable()
    {
        widgetJson = PlayerPrefs.GetString("widgetLib");
        Debug.Log("on load:" + widgetJson);
    }

    //deprecated
    //public void RegenWidget()
    //{
    //    Debug.Log("RegenWidget");

    //    Dictionary<string,string> script = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>(widgetJson);
    //    string widgetScripts = "";
    //    foreach (KeyValuePair<string,string> pair in script)
    //    {
    //        Debug.Log("key: " + pair.Key + " value: " + pair.Value);
    //        //create a new script that with the class name of the key and the script content of the value
    //        widgetScripts += pair.Value;


    //    }
    //    Run(widgetScripts);
  
    //}

    //async void Run(string widgetSummary)
    //{
    //    try
    //    {
    //        await OpenAICompleter.GetComponent<ChatTest>().ReloadWidget(cts.Token, widgetSummary);
    //        //await OpenAICompleter.GetComponent<IterPrompting>().ReloadWidget(cts.Token, widgetSummary);
    //        //await Task.Delay(500, cts.Token);
    //        Compile();
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError(e.Message);
    //    }
    //}
    //void Compile()
    //{
    //    roslynCompiler.GetComponent<CompileCompletionsWithReferences>().RunCode();
    //}

    public void ReloadWidgetScript()
   {
       Debug.Log("ReloadWidget_Script");



        List<Toggle> toggles = WidgetLibrary.GetComponent<WidgetLibUI>().toggles;
        List<string> widgetNames = new List<string>();
        foreach (Toggle t in toggles)
        {
            if (t.isOn)
            {
                widgetNames.Add(t.GetComponentInChildren<Text>().text);
            }
        }

       Dictionary<string, string> script = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(widgetJson);
       string widgetScripts = "";
       foreach (KeyValuePair<string, string> pair in script)
       {
           //Debug.Log("key: " + pair.Key + " value: " + pair.Value);

           string scriptClassName = pair.Key.ToString();
            //System.Type type = System.Type.GetType(scriptName);
            //this.gameObject.AddComponent<scriptName>();

            if (widgetNames.Contains(scriptClassName))
            {
                scriptClassName = scriptClassName.Substring(0, scriptClassName.Length - 3) + ".txt";
                string previousCodetoRun = ReadScriptFile(scriptClassName);
                roslynCompiler.GetComponent<CompileCompletionsWithReferences>().LoadCode(previousCodetoRun);
            }



       }
     

   }

    public string ReadScriptFile(string GPTClassName)
    {
        Debug.Log("reading: " + GPTClassName);
        GPTClassName = string.IsNullOrEmpty(GPTClassName) ? "SampleCode.txt" : GPTClassName;

        string path = Path.Combine("Scripts", "Scripts_Gen", GPTClassName);
        GPTClassName = Path.Combine(Application.dataPath, path);
        

        //var parsedFile = CSharpSyntaxTree.ParseText(File.ReadAllText(GPTClassName));
        var parsedFile = File.ReadAllText(GPTClassName);
        Debug.Log(parsedFile);
        return parsedFile.ToString();

    }
 


    public void SaveWidget()
    {
        Debug.Log("saveWidget");

        //iterate through the children widget gameobjects and summarize each one
        //foreach (Transform child in this.gameObject.transform)
        //{
        //    WidgetDesc w = new WidgetDesc(child.gameObject);

        //    json += JsonUtility.ToJson(w);
        //    //Debug.Log(json);
        //}
        //Debug.Log(json);
        WidgetDesc w = new WidgetDesc(this.gameObject);
        //json = JsonUtility.ToJson(w);
        widgetJson = w.scripts;
        Debug.Log(widgetJson);
    }

    void OnDisable()
    {
        PlayerPrefs.SetString("widgetLib", widgetJson);
        Debug.Log("on disable set string:" + widgetLib);

    }

    // a class to represent a widget
    [System.Serializable]
    public class WidgetDesc
    {
        //public string objName; //name of widget gameobject that the widget scripts are attached to
        public string scripts;
        private Dictionary<string, string> scriptJson { get; set; }

        //a constructor
        public WidgetDesc(GameObject gameObject)
        {
            //objName = gameObject.name;
            //Debug.Log("name of gameobject: " + objName);
            scriptJson = new Dictionary<string,string>();
            

            //get all components that is a type of c sharp monoscript
            foreach (UnityEngine.Component s in gameObject.GetComponents(typeof(Widgets)))
            {

                Debug.Log("name of script: " + s.GetType());

                System.Type type = s.GetType();
                Widgets targetScript = gameObject.GetComponent(type) as Widgets;
                string scriptSum = targetScript.summary;

                if (scriptSum != null)
                {
                    Debug.Log("summary of script: " + scriptSum);
                }
                else
                {
                    scriptSum = "no summary";
                    Debug.Log("no summary");
                }
                if (!scriptJson.TryAdd(type.ToString() + ".cs", scriptSum)){
                    scriptJson.Add(type.ToString() + ".cs", scriptSum);
                }
                else { Debug.Log("script already added");}

            }
            scripts = Newtonsoft.Json.JsonConvert.SerializeObject(scriptJson);
        }
    }


}
