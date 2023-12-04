using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;


public class CSHttpClientSample : MonoBehaviour
{
    public Texture2D local_img;

    public string local_img_path = "Assets/Images/CurrentScene.jpg";

    public bool use_online_img;
    public DetectedObject[] detected_objects;
    public GameObject relationalGPT_manager;
    //public string obj_descriptions;
    public GameObject img_parser;
    public GameObject objectGPTInput;

    public GameObject scenePrompt;
    public GameObject relationalGPTOutput;
    public bool dense_captioning;

    // {"url":"https://portal.vision.cognitive.azure.com/dist/assets/ImageCaptioningSample1-bbe41ac5.png"}
    void Start()
    {
        //MakeRequest();
    }

    public async void MakeRequest()
    {
        var client = new HttpClient();
        var queryString = HttpUtility.ParseQueryString(string.Empty);

        // Request headers
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "c6772bd141de4d7fa462a793a32f4353");

        // Request parameters
        if (dense_captioning)
        {
            queryString["features"] = "denseCaptions";
        }
        else
        {
            queryString["features"] = "objects";
        }

        queryString["language"] = "en";
        queryString["model-version"] = "latest";

        //var uri = "https://eastus.api.cognitive.microsoft.com/vision/v3.2/analyze?" + HttpUtility.UrlEncode(queryString.ToString());
        var uri = "https://eastus.api.cognitive.microsoft.com/computervision/imageanalysis:analyze?api-version=2023-02-01-preview&" + queryString;

        HttpResponseMessage response;

        // Img
        byte[] byteData;
        string request_header;
        if (use_online_img)
        {
            request_header = "application/json";
            byteData = Encoding.UTF8.GetBytes(@"{""url"":""https://portal.vision.cognitive.azure.com/dist/assets/ImageCaptioningSample1-bbe41ac5.png""}");
        }
        else
        {
            request_header = "application/octet-stream";
            // to change to local image
            //Texture2D copy = new Texture2D(local_img.width, local_img.height, TextureFormat.RGBA32, false);
            Texture2D local_img = new Texture2D(2, 2); // Create a dummy texture
            local_img.LoadImage(File.ReadAllBytes(local_img_path)); // Load the image from the file
            Texture2D copy = new Texture2D(local_img.width, local_img.height, TextureFormat.RGBA32, false); // Create a copy texture


            // To copy the pixel data from the original texture to the copy, use:
            copy.SetPixels(local_img.GetPixels());
            copy.Apply();
            byteData = copy.EncodeToPNG();
            //File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", byteData);
            Destroy(copy);
        }

        using (var content = new ByteArrayContent(byteData))
        {
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue(request_header);
            response = await client.PostAsync(uri, content);
        }

        // Handle the response
        string result = null;
        if (response.IsSuccessStatusCode)
        {
            // Parse the response as JSON and do something with it
            result = await response.Content.ReadAsStringAsync();
            Debug.Log(result);
        }
        else
        {
            // Handle the error
            Debug.LogError(response.StatusCode + " " + response.ReasonPhrase);
        }

        JObject result_json = JObject.Parse(result);
        process_response(result_json);
        //return detected_objects;
        float W = (float)result_json["metadata"]["width"];
        float H = (float)result_json["metadata"]["height"];
        var detected_obj_str = detected_objs_to_str(detected_objects, W, H);
        Debug.Log(detected_obj_str[0]);
        //obj_descriptions = detected_obj_str[1];
        img_parser.GetComponent<ImageParser>().obj_descriptions = detected_obj_str[1];

        // Begin making calls to GPT
        // TODO: consider decouple this part - need to know how to listen to async to finish 

        // First, let relationalGPT figure out a sensible placement of the objects in the scene
        // send the partially computed coordinates to relationalGPT and let it figure out what the depth of these objects should be
        string originalScenePrompt = scenePrompt.GetComponent<TextMeshPro>().text;
        relationalGPT_manager.GetComponent<ChatRelationalGPT>().LetOrchestraChangeInput(detected_obj_str[0]);
        await relationalGPT_manager.GetComponent<ChatRelationalGPT>().TestChatStream();

        // Second, change the objectGPT's input to reflect the relational information. Afterwards, press TAB to create a scene.
        string objectGPT_input = objectGPTInput.GetComponent<TextMeshPro>().text;
        objectGPT_input += "\n Create the following list of objects: " + detected_obj_str[1];
        objectGPT_input += "\n Their locations are: " + relationalGPTOutput.GetComponent<TextMeshPro>().text;
        //string coord_instructions = ''
        objectGPTInput.GetComponent<TextMeshPro>().text = objectGPT_input;
    }

    void process_response(JObject response)
    {
        // image dimensions
        float W = (float)response["metadata"]["width"];
        float H = (float)response["metadata"]["height"];
        string result_name = "";
        string mode_name = "";
        if (dense_captioning)
        {
            result_name = "denseCaptionsResult";
            mode_name = "denseCaptioning";
        }
        else
        {
            result_name = "objectsResult";
            mode_name = "objects";
        }

        var object_list = (JArray)response[result_name]["values"];

        // TODO: filter object list by confidence 
        int n_objs = object_list.Count;
        detected_objects = new DetectedObject[n_objs];

        // By the end, detected_objects should be a list of each object's center (of its bounding box) as well as its description.
        for (int i = 0; i < n_objs; i++)
        {
            var obj = object_list[i];
            var bbox = obj["boundingBox"];
            //Debug.Log(obj);
            var x = bbox["x"].ToObject<float>();
            var y = bbox["y"].ToObject<float>();
            var w = bbox["w"].ToObject<float>();
            var h = bbox["h"].ToObject<float>();
            DetectedObject detected_obj = new DetectedObject();
            detected_obj.center = Utils.get_bb_center(new Vector2(x, y), w, h, W, H, mode_name);
            if (dense_captioning)
            {
                detected_obj.description = (string)obj["text"];
            }
            else
            {
                detected_obj.description = (string)obj["tags"][0]["name"];
            }

            detected_obj.box_dim = new Vector2(w, h);
            detected_objects[i] = detected_obj;
        }

    }

    List<string> detected_objs_to_str(DetectedObject[] detected_objects, float W, float H)
    {
        string obj_description = "[";
        foreach (DetectedObject obj in detected_objects)
        {
            obj_description += obj.description + ",";
        }
        obj_description = obj_description.TrimEnd(',');
        obj_description += "]";

        string obj_center = "[";
        foreach (DetectedObject obj in detected_objects)
        {
            obj_center += "(" + obj.center.x.ToString("n2") + "," + obj.center.y.ToString("n2") + ") ";
        }
        obj_center += "]";

        string obj_bb_dim = "[";
        foreach (DetectedObject obj in detected_objects)
        {
            obj_bb_dim += "(" + obj.box_dim.x.ToString("n2") + "," + obj.box_dim.y.ToString("n2") + ") ";
        }
        obj_bb_dim += "]";

        string meta_str = "{" + W.ToString("n2") + ", " + H.ToString("n2") + "}";

        List<string> list_str = new List<string>();
        list_str.Add(obj_description + obj_center + obj_bb_dim + meta_str);
        list_str.Add(obj_description);
        list_str.Add(obj_center);
        list_str.Add(obj_bb_dim);

        return list_str;
    }

}

public class DetectedObject
{
    public Vector2 center;
    public string description;
    public Vector2 box_dim;
}