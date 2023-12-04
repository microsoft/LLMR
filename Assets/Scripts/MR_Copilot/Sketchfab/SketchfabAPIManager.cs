//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//using UnityEngine.Networking;
//using UniGLTF;
//using System.IO;
//using System.Text;
//using UnityEngine.UI;
//using TMPro;

//public class SketchfabAPIManager : MonoBehaviour
//{
//    [System.Serializable]
//    public class Archives
//    {
//        public List<Glb> glb;
//        public List<Gltf> gltf;
//        public List<Source> source;
//        public List<Usdz> usdz;
//    }

//    [System.Serializable]
//    public class Avatar
//    {
//        public string uri;
//        public List<Image> images;
//    }

//    [System.Serializable]
//    public class Category
//    {
//        public string name;
//    }

//    [System.Serializable]
//    public class Cursors
//    {
//        public string next;
//        public object previous;
//    }

//    [System.Serializable]
//    public class Glb
//    {
//        public int textureCount;
//        public int size;
//        public string type;
//        public int textureMaxResolution;
//        public int faceCount;
//        public int vertexCount;
//    }

//    [System.Serializable]
//    public class Gltf
//    {
//        public int textureCount;
//        public int size;
//        public string type;
//        public int textureMaxResolution;
//        public int faceCount;
//        public int vertexCount;
//    }

//    [System.Serializable]
//    public class Image
//    {
//        public string uid;
//        public int size;
//        public int width;
//        public string url;
//        public int height;
//    }

//    [System.Serializable]
//    public class License
//    {
//        public string uid;
//        public string label;
//    }

//    [System.Serializable]
//    public class Result
//    {
//        public string uri;
//        public string uid;
//        public string name;
//        public object staffpickedAt;
//        public int viewCount;
//        public int likeCount;
//        public int animationCount;
//        public string viewerUrl;
//        public string embedUrl;
//        public int commentCount;
//        public bool isDownloadable;
//        public DateTime publishedAt;
//        public List<Tag> tags;
//        public List<Category> categories;
//        public Thumbnails thumbnails;
//        public User user;
//        public string description;
//        public int faceCount;
//        public DateTime createdAt;
//        public int vertexCount;
//        public bool isAgeRestricted;
//        public int soundCount;
//        public bool isProtected;
//        public License license;
//        public object price;
//        public Archives archives;
//    }

//    [System.Serializable]
//    public class SFSearchResults
//    {
//        public Cursors cursors;
//        public string next;
//        public object previous;
//        public List<Result> results;
//    }

//    [System.Serializable]
//    public class Source
//    {
//        public object textureCount;
//        public int size;
//        public string type;
//        public object textureMaxResolution;
//        public object faceCount;
//        public object vertexCount;
//    }

//    [System.Serializable]
//    public class Tag
//    {
//        public string name;
//        public string slug;
//        public string uri;
//    }

//    [System.Serializable]
//    public class Thumbnails
//    {
//        public List<Image> images;
//    }

//    [System.Serializable]
//    public class Usdz
//    {
//        public object textureCount;
//        public int size;
//        public string type;
//        public object textureMaxResolution;
//        public object faceCount;
//        public object vertexCount;
//    }

//    [System.Serializable]
//    public class User
//    {
//        public string uid;
//        public string username;
//        public string displayName;
//        public string profileUrl;
//        public string account;
//        public Avatar avatar;
//        public string uri;
//    }

//    [System.Serializable]
//    public class ImplicitAccessToken
//    {
//        public string access_token;
//        public int expires_in;
//        public string token_type;
//        public string scope;
//        public string refresh_token;
//    }

//    [System.Serializable]
//    public class _Glb
//    {
//        public string url;
//        public int size;
//        public int expires;
//    }

//    [System.Serializable]
//    public class _Gltf
//    {
//        public string url;
//        public int size;
//        public int expires;
//    }

//    [System.Serializable]
//    public class _Usdz
//    {
//        public string url;
//        public int size;
//        public int expires;
//    }

//    [System.Serializable]
//    public class _ModelDownloadMetaData
//    {
//        public _Gltf gltf;
//        public _Usdz usdz;
//        public _Glb glb;
//    }

//    public class Model
//    {
//        public GameObject obj;
//        public bool readyToMove;

//        public Model(string name)
//        {
//            obj = new GameObject(name);
//            readyToMove = false;
//        }

//        public Model(GameObject oldObj)
//        {
//            obj = oldObj;
//            readyToMove = false;
//        }
//    }

//    private GameObject spawnedModel;
//    [Header("Mandatory -- Sketchfab Username")] public string username = "t-abanburski@microsoft.com";
//    [Header("Mandatory -- Sketchfab Password")] public string password = "kalamazoo1!";
//    [Space]public string baseUrl;
//    public string keywordToSearch;
//    public int maximumPageLimit = 1;
//    public List<SFSearchResults> searchResults;
//     public Button searchBtn;
//    public TMP_InputField inputField;
//    public GameObject fillbarObject;
//    public UnityEngine.UI.Image fillbarForeground;

//    private string _nextUrl;
//    private int currentPageNumber;
//    private bool shouldLoadNextPage;
//    private string clientId = "sDvwSIxKARJ6m6PZSLLYhmT60ha1rzDYO6J6RUnx";
//    private string clientSecret = "7wbty2FT0fx7S90h9QeGMbzsGvyIHhoQPwd2I0qJYYYVirB4X82ORQk9l4pFOH0783tg9SHiOSOGtVKCIMB9vVcjnu01HFtc15XnJoXcEViF5tZJO87Ub5TMuNbcIeHw";
//    [SerializeField] private ImplicitAccessToken IAT;

//    private void Start()
//    {
//        searchBtn.onClick.AddListener(() => _PerformSearch());

//        //Model apple = LoadModel("apple");
//        //MoveModel(apple, new Vector3(0, 2, 0));
//        //Model banana = LoadModel("banana");
//        //MoveModel(banana, new Vector3(0, -1, 0));
//    }

//    public Model LoadModel(string modelName)
//    {
//        Model model = new Model(modelName);
//        StartCoroutine(LoadModelsTest(modelName, model));
//        return model;
//    }

//    public void MoveModel(Model model, Vector3 move)
//    {
//        StartCoroutine(MoveTest(move, model));
//    }

//    private bool boolTest = false;

//    private IEnumerator LoadModelsTest(string modelname, Model model)
//    {
//        boolTest = false;

//        yield return StartCoroutine(PerformSearch(modelname, model));

//        //while(boolTest == false)
//        //{
//        //    yield return null;
//       // }

//        //yield return StartCoroutine(MoveTest(shift));
//    }

//    private IEnumerator MoveTest(Vector3 move, Model model)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            Debug.Log("moving apple");
//            model.obj.transform.position += move;
//            yield return null;
//        }
        
//    }

//    private IEnumerator AuthenticateUser()
//    {
//        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
//        {
//            Debug.Log("Username And/Or Password are empty!");
//            yield return null;
//        }
//        else
//        {
//            string _Url = "https://sketchfab.com/oauth2/token/";


//            // string s = "";
//            WWWForm form = new WWWForm();
//            form.AddField("grant_type", "password");
//            form.AddField("username", username);
//            form.AddField("password", password);

//            using (UnityWebRequest www = UnityWebRequest.Post(_Url, form))
//            {
//                string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes (clientId + ":" + clientSecret));

//                www.SetRequestHeader("Authorization", "Basic " + cred);
//                yield return www.SendWebRequest();
//                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    // Error
//                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                    Debug.Log(www.error);
//                }
//                else
//                {
//                    if (www.isDone)
//                    {
//                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                        IAT =  (ImplicitAccessToken)JsonUtility.FromJson(jsonResult, typeof(ImplicitAccessToken));
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator PerformSearch()
//    {
//        fillbarObject.SetActive(false);
//        _nextUrl = string.Empty;
//        searchResults = new List<SFSearchResults>();
//        currentPageNumber = 0;
//        shouldLoadNextPage = true;

//        keywordToSearch = inputField.text;

//        if (keywordToSearch.Equals(""))
//        {
//            Debug.Log("Please enter a valid keyword!");
//            yield return null;
//        }
//        else
//        {
//            if (spawnedModel != null)
//            {
//                DestroyImmediate(spawnedModel);
//            }

//            //DeleteFiles();

//           // string _Url = baseUrl + "tags=" + keywordToSearch + "&downloadable=true&archives_flavours=true";
//            string _Url = "https://api.sketchfab.com/v3/search?type=models&q=" + keywordToSearch + "&animated=false&downloadable=true&archives_flavours=true";
//            _Url = Uri.EscapeUriString(_Url);
//            using (UnityWebRequest www = UnityWebRequest.Get(_Url))
//            {
//                www.SetRequestHeader("Content-Type", "application/json");
//                yield return www.SendWebRequest();
//                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    // Error
//                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                    Debug.Log(www.error);
//                }
//                else
//                {
//                    if (www.isDone)
//                    {
//                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                        SFSearchResults _searchResults =  (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
//                        searchResults.Add(_searchResults);
//                        currentPageNumber++;
//                        if (!string.IsNullOrEmpty(_searchResults.next))
//                        {
//                            _nextUrl = _searchResults.next;

//                            while (currentPageNumber < maximumPageLimit && shouldLoadNextPage)
//                            {
//                                shouldLoadNextPage = false;
//                                StartCoroutine(LoadNextPages(_nextUrl));

//                                while (!shouldLoadNextPage)
//                                    yield return null;
//                            }
//                        }

//                        // Get model download details
//                        int randomElementIndex = 0;//UnityEngine.Random.Range(0, maximumPageLimit);
//                        int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
//                        StartCoroutine(GetModelDetails(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, new Model(keywordToSearch)));
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator PerformSearch(string modelName, Model model)
//    {
//        Debug.Log("Performing search for " + modelName);

//        fillbarObject.SetActive(false);
//        _nextUrl = string.Empty;
//        List<SFSearchResults> searchResults = new List<SFSearchResults>();
//        currentPageNumber = 0;
//        shouldLoadNextPage = true;

//        //keywordToSearch = modelName;

//        if (modelName.Equals(""))
//        {
//            Debug.Log("Please enter a valid keyword!");
//            yield return null;
//        }
//        else
//        {
            

//            // string _Url = baseUrl + "tags=" + keywordToSearch + "&downloadable=true&archives_flavours=true";
//            string _Url = "https://api.sketchfab.com/v3/search?type=models&q=" + modelName + "&animated=false&downloadable=true&archives_flavours=true";
//            _Url = Uri.EscapeUriString(_Url);
//            using (UnityWebRequest www = UnityWebRequest.Get(_Url))
//            {
//                www.SetRequestHeader("Content-Type", "application/json");
//                yield return www.SendWebRequest();
//                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    // Error
//                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                    Debug.Log(www.error);
//                }
//                else
//                {
//                    if (www.isDone)
//                    {
//                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                        SFSearchResults _searchResults = (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
//                        searchResults.Add(_searchResults);
//                        currentPageNumber++;
//                        if (!string.IsNullOrEmpty(_searchResults.next))
//                        {
//                            _nextUrl = _searchResults.next;

//                            while (currentPageNumber < maximumPageLimit && shouldLoadNextPage)
//                            {
//                                shouldLoadNextPage = false;
//                                StartCoroutine(LoadNextPages(_nextUrl));

//                                while (!shouldLoadNextPage)
//                                    yield return null;
//                            }
//                        }

//                        // Get model download details
//                        int randomElementIndex = UnityEngine.Random.Range(0, maximumPageLimit);
//                        int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
//                        StartCoroutine(GetModelDetails(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator LoadNextPages(string url)
//    {
//        Debug.Log("Loading next pages");
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            www.SetRequestHeader("Content-Type", "application/json");
//            yield return www.SendWebRequest();
//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {
//                // Error
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log(www.error);
//            }
//            else
//            {
//                if (www.isDone)
//                {
//                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                    SFSearchResults _searchResults =  (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
//                    searchResults.Add(_searchResults);
//                    _nextUrl = _searchResults.next;
//                    currentPageNumber++;
//                    shouldLoadNextPage = true;
//                }
//            }
//        }
//    }

//    private IEnumerator GetModelDetails(string UID, Model model)
//    {
//        Debug.Log("Getting more details");
//       // UID = "cf4194d4ce404273a62c7711447f5f51";
//        if (string.IsNullOrEmpty(IAT.access_token))
//        {
//            StartCoroutine(AuthenticateUser());
//            yield return new WaitForSeconds(2f);
//            StartCoroutine(GetSpecificModelDetails(UID, model));
//        }
//        else
//        {
//            StartCoroutine(GetSpecificModelDetails(UID, model));
//            yield return null;
//        }
//    }

//    private IEnumerator GetSpecificModelDetails(string UID, Model model)
//    {
//        string downloadUrl = "https://api.sketchfab.com/v3/models/" + UID + "/download";
//        Debug.Log("________----------_____ The Download URL is "+ downloadUrl);
//        using (UnityWebRequest www = UnityWebRequest.Get(downloadUrl))
//        {
//            www.SetRequestHeader("Authorization", "Bearer " + IAT.access_token);
//            www.SetRequestHeader("Content-Type", "application/json");
//            yield return www.SendWebRequest();
//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {
//                // Error
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log("Error fetching details: " + www.error);
//            }
//            else
//            {
//                if (www.isDone)
//                {
//                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                    _ModelDownloadMetaData modelMetaData = (_ModelDownloadMetaData)JsonUtility.FromJson(jsonResult, typeof(_ModelDownloadMetaData));
//                    StartCoroutine(BeginDownloadProcess(modelMetaData.gltf.url, model));
//                }
//            }
//        }
//    }

//    private IEnumerator BeginDownloadProcess(string url, Model model)
//    {
//        Debug.Log("Downloading from: " + url);
//        fillbarObject.SetActive(true);
//        fillbarForeground.fillAmount = 0;
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            var operation = www.SendWebRequest();

//            while (!operation.isDone)
//            {
//                float progress = www.downloadProgress * 100 / 100.0f;
//                fillbarForeground.fillAmount = progress;
//                yield return null;
//            }

//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {
//                fillbarObject.SetActive(false);
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log("Download err: " + www.error);
//            }
//            else
//            {
//                if (operation.isDone)
//                {
//                    fillbarObject.SetActive(false);
//                    string modelName = model.obj.name + ".zip";
//                    string write_path = Application.persistentDataPath + "/" + modelName;
//                    Debug.Log("Path: " + write_path);
//                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
//                    DisplayModel(write_path, model);
//                }
//            }
//        }
//    }

    
//    public float size;

    

//    private void DisplayModel(string path, Model model)
//    {
//        Debug.Log("Displaying model");
//        var context = gltfImporter.Load(path);
//        context.ShowMeshes();
//        Destroy(model.obj);
//        model.obj = context.Root;
//        model.obj.transform.position = Vector3.zero;
//        model.obj.transform.localEulerAngles = new Vector3(0, 0, 0);
//        Debug.Log("Spawned model name is " + model.obj.name);

//        MeshRenderer collider = model.obj.GetComponentInChildren<MeshRenderer>();
//        SkinnedMeshRenderer _smr = model.obj.GetComponentInChildren<SkinnedMeshRenderer>();

//        Vector3 targetSize = Vector3.one * size;

//        Bounds meshBounds = GetRenderBounds(model.obj);


//        Vector3 meshSize = meshBounds.size;
//        Debug.Log("before rescaling the mesh size is " + meshSize);
//        float xScale = targetSize.x / meshSize.x;
//        float yScale = targetSize.y / meshSize.y;
//        float zScale = targetSize.z / meshSize.z;
//        float scaleFactor = Mathf.Min(xScale, yScale, zScale);
//        model.obj.transform.localScale *= scaleFactor;


//        //Then repoisition to center it, with bottom of the bounding box at (0,0,0)
//        model.obj.transform.position -= scaleFactor * meshBounds.center;
//        model.obj.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, scaleFactor, 0));

//        model.readyToMove = true;
//        boolTest = true;

//        //yield return null;
//    }

//    private Bounds GetRenderBounds(GameObject obj)
//    {
//        var r = obj.GetComponent<Renderer>();

//        var renderers = obj.GetComponentsInChildren<Renderer>();
//        if (r == null)
//            r = renderers[0];
//        var bounds = r.bounds;
//        foreach (Renderer render in renderers)
//        {
//            if (render != r)
//                bounds.Encapsulate(render.bounds);
//        }
//        return bounds;
//    }

//    public void DeleteFiles()
//    {
//        searchResults = new List<SFSearchResults>();
//        IAT = null;
//        string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
//        foreach (string filePath in filePaths)
//            File.Delete(filePath); 
//    }

//    public void _PerformSearch()
//    {
//        StartCoroutine(PerformSearch());
//    }

//    public void _AuthenticateUser()
//    {
//        StartCoroutine(AuthenticateUser());
//    }
//}
