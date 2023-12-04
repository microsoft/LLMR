//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//using UnityEngine.Networking;
//using UniGLTF;
//using System.IO;
//using System.Text;
//using Unity.VisualScripting;

//public class LegacySketchfabLoader : MonoBehaviour
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
//            readyToMove = true;
//        }
//    }

//    //private GameObject spawnedModel;
//    public string username = "abanburski@microsoft.com";
//    public string password = "kalamazoo1!";
//    public string baseUrl = "https://api.sketchfab.com/v3/models?";
//    //public string keywordToSearch;
//    public int maximumPageLimit = 1;
//    public List<SFSearchResults> searchResults;

//    //Default size of loaded objects
//    public float size = 1;

//    private string _nextUrl;
//    private int currentPageNumber;
//    private bool shouldLoadNextPage;
//    private string clientId = "sDvwSIxKARJ6m6PZSLLYhmT60ha1rzDYO6J6RUnx";
//    private string clientSecret = "7wbty2FT0fx7S90h9QeGMbzsGvyIHhoQPwd2I0qJYYYVirB4X82ORQk9l4pFOH0783tg9SHiOSOGtVKCIMB9vVcjnu01HFtc15XnJoXcEViF5tZJO87Ub5TMuNbcIeHw";
//    [SerializeField] private ImplicitAccessToken IAT;

//    private bool oneAtATime = false;
//    private void Start()
//    {
        
//        //Model apple = LoadModel("apple");
//        //MoveModel(apple, new Vector3(0, 2, 0));
//        //Model banana = LoadModel("banana");
//        //MoveModel(banana, new Vector3(0, -1, 0));
//        //PlaceModelNextTo(apple, banana, new Vector3(0, 0.2f, 0));
//    }

//    public Model LoadModel(string modelName)
//    {
//        Model model = new Model(modelName);
//        StartCoroutine(_LoadModel(modelName, model));
//        return model;
//    }
//    public GameObject Load(string modelName)
//    {
//        GameObject model = new GameObject(modelName);
//        StartCoroutine(_LoadModelNew(modelName, model));
//        return model;
//    }

//    public void AttachModelToGameObject(Model model, GameObject newParent)
//    {
//        StartCoroutine(AttachTest(model, newParent));
//    }

//    public void MoveModel(Model model, Vector3 move)
//    {
//        StartCoroutine(MoveTest(move, model));
//    }

//    public void Move(GameObject model, Vector3 move)
//    {
//        StartCoroutine(MoveTest(move, model));
//    }

//    public void ScaleModel(Model model, float scale)
//    {
//        StartCoroutine(RescaleTest(scale, model));
//    }

//    public void Scale(GameObject model, float scale)
//    {
//        StartCoroutine(RescaleTest(scale, model));
//    }

//    public void AddPhysics(Model model, float mass = 1)
//    {
//        StartCoroutine(PhysicsTest(model, mass));
//    }

//    public void AddPhysics(GameObject model, float mass = 1)
//    {
//        StartCoroutine(PhysicsTest(model, mass));
//    }

//    public void PlaceModelNextTo(Model originalModel, Model modelToPlace, Vector3 move)
//    {
//        StartCoroutine(MoveTest2(move, originalModel, modelToPlace));
//    }

//    public void ReplaceModel(Model oldModel, string newModelName)
//    {
//        Model newModel = LoadModel(newModelName);
//        StartCoroutine(ReplaceTest(newModel, oldModel));
//    }
//    public void ReplaceModel(GameObject oldModel, string newModelName)
//    {
//        Model newModel = LoadModel(newModelName);
//        StartCoroutine(ReplaceTest(newModel, new Model(oldModel)));
//    }

//    private IEnumerator _LoadModel(string modelname, Model model)
//    {
//        while (oneAtATime)
//        {
//            yield return null;
//        }
//        if (oneAtATime == false)
//        {
//            yield return StartCoroutine(PerformSearch(modelname, model));
//        }
        
//    }

//    private IEnumerator _LoadModelNew(string modelname, GameObject model)
//    {
//        while (oneAtATime)
//        {
//            yield return null;
//        }
//        if (oneAtATime == false)
//        {
//            yield return StartCoroutine(PerformSearchNew(modelname, model));
//        }

//    }

//    private IEnumerator AttachTest(Model model, GameObject newParent)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            model.obj.transform.parent.gameObject.transform.parent = newParent.transform;
//            model.obj.transform.parent.gameObject.transform.localPosition = new Vector3(0, 0, 0);
//            yield return null;
//        }

//    }

//    private IEnumerator ReplaceTest(Model model, Model oldObject)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            model.obj.transform.position = oldObject.obj.transform.position;
//            model.obj.transform.rotation = oldObject.obj.transform.rotation;
//            Destroy(oldObject.obj);
//            yield return null;
//        }

//    }

//    private IEnumerator MoveTest(Vector3 move, Model model)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            model.obj.transform.parent.gameObject.transform.position = move;
//            yield return null;
//        }

//    }

//    private IEnumerator MoveTest(Vector3 move, GameObject model)
//    {
//        Debug.Log(ReadyToMove(model));
//        while (ReadyToMove(model) == false)
//        {
//            yield return null;
//        }

//        if (ReadyToMove(model))
//        {
//            Debug.Log("model name is: " + model.name);
//            Debug.Log("model parent name is: "+ model.transform.parent.gameObject.name);
//            model.transform.parent.gameObject.transform.position = move;
//            yield return null;
//        }

//    }

//    private IEnumerator PhysicsTest(Model model, float mass)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            //First add colliders to all children
//            foreach(Transform child in model.obj.transform.GetComponentsInChildren<Transform>())
//            {
//                MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
//                col.convex = true;
//            }

//            Rigidbody rb = model.obj.transform.parent.gameObject.AddComponent<Rigidbody>();
//            rb.mass = mass;
//            yield return null;
//        }

//    }

//    private IEnumerator PhysicsTest(GameObject model, float mass)
//    {
//        while (ReadyToMove(model) == false)
//        {
//            yield return null;
//        }

//        if (ReadyToMove(model))
//        {
//            //First add colliders to all children
//            foreach (Transform child in model.transform.GetComponentsInChildren<Transform>())
//            {
//                MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
//                col.convex = true;
//            }

//            Rigidbody rb = model.transform.parent.gameObject.AddComponent<Rigidbody>();
//            rb.mass = mass;
//            yield return null;
//        }

//    }


//    private IEnumerator RescaleTest(float scale, Model model)
//    {
//        while (model.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model.readyToMove)
//        {
//            model.obj.transform.parent.gameObject.transform.localScale *= scale;
//            //Need to reposition it
//            yield return null;
//        }

//    }

//    private IEnumerator RescaleTest(float scale, GameObject model)
//    {
//        while (ReadyToMove(model) == false)
//        {
//            yield return null;
//        }

//        if (ReadyToMove(model))
//        {
//            model.transform.parent.gameObject.transform.localScale *= scale;
//            //Need to reposition it
//            yield return null;
//        }

//    }

//    private IEnumerator MoveTest2(Vector3 move, Model model1, Model Model2)
//    {
//        while (model1.readyToMove == false || Model2.readyToMove == false)
//        {
//            yield return null;
//        }

//        if (model1.readyToMove && Model2.readyToMove)
//        {
//            Debug.Log("moving apple");
//            PlaceNextTo(model1.obj.transform.parent.gameObject,Model2.obj.transform.parent.gameObject, move);
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
//                string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret));

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
//                        IAT = (ImplicitAccessToken)JsonUtility.FromJson(jsonResult, typeof(ImplicitAccessToken));
//                    }
//                }
//            }
//        }
//    }
       
//    private IEnumerator PerformSearch(string modelName, Model model)
//    {
//        Debug.Log("Performing search for " + modelName);
//        oneAtATime = true;

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
//                        int randomElementIndex = 0;// UnityEngine.Random.Range(0, maximumPageLimit);
//                        int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
//                        StartCoroutine(GetModelDetails(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator PerformSearchNew(string modelName, GameObject model)
//    {
//        Debug.Log("Performing search for " + modelName);
//        oneAtATime = true;

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
//                        int randomElementIndex = 0;// UnityEngine.Random.Range(0, maximumPageLimit);
//                        int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
//                        StartCoroutine(GetModelDetailsNew(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
//                    }
//                }
//            }
//        }
//    }

//    private IEnumerator LoadNextPages(string url)
//    {
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
//                    SFSearchResults _searchResults = (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
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
//        // UID = "cf4194d4ce404273a62c7711447f5f51";
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

//    private IEnumerator GetModelDetailsNew(string UID, GameObject model)
//    {
//        // UID = "cf4194d4ce404273a62c7711447f5f51";
//        if (string.IsNullOrEmpty(IAT.access_token))
//        {
//            StartCoroutine(AuthenticateUser());
//            yield return new WaitForSeconds(2f);
//            StartCoroutine(GetSpecificModelDetailsNew(UID, model));
//        }
//        else
//        {
//            StartCoroutine(GetSpecificModelDetailsNew(UID, model));
//            yield return null;
//        }
//    }

//    private IEnumerator GetSpecificModelDetails(string UID, Model model)
//    {
//        string downloadUrl = "https://api.sketchfab.com/v3/models/" + UID + "/download";
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

//    private IEnumerator GetSpecificModelDetailsNew(string UID, GameObject model)
//    {
//        string downloadUrl = "https://api.sketchfab.com/v3/models/" + UID + "/download";
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
//                    StartCoroutine(BeginDownloadProcessNew(modelMetaData.gltf.url, model));
//                }
//            }
//        }
//    }

//    private IEnumerator BeginDownloadProcess(string url, Model model)
//    {
//        Debug.Log("Downloading from: " + url);
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            var operation = www.SendWebRequest();

//            while (!operation.isDone)
//            {                
//                yield return null;
//            }

//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {                
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log("Download err: " + www.error);
//            }
//            else
//            {
//                if (operation.isDone)
//                {                    
//                    string modelName = model.obj.name + ".zip";
//                    string write_path = Application.persistentDataPath + "/" + modelName;
//                    Debug.Log("Path: " + write_path);
//                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
//                    DisplayModel(write_path, model);
//                }
//            }
//        }
//    }

//    private IEnumerator BeginDownloadProcessNew(string url, GameObject model)
//    {
//        Debug.Log("Downloading from: " + url);
//        using (UnityWebRequest www = UnityWebRequest.Get(url))
//        {
//            var operation = www.SendWebRequest();

//            while (!operation.isDone)
//            {
//                yield return null;
//            }

//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log("Download err: " + www.error);
//            }
//            else
//            {
//                if (operation.isDone)
//                {
//                    string modelName = model.name + ".zip";
//                    string write_path = Application.persistentDataPath + "/" + modelName;
//                    Debug.Log("Path: " + write_path);
//                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
//                    DisplayModelNew(write_path, model);
//                }
//            }
//        }
//    }

//    private void DisplayModel(string path, Model model)
//    {
//        try
//        {
//            var context = gltfImporter.Load(path);
//            context.ShowMeshes();
//            Destroy(model.obj);
//            model.obj = context.Root;
//        }
//        catch (Exception e)
//        {
//            Debug.Log(e.Message);
//            StartCoroutine(PerformSearch(model.obj.name, model));// Try again?
//            return; // no need to continue here
//            //oneAtATime = false; // Allows next model to be loaded, skip this one
//        }

//        GameObject objParent = new GameObject(model.obj.name);
//        objParent.tag = "Spawned";
//        model.obj.transform.parent = objParent.transform;
//        model.obj.name = model.obj.name + "_rescaled";
//        model.obj.transform.position = Vector3.zero;
//        model.obj.transform.localEulerAngles = new Vector3(0, 0, 0);
//        model.obj.tag = "Spawned";

//        MeshRenderer collider = model.obj.GetComponentInChildren<MeshRenderer>();
//        SkinnedMeshRenderer _smr = model.obj.GetComponentInChildren<SkinnedMeshRenderer>();

//        Vector3 targetSize = Vector3.one * size;

//        Bounds meshBounds = GetRenderBounds(model.obj);


//        Vector3 meshSize = meshBounds.size;
//        //Debug.Log("before rescaling the mesh size is " + meshSize);
//        float xScale = targetSize.x / meshSize.x;
//        float yScale = targetSize.y / meshSize.y;
//        float zScale = targetSize.z / meshSize.z;
//        float scaleFactor = Mathf.Min(xScale, yScale, zScale);
//        model.obj.transform.localScale *= scaleFactor;


//        //Then repoisition to center it, with bottom of the bounding box at (0,0,0)
//        model.obj.transform.position -= scaleFactor * meshBounds.center;
//        model.obj.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, scaleFactor, 0));


//        // Try a rotation towards the camera?
//        //model.obj.transform.LookAt(GameObject.Find("Holodeck").transform);

//        model.readyToMove = true;
//        oneAtATime = false; // Allow next model to be searched for
//    }

//    private void DisplayModelNew(string path, GameObject model)
//    {
//        try
//        {
//            var context = gltfImporter.Load(path);
//            context.ShowMeshes();
//            //Destroy(model);
//            context.Root.transform.parent = model.transform;
//        }
//        catch (Exception e)
//        {
//            Debug.Log(e.Message);
//            StartCoroutine(PerformSearchNew(model.name, model));// Try again?
//            return; // no need to continue here
//            //oneAtATime = false; // Allows next model to be loaded, skip this one
//        }

//        GameObject objParent = new GameObject(model.name);
//        objParent.tag = "Spawned";
//        model.transform.parent = objParent.transform;
//        model.name = model.name + "_rescaled";
//        model.transform.position = Vector3.zero;
//        model.transform.localEulerAngles = new Vector3(0, 0, 0);
//        model.tag = "Spawned";

//        MeshRenderer collider = model.GetComponentInChildren<MeshRenderer>();
//        SkinnedMeshRenderer _smr = model.GetComponentInChildren<SkinnedMeshRenderer>();

//        Vector3 targetSize = Vector3.one * size;

//        Bounds meshBounds = GetRenderBounds(model);


//        Vector3 meshSize = meshBounds.size;
//        //Debug.Log("before rescaling the mesh size is " + meshSize);
//        float xScale = targetSize.x / meshSize.x;
//        float yScale = targetSize.y / meshSize.y;
//        float zScale = targetSize.z / meshSize.z;
//        float scaleFactor = Mathf.Min(xScale, yScale, zScale);
//        model.transform.localScale *= scaleFactor;


//        //Then repoisition to center it, with bottom of the bounding box at (0,0,0)
//        model.transform.position -= scaleFactor * meshBounds.center;
//        model.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, scaleFactor, 0));


//        // Try a rotation towards the camera?
//        //model.obj.transform.LookAt(GameObject.Find("Holodeck").transform);

//        //model.readyToMove = true;
//        oneAtATime = false; // Allow next model to be searched for
//    }

//    public bool ReadyToMove(GameObject obj)
//    {
//        if (obj.tag == "Spawned")
//            return true;
//        else return false;
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

//    // Places objectToPlace on or next to originalObject, in the direction of the placement
//    // vector, making sure that the objects don't overlap. If placement would be within the mesh bounds,
//    // then the object is pushed out to the closest point on the surface 
//    public void PlaceNextTo(GameObject originalObj, GameObject objToPlace, Vector3 placement)
//    {

//        // Play with rotation here
//        objToPlace.transform.LookAt(originalObj.transform);

//        Bounds originalObjBounds = GetRenderBounds(originalObj);
//        Bounds objToPlaceBounds = GetRenderBounds(objToPlace);
//        Vector3 objToPlaceCenterOffset = objToPlaceBounds.center - objToPlace.transform.position;
//        Debug.Log(originalObj.name + " bounds center is " + originalObjBounds.center);
//        Debug.Log(originalObj.name + " bounds size is " + originalObjBounds.size);
//        Debug.Log(objToPlace.name + " bounds center is " + objToPlaceBounds.center);
//        Debug.Log(objToPlace.name + " bounds size is " + objToPlaceBounds.size);


//        objToPlace.transform.position = originalObj.transform.position + placement;
//        //Debug.Log(objToPlace.name + " position is " + objToPlace.transform.position);
//        objToPlaceBounds = GetRenderBounds(objToPlace);
//        if (originalObjBounds.Intersects(objToPlaceBounds))
//        {
//            //First reset
//            objToPlace.transform.position = originalObj.transform.position;
//            objToPlaceBounds = GetRenderBounds(objToPlace);
//            //Shift by the bounds centers
//            Vector3 relDisBtwCenters = originalObjBounds.center - objToPlaceBounds.center;
//            Debug.Log("Distance between centers is " + relDisBtwCenters);
//            objToPlace.transform.position += relDisBtwCenters;
//            objToPlaceBounds = GetRenderBounds(objToPlace);
//            //Now centers of bounds are at the  same point

//            // Construct ray for intersections
//            Ray ray = new Ray(originalObjBounds.center, placement);
//            //Debug.Log("The ray direction is " + ray.direction);
//            //Debug.Log("Going 1 unit along the ray from the bound center gets us to " + ray.GetPoint(1));

//            //define distances of intersection of the ray for both of the boxes
//            float originalObjDistance;
//            float objToPlaceDistance;

//            originalObjBounds.IntersectRay(ray, out originalObjDistance);
//            objToPlaceBounds.IntersectRay(ray, out objToPlaceDistance);
//            Debug.Log("first distance is " + originalObjDistance);
//            Debug.Log("second distance is " + objToPlaceDistance);

//            Debug.Log("Ray points to " + ray.GetPoint(-originalObjDistance - objToPlaceDistance));
//            Debug.Log("relative distance between centers is " + relDisBtwCenters);

//            objToPlace.transform.position += ray.GetPoint(-originalObjDistance - objToPlaceDistance) - originalObjBounds.center;//  - objToPlaceCenterOffset; // - relDisBtwCenters
            
//            // Place on floor if the first object is on it and there was no upward component to placement
//            if (placement.y == 0 && Mathf.Abs(originalObj.transform.position.y) <= 0.001f)
//            {
//                //Debug.Log("Resetting to floor");
//                objToPlace.transform.position = Vector3.Scale(objToPlace.transform.position, new Vector3(1, 0, 1));
//            }
//        }

//    }

//    public void DestroyLoadedObjects()
//    {
//        GameObject[] objs = GameObject.FindGameObjectsWithTag("Spawned");

//        foreach (GameObject obj in objs)
//        {
//            Destroy(obj);
//        }
//    }
               

//    public void _AuthenticateUser()
//    {
//        StartCoroutine(AuthenticateUser());
//    }
//}


