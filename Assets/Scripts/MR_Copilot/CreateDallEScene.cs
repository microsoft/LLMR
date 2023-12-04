using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CreateDallEScene : MonoBehaviour
{

    // the canvas to display the image
    public Canvas imageCanvas;

    // the image component to show the image
    public Image image;

    public string flaskURLDalle = "http://localhost:5000/get_scene_from_prompt";

    [Serializable] // add this attribute
    public class PrompScene // create a custom class
    {
        public string user_prompt; // add a public field
        public string dalle_scene_filename; // add another public field
    }

    [Serializable] // add this attribute
    public class APIReturn // create a custom class
    {
        public string dalle_scene_base64; // add a public field
    }

    // the coroutine to send the prompt and get the image
    private IEnumerator SendPromptAndGetImage(string useprompt, Action<string> callback)
    {
        // get the user prompt from the model object
        string userPrompt = useprompt;
        Debug.Log("***"+ useprompt);
        // create a JSON object with the user prompt
        var promptData = new PrompScene();
        promptData.user_prompt = userPrompt;
        // generate a random filename for the DALLE image
        promptData.dalle_scene_filename = userPrompt + "_" + Guid.NewGuid().ToString() + ".png";
        string promptJson = JsonUtility.ToJson(promptData);

        Debug.Log(promptJson);

        // create a POST request with the JSON object as the body
        var request = new UnityWebRequest(flaskURLDalle, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptJson);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // send the request and wait for the response
        yield return request.SendWebRequest();

        // check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // parse the response as a JSON object
            Debug.Log(request.downloadHandler.text);

            string responseText = request.downloadHandler.text;

            var responseData = JsonUtility.FromJson<APIReturn>(responseText);

            // get the image base64 string from the response
            string imageBase64 = responseData.dalle_scene_base64;

            // decode the base64 string into a byte array
            byte[] imageBytes = System.Convert.FromBase64String(imageBase64);

            


            // create a file path for the image
            string filePath = Application.dataPath + "/Images/CurrentScene.jpg";

            // save the image bytes to the file
            File.WriteAllBytes(filePath, imageBytes);

            // optionally, refresh the asset database to show the image in the editor
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
            // create a texture from the byte array
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageBytes);

            // create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // assign the sprite to the image component
            image.sprite = sprite;

            // enable the canvas to show the image
            imageCanvas.enabled = true;

            request.Dispose();
        }
    }

    // the method to find the 3D model closest to the label
    public void GenerateSceneDallE(string prompt, Action<string> callback)
    {
        // start the coroutine to send the prompt and get the image, and assign the uid in the callback
        // pass the model object as a parameter
        StartCoroutine(SendPromptAndGetImage(prompt, callback));
    }
    // Start is called before the first frame update
    void Start()
    {

        // disable the canvas by default
        //imageCanvas.enabled = false;

    }

    
}
