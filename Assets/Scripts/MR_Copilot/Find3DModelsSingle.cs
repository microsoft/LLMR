using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Find3DModelsSingle : MonoBehaviour
{

    public bool no_UI;
    //private SketchfabLoaderUID loader;
    // define a delegate type for the UID found event
    public delegate void UIDFoundEventHandler(string uid);

    // define an event to raise when the UID is found
    public event UIDFoundEventHandler UIDFound;

    // the URL of the flask app
    public string flaskURLDalle = "http://localhost:5000/get_image_from_prompt";

    public string flaskURLSketchF = "http://localhost:5000/get_closest_skfb_model";

    // the input field for the user prompt
    public TMP_InputField promptInput;

    // the canvas to display the image
    public Canvas imageCanvas;

    // the image component to show the image
    public Image image;

    // the button to trigger the whole process
    public Button processButton;

    // the text component to show the closest SketchFab UID
    public TextMeshPro uidText;

    public List<ModelData> models;

    [Serializable] // add this attribute
    public class PromptData // create a custom class
    {
        public string user_prompt; // add a public field
        public string dalle_image_filename; // add another public field
    }

    [Serializable] // add this attribute
    public class APIReturn // create a custom class
    {
        public string dalle_image_base64; // add a public field
    }

    [Serializable] // add this attribute
    public class UIDReturn // create another custom class
    {
        public string closest_uid; // add a public field
    }


    // the coroutine to send the prompt and get the image

    // the coroutine to send the prompt and get the image
    private IEnumerator SendPromptAndGetImage(ModelData model, Action<string> callback)
    {
        // get the user prompt from the model object
        string userPrompt = model.label;

        // create a JSON object with the user prompt
        var promptData = new PromptData();
        promptData.user_prompt = userPrompt;
        // generate a random filename for the DALLE image
        promptData.dalle_image_filename = userPrompt + "_" + Guid.NewGuid().ToString() + ".png";
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
            string imageBase64 = responseData.dalle_image_base64;

            // decode the base64 string into a byte array
            byte[] imageBytes = System.Convert.FromBase64String(imageBase64);

            // create a texture from the byte array
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageBytes);

            // create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // assign the sprite to the image component
            image.sprite = sprite;

            // enable the canvas to show the image
            imageCanvas.enabled = true;


            // after getting the image, start the coroutine to get the UID and invoke the callback
            // pass the model object as a parameter
            StartCoroutine(SendImageAndGetUID(model, callback));
            request.Dispose();
        }
    }

    // the method to handle the UID found by the coroutines
    private void OnUIDFound(string uid)
    {
        // do something with the uid, for example, download the model
        // this is only for testing purposes
       
        // raise the event with the uid
        UIDFound?.Invoke(uid);
        Debug.Log("UID found: " + uid);
    }

    // the coroutine to send the image filename and get the closest SketchFab UID
    private IEnumerator SendImageAndGetUID(ModelData model, Action<string> callback)
    {
        // create a JSON object with the user prompt and the image filename from the model object
        var promptData = new PromptData();
        promptData.user_prompt = model.label;
        promptData.dalle_image_filename = image.sprite.name;
        string promptJson = JsonUtility.ToJson(promptData);

        Debug.Log(promptJson);

        // create a POST request with the JSON object as the body
        var request = new UnityWebRequest(flaskURLSketchF, "POST");
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

            var responseData = JsonUtility.FromJson<UIDReturn>(responseText);

            // get the closest SketchFab UID from the response
            string closestUID = responseData.closest_uid;

            // assign the UID to the text component
            uidText.text = closestUID;

            request.Dispose();

            // update the model uid with the closest uid
            model.uid = closestUID;

            // invoke the callback with the UID
            callback(closestUID);

            // after invoking the callback, start the coroutine to download the model by the UID
            StartCoroutine(OnDownloadByUID(model));
        }
    }

    // the method to call when the process button is clicked
    public void OnProcessButtonClick()
    {
        // // disable the canvas to hide the previous image
        // imageCanvas.enabled = false;
        // // create a list of models with labels and positions
        // models = new List<ModelData>() {
        //     new ModelData("", "rooster", new Vector3(-0.826f, 0, -7.855f), 1f),
        //     new ModelData("", "clock", new Vector3(0.826f, 0, -7.855f), 0.5f),
        //     new ModelData("", "red chair", new Vector3(0, 0, -7.855f), 0.2f)
        // };

        // // call the Find3DModelsClosestToLabels method with the list and a callback
        // Find3DModelsClosestToLabels(models, (uid, index) =>
        // {
        //     // do something with the uid and the index, for example, update the model data
        //     models[index].uid = uid;
        //     Debug.Log("UID for label: " + uid);
        // });

    }


    // the method to download the model by the UID
    // this method is for testing purposes only 
    public IEnumerator OnDownloadByUID(ModelData model)
    {
        // wait until the SketchfabLoaderUID component is ready
        //SketchfabLoaderUID loader = gameObject.GetComponent<SketchfabLoaderUID>();
        SketchfabLoader loader = gameObject.GetComponent<SketchfabLoader>();


        //yield return new WaitUntil(() => loader.Ready);

        // load the model by its uid and label
        // LOADBYID resizes to 1 by 1 by 1 - find the normalization
        GameObject placecp = loader.LoadByID(model.uid, model.label);
        // GPT decides the scale (prompt it in a way that is a relative size based on the other ones)
        loader.Scale(placecp, model.scale);
        loader.Move(placecp, model.position);

        yield return null;
    }


    // the method to find the 3D model closest to the label
    private void Find3DModelClosestToLabel(ModelData model, Action<string> callback)
    {
        // start the coroutine to send the prompt and get the image, and assign the uid in the callback
        // pass the model object as a parameter
        StartCoroutine(SendPromptAndGetImage(model, callback));
    }

    // the method to find the 3D models closest to a list of models
    public void Find3DModelsClosestToLabels(List<ModelData> models, Action<string, int> callback)
    {
        // use a queue to store the models
        Queue<ModelData> modelQueue = new Queue<ModelData>(models);

        // use a counter to store the number of models to process
        int counter = modelQueue.Count;

        // use an index to keep track of the model position in the list
        int index = 0;

        // define a recursive function to process the queue
        void ProcessQueue()
        {
            // check if the queue is empty
            if (modelQueue.Count == 0)
            {
                // if yes, return
                return;
            }
            // if not, dequeue the first model
            ModelData model = modelQueue.Dequeue();

            // call the Find3DModelClosestToLabel method with the model label and a modified callback
            Find3DModelClosestToLabel(model, (uid) =>
            {
                // invoke the original callback with the uid and the index
                callback(uid, index);

                // increment the index
                index++;

                // decrement the counter
                counter--;

                // check if the counter is zero
                if (counter == 0)
                {
                    // if yes, invoke OnUIDFound with the last uid
                    OnUIDFound(uid);
                }

                // call the ProcessQueue function again to process the next model
                ProcessQueue();
            });
        }

        // start the recursive function
        ProcessQueue();
    }


    // the method to initialize the UI elements
    private void Start()
    {
        if (no_UI)
        {
            // disable the canvas by default
            imageCanvas.enabled = false;

            // add a listener to the process button
            processButton.onClick.AddListener(OnProcessButtonClick);
        }

    }
}