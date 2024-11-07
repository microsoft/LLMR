using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
//using UniGLTF;

using Siccity.GLTFUtility;

//using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using static SketchfabLoader;

public class SketchfabLoader : MonoBehaviour
{
    #region HelperClasses

    [System.Serializable]
    public class Archives
    {
        public List<Glb> glb;
        public List<Gltf> gltf;
        public List<Source> source;
        public List<Usdz> usdz;
    }

    [System.Serializable]
    public class Avatar
    {
        public string uri;
        public List<Image> images;
    }

    [System.Serializable]
    public class Category
    {
        public string name;
    }

    [System.Serializable]
    public class Cursors
    {
        public string next;
        public object previous;
    }

    [System.Serializable]
    public class Glb
    {
        public int textureCount;
        public int size;
        public string type;
        public int textureMaxResolution;
        public int faceCount;
        public int vertexCount;
    }

    [System.Serializable]
    public class Gltf
    {
        public int textureCount;
        public int size;
        public string type;
        public int textureMaxResolution;
        public int faceCount;
        public int vertexCount;
    }

    [System.Serializable]
    public class Image
    {
        public string uid;
        public int size;
        public int width;
        public string url;
        public int height;
    }

    [System.Serializable]
    public class License
    {
        public string uid;
        public string label;
    }

    [System.Serializable]
    public class Result
    {
        public string uri;
        public string uid;
        public string name;
        public object staffpickedAt;
        public int viewCount;
        public int likeCount;
        public int animationCount;
        public string viewerUrl;
        public string embedUrl;
        public int commentCount;
        public bool isDownloadable;
        public DateTime publishedAt;
        public List<Tag> tags;
        public List<Category> categories;
        public Thumbnails thumbnails;
        public User user;
        public string description;
        public int faceCount;
        public DateTime createdAt;
        public int vertexCount;
        public bool isAgeRestricted;
        public int soundCount;
        public bool isProtected;
        public License license;
        public object price;
        public Archives archives;
    }

    [System.Serializable]
    public class SFSearchResults
    {
        public Cursors cursors;
        public string next;
        public object previous;
        public List<Result> results;
    }

    [System.Serializable]
    public class Source
    {
        public object textureCount;
        public int size;
        public string type;
        public object textureMaxResolution;
        public object faceCount;
        public object vertexCount;
    }

    [System.Serializable]
    public class Tag
    {
        public string name;
        public string slug;
        public string uri;
    }

    [System.Serializable]
    public class Thumbnails
    {
        public List<Image> images;
    }

    [System.Serializable]
    public class Usdz
    {
        public object textureCount;
        public int size;
        public string type;
        public object textureMaxResolution;
        public object faceCount;
        public object vertexCount;
    }

    [System.Serializable]
    public class User
    {
        public string uid;
        public string username;
        public string displayName;
        public string profileUrl;
        public string account;
        public Avatar avatar;
        public string uri;
    }

    [System.Serializable]
    public class ImplicitAccessToken
    {
        public string access_token;
        public int expires_in;
        public string token_type;
        public string scope;
        public string refresh_token;
    }

    [System.Serializable]
    public class _Glb
    {
        public string url;
        public int size;
        public int expires;
    }

    [System.Serializable]
    public class _Gltf
    {
        public string url;
        public int size;
        public int expires;
    }

    [System.Serializable]
    public class _Usdz
    {
        public string url;
        public int size;
        public int expires;
    }

    [System.Serializable]
    public class _ModelDownloadMetaData
    {
        public _Gltf gltf;
        public _Usdz usdz;
        public _Glb glb;
    }

    #endregion

    public bool use_DALLE_CLIP_refinement;
    public bool save_models_to_nontemp_folder;
    public Find3DModelsSingle finder;
    public string username = "...";
    public string password = "...";
    public string baseUrl = "https://api.sketchfab.com/v3/models?";
    public int maximumPageLimit = 1;
    public List<SFSearchResults> searchResults;

    //Default size of loaded objects
    public float size = 1;

    private string _nextUrl;
    private int currentPageNumber;
    private bool shouldLoadNextPage;
    private string clientId = "...";
    private string clientSecret = "...";
    [SerializeField] private ImplicitAccessToken IAT;

    
    private bool oneAtATime = false;
    private int curr_cache_idx;
    private int batch_num_jobs_finished = 0;
    private Dictionary<int, bool> cache_hit = new Dictionary<int, bool>();
    public GameObject currentModel;
    // flag for whether some operation is going on and needs to be waited on. 
    // Basically a temporary solution for await
    //public bool is_processing; 
    public bool createParentCollider = true;

    private void Start()
    {

    }


    private List<ModelData> RemoveCachedModelsInLoad(List<ModelData> models)
    {
        List<ModelData> models_to_download = new List<ModelData>();
        
        for (int i = 0; i < models.Count; i++)
        {
            // if model not found locally, add it to the download list.
            if (!cache_hit.ContainsKey(i))
            {
                models_to_download.Add(models[i]);
            }
        }

        return models_to_download;
    }


    public void LoadBatchWithCaching(List<ModelData> models)
    {
        StartCoroutine(LoadBatchWithCachingCoroutine(models));
    }

    public void _LoadBatchWithCaching(List<ModelData> models)
    {
        // try to see if the models already exist locally. If so, load them from disc.
        //bool load_success = false;
        int n_models = models.Count;
        bool[] cached = new bool[n_models];
        for (int i = 0; i < n_models; i++)
        {
            StartCoroutine(LoadLocalCoroutine(models[i].label, i)); //wait for these to finish
            //while (oneAtATime)
            //{
            //    print("spin locked");
            //    yield return null;
            //}
            //if (!oneAtATime)
            //{
            //    oneAtATime = true;
            //    yield return StartCoroutine(LoadLocal(models[i].label, i)); //wait for these to finish
            //}
        }

        Debug.Log("Checking for cached models finished. Start downloading the remaining models.");
        models = RemoveCachedModelsInLoad(models);

        // download the non-cached models from sketchfab
        if (models.Count != 0)
        {
            LoadBatch(models);
        }
    }

    public IEnumerator LoadBatchWithCachingCoroutine(List<ModelData> models) 
    {
        // try to see if the models already exist locally. If so, load them from disc.
        int n_models = models.Count;
        bool[] cached = new bool[n_models];
        batch_num_jobs_finished = 0;
        for (int i = 0; i < n_models; i++)
        {
            while (oneAtATime)
            {
                yield return null;
            }
            if (!oneAtATime)
            {
                oneAtATime = true;
                //yield return StartCoroutine(LoadLocalCoroutine(models[i].label, i)); //wait for these to finish
                LoadLocal(models[i].label, i);
            }
        }

        //wait for all coroutines to finish
        while (batch_num_jobs_finished < n_models) 
        {
            yield return null;
        }

        Debug.Log("Checking for cached models finished. Start downloading the remaining models if any exists.");
        // get the non-cached models
        models = RemoveCachedModelsInLoad(models);

        // download the non-cached models from sketchfab
        if (models.Count != 0)
        {
            LoadBatch(models);
        }
    }


    public void LoadBatch(List<ModelData> models)
    {
        if (use_DALLE_CLIP_refinement)
        {
            finder.models = models;
            finder.Find3DModelsClosestToLabels(finder.models, (uid, index) =>
            {
                // do something with the uid and the index, for example, update the model data
                finder.models[index].uid = uid;
                //Debug.Log("UID for label: " + uid);
            });
        }
        else
        {
            // load one by one with no DALLE-CLIP refinement
            foreach (ModelData d in models)
            {
                GameObject obj = Load(d.label);
                Scale(obj, d.scale);
                Move(obj, d.position);
            }
            
        }
    }


    //public GameObject LoadWithCaching(string modelName)
    //{
    //    bool load_success = false;
    //    GameObject model = LoadLocal(modelName, out load_success);
    //    // if local files not found, download it from sketchfab
    //    if (!load_success)
    //    {
    //        StartCoroutine(_LoadModelNew(modelName, model));
    //    }
        
    //    return model;
    //}

    void OnFinishAsyncLocal(GameObject result, AnimationClip[] animations)
    {
        Debug.Log("Loaded " + result.name + " from cache.");
        result.transform.parent = currentModel.transform;
        CenterCurrentModel();
        cache_hit[curr_cache_idx] = true;
        batch_num_jobs_finished++;
    }


    public void LoadLocal(string modelName, int i)
    {
        // oneAtATime means some other model is currently being loaded.
        // it serves as a spinlock
        oneAtATime = true;

        // currentModel will be the parent of the loaded model
        currentModel = new GameObject(modelName);
        curr_cache_idx = i;
        //string modelLocation = Application.dataPath + "/SketchfabModels/" + modelName;
        string modelLocation = "";
        if (save_models_to_nontemp_folder)
        {
            //modelLocation = "C:/Unity/SketchfabModels/" + modelName;
            modelLocation = Application.persistentDataPath + "/" + modelName + "/scene.gltf";
            Debug.Log("Cache checking at " + modelLocation);
        }
        else
        {
            modelLocation = Application.temporaryCachePath + "/" + modelName + "/scene.gltf";
        }

        try
        {
            //ImportGLTFAsync(modelLocation);
            Importer.ImportGLTFAsync(modelLocation, new ImportSettings(), OnFinishAsyncLocal);

        }
        catch (Exception e)
        {
            Debug.Log("Cached model not found: " + modelName);
            // release spin lock
            oneAtATime = false;
            batch_num_jobs_finished++;
            // if we don't destroy here, there will be an empty GO after the downloading is finished.
            DestroyImmediate(currentModel);
        }

    }

    public IEnumerator LoadLocalCoroutine(string modelName, int i)
    {
        // oneAtATime means some other model is currently being loaded.
        // it serves as a spinlock
        while (oneAtATime)
        {
            yield return null;
        }
        if (!oneAtATime)
        {
            oneAtATime = true;

            // currentModel will be the parent of the loaded model
            currentModel = new GameObject(modelName);
            curr_cache_idx = i;
            //string modelLocation = Application.dataPath + "/SketchfabModels/" + modelName;
            string modelLocation = "";
            if (save_models_to_nontemp_folder)
            {
                //modelLocation = "C:/Unity/SketchfabModels/" + modelName;
                modelLocation = Application.persistentDataPath + "/" + modelName + "/scene.gltf";
                Debug.Log("Cache checking at " + modelLocation);
            }
            else
            {
                modelLocation = Application.temporaryCachePath + "/" + modelName + "/scene.gltf";
            }

            try
            {
                //ImportGLTFAsync(modelLocation);
                Importer.ImportGLTFAsync(modelLocation, new ImportSettings(), OnFinishAsyncLocal);

            }
            catch (Exception e)
            {
                Debug.Log("Cached model not found: " + modelName);
            }
        }

    }

    public GameObject Load(string modelName)
    {
        GameObject model = new GameObject(modelName);        
        StartCoroutine(_LoadModelNew(modelName, model));
        return model;
    }

    private IEnumerator _LoadModelByUID(string UID, GameObject model)
    {
        while (oneAtATime)
        {
            yield return null;
        }
        if (oneAtATime == false)
        {
            currentModel = model;
            yield return StartCoroutine(GetModelDetailsNew(UID, model));
        }

    }

    // load model from sketchfab by their id
    public GameObject LoadByID(string UID, string modelName)
    {
        GameObject model = new GameObject(modelName);
        StartCoroutine(_LoadModelByUID(UID, model));
        return model;
    }


    public void Move(GameObject model, Vector3 move)
    {
        StartCoroutine(MoveTest(move, model));
    }


    public void Scale(GameObject model, float scale)
    {
        StartCoroutine(RescaleTest(scale, model));
    }


    public void AddPhysics(GameObject model, float mass = 1)
    {
        StartCoroutine(PhysicsTest(model, mass));
    }

    public void MoveNextTo(GameObject model1, GameObject model2, Vector3 placement)
    {
        StartCoroutine(MoveTest2(placement, model1, model2));
    }

    public GameObject Instantiate(GameObject model, Vector3 position, Quaternion rotation)
    {
        GameObject instance = new GameObject(model.name);
        StartCoroutine(InstantiateTest(model, instance, position, rotation)) ; 
        return instance;
    }


    private IEnumerator _LoadModelNew(string modelname, GameObject model)
    {
        while (oneAtATime)
        {
            yield return null;
        }
        if (oneAtATime == false)
        {
            currentModel = model;
            yield return StartCoroutine(PerformSearchNew(modelname, model));
        }

    }


    private IEnumerator MoveTest(Vector3 move, GameObject model)
    {
        Debug.Log(ReadyToMove(model));
        while (ReadyToMove(model) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model))
        {
            Debug.Log("model name is: " + model.name);
            Debug.Log("model parent name is: "+ model.transform.parent.gameObject.name);
            model.transform.parent.gameObject.transform.position = move;
            yield return null;
        }

    }


    private IEnumerator PhysicsTest(GameObject model, float mass)
    {
        while (ReadyToMove(model) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model))
        {
            //First add colliders to all children
            foreach (Transform child in model.transform.GetComponentsInChildren<Transform>())
            {
                MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
                col.convex = true;
            }

            Rigidbody rb = model.transform.parent.gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            yield return null;
        }

    }

    private IEnumerator MoveTest2(Vector3 move, GameObject model1, GameObject model2)
    {
        while (ReadyToMove(model1) == false || ReadyToMove(model2) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model1) && ReadyToMove(model2))
        {
            PlaceNextTo(model1.transform.parent.gameObject, model2.transform.parent.gameObject, move);
            yield return null;
        }

    }

    public IEnumerator InstantiateTest(GameObject model, GameObject instance, Vector3 position, Quaternion rotation)
    {
        instance.name = model.name + "instance";
        while (ReadyToMove(model) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model))
        {
            instance = GameObject.Instantiate(model, position, rotation);
            //Need to reposition it
            yield return null;
        }

    }

    private IEnumerator RescaleTest(float scale, GameObject model)
    {
        while (ReadyToMove(model) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model))
        {
            model.transform.parent.gameObject.transform.localScale *= scale;
            //Need to reposition it
            yield return null;
        }
    }

    private IEnumerator AuthenticateUser()
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Username And/Or Password are empty!");
            yield return null;
        }
        else
        {
            string _Url = "https://sketchfab.com/oauth2/token/";


            // string s = "";
            WWWForm form = new WWWForm();
            form.AddField("grant_type", "password");
            form.AddField("username", username);
            form.AddField("password", password);

            using (UnityWebRequest www = UnityWebRequest.Post(_Url, form))
            {
                string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret));

                www.SetRequestHeader("Authorization", "Basic " + cred);
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Error
                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                    Debug.Log(www.error);
                }
                else
                {
                    if (www.isDone)
                    {
                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                        IAT = (ImplicitAccessToken)JsonUtility.FromJson(jsonResult, typeof(ImplicitAccessToken));
                    }
                }
            }
        }
    }
       
    private IEnumerator PerformSearchNew(string modelName, GameObject model)
    {
        Debug.Log("Performing search for " + modelName);
        oneAtATime = true;

        _nextUrl = string.Empty;
        List<SFSearchResults> searchResults = new List<SFSearchResults>();
        currentPageNumber = 0;
        shouldLoadNextPage = true;

        //keywordToSearch = modelName;

        if (modelName.Equals(""))
        {
            Debug.Log("Please enter a valid keyword!");
            yield return null;
        }
        else
        {
            // string _Url = baseUrl + "tags=" + keywordToSearch + "&downloadable=true&archives_flavours=true";
            string _Url = "https://api.sketchfab.com/v3/search?type=models&q=" + modelName + "&animated=false&downloadable=true&archives_flavours=true";
            _Url = Uri.EscapeUriString(_Url);
            using (UnityWebRequest www = UnityWebRequest.Get(_Url))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Error
                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                    Debug.Log(www.error);
                }
                else
                {
                    if (www.isDone)
                    {
                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                        SFSearchResults _searchResults = (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
                        searchResults.Add(_searchResults);
                        currentPageNumber++;
                        if (!string.IsNullOrEmpty(_searchResults.next))
                        {
                            _nextUrl = _searchResults.next;

                            while (currentPageNumber < maximumPageLimit && shouldLoadNextPage)
                            {
                                shouldLoadNextPage = false;
                                StartCoroutine(LoadNextPages(_nextUrl));

                                while (!shouldLoadNextPage)
                                    yield return null;
                            }
                        }

                        //// Get model download details
                        //int randomElementIndex = 0;// UnityEngine.Random.Range(0, maximumPageLimit);
                        //int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
                        //StartCoroutine(GetModelDetailsNew(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
                        int randomElementIndex = 0;// UnityEngine.Random.Range(0, maximumPageLimit);
                        int n_results = searchResults[randomElementIndex].results.Count;
                        if (n_results != 0)
                        {
                            int randomResultObjectIndex = UnityEngine.Random.Range(0, n_results);
                            StartCoroutine(GetModelDetailsNew(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
                        }
                        else
                        {
                            Debug.Log("Found 0 results on Sketchfab");
                            oneAtATime = false;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator LoadNextPages(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                // Error
                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                Debug.Log(www.error);
            }
            else
            {
                if (www.isDone)
                {
                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                    SFSearchResults _searchResults = (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
                    searchResults.Add(_searchResults);
                    _nextUrl = _searchResults.next;
                    currentPageNumber++;
                    shouldLoadNextPage = true;
                }
            }
        }
    }

    private IEnumerator GetModelDetailsNew(string UID, GameObject model)
    {
        // UID = "cf4194d4ce404273a62c7711447f5f51";
        if (string.IsNullOrEmpty(IAT.access_token))
        {
            StartCoroutine(AuthenticateUser());
            yield return new WaitForSeconds(2f);
            StartCoroutine(GetSpecificModelDetailsNew(UID, model));
        }
        else
        {
            StartCoroutine(GetSpecificModelDetailsNew(UID, model));
            yield return null;
        }
    }

    private IEnumerator GetSpecificModelDetailsNew(string UID, GameObject model)
    {
        string downloadUrl = "https://api.sketchfab.com/v3/models/" + UID + "/download";
        using (UnityWebRequest www = UnityWebRequest.Get(downloadUrl))
        {
            www.SetRequestHeader("Authorization", "Bearer " + IAT.access_token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                // Error
                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                Debug.Log("Error fetching details: " + www.error);
            }
            else
            {
                if (www.isDone)
                {
                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                    _ModelDownloadMetaData modelMetaData = (_ModelDownloadMetaData)JsonUtility.FromJson(jsonResult, typeof(_ModelDownloadMetaData));
                    StartCoroutine(BeginDownloadProcessNew(modelMetaData.gltf.url, model));
                }
            }
        }
    }

    private IEnumerator BeginDownloadProcessNew(string url, GameObject model)
    {
        Debug.Log("Downloading from: " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                Debug.Log("Download err: " + www.error);
            }
            else
            {
                if (operation.isDone)
                {
                    
                    //string modelName = model.name + ".zip";

                    string exportLocation = "";
                    string write_path = "";

                    if (save_models_to_nontemp_folder)
                    {
                        //exportLocation = "C:/Unity/SketchfabModels/" + model.name;
                        //CreateDirectoryIfNotFound(exportLocation);
                        write_path     = Application.persistentDataPath + "/" + model.name + ".zip";
                        exportLocation = Application.persistentDataPath + "/" + model.name;
                    }
                    else
                    {
                        write_path     = Application.temporaryCachePath + "/" + model.name +".zip";
                        exportLocation = Application.temporaryCachePath + "/" + model.name;
                    }
                    Debug.Log("Path: " + write_path);
                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
                    ZipUtil.Unzip(write_path, exportLocation);
                    DisplayModelNew(exportLocation + "/scene.gltf", model);
                }
            }
        }
    }

    //void CreateDirectoryIfNotFound(string dirPath)
    //{
    //    // Check if the directory already exists using the Exists method
    //    if (!Directory.Exists(dirPath))
    //    {
    //        // If not, create the directory using the CreateDirectory method
    //        Directory.CreateDirectory(dirPath);
    //    }
    //}

    //public async Task ImportGLTFAsyncTask(string filepath)
    //{
    //    Importer.ImportGLTFAsync(filepath, new ImportSettings(), OnFinishAsync);
    //}

    public void ImportGLTFAsync(string filepath)
    {
        Importer.ImportGLTFAsync(filepath, new ImportSettings(), OnFinishAsync);
    }

    void OnFinishAsync(GameObject result, AnimationClip[] animations)
    {
        Debug.Log("Finished importing " + result.name);
        result.transform.parent = currentModel.transform;
        CenterCurrentModel();
    }

    Vector3 getCenterCoord(Transform obj)
    {
        Vector3 center = new Vector3();
        if (obj.GetComponent<Renderer>() != null)
        {
            center = obj.GetComponent<Renderer>().bounds.center;
        }
        else
        {
            foreach (Transform subObj in obj)
            {
                center += getCenterCoord(subObj);
            }
            center /= obj.childCount;
        }
        return center;
    }

    //Vector3 GetCenterCoord(GameObject obj, float scale)
    //{
    //    Bounds bounds = new Bounds();
    //    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
    //    foreach (Renderer renderer in renderers)
    //    {
    //        bounds.Encapsulate(renderer.bounds);
    //    }
    //    Vector3 centerPivot = bounds.center;

    //    return centerPivot;
    //}

    void CenterCurrentModel()
    {
        GameObject objParent = new GameObject(currentModel.name);

        Transform tempTransform = currentModel.transform;
        Transform currentParent;
      
        currentModel.transform.parent = objParent.transform;    
        currentModel.name = currentModel.name + "_rescaled";
        currentModel.transform.position = Vector3.zero;
        currentModel.transform.localEulerAngles = new Vector3(0, 0, 0);


        MeshRenderer collider = currentModel.GetComponentInChildren<MeshRenderer>();
        SkinnedMeshRenderer _smr = currentModel.GetComponentInChildren<SkinnedMeshRenderer>();

        Vector3 targetSize = Vector3.one * size;

        Bounds meshBounds = GetRenderBounds(currentModel);


        Vector3 meshSize = meshBounds.size;
        Debug.Log("before rescaling the mesh size is " + meshSize);
        float xScale = targetSize.x / meshSize.x;
        float yScale = targetSize.y / meshSize.y;
        float zScale = targetSize.z / meshSize.z;
        float scaleFactor = Mathf.Min(xScale, yScale, zScale);
        currentModel.transform.localScale *= scaleFactor;
        Debug.Log("rescaled by a factor of " + scaleFactor);

        //Then reposition to center it, with bottom of the bounding box at (0,0,0)
        currentModel.transform.position -= scaleFactor * meshBounds.center;
        currentModel.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, scaleFactor, 0));

        //Finally apply any previous transform
        //This needs to be fixed with scale etc
        objParent.transform.position += tempTransform.position;
        //objParent.transform.localScale *= tempTransform.localScale.x; //assumes uniform scaling previously

        if (createParentCollider)
        {
            objParent.AddComponent<BoxCollider>();
            objParent.GetComponent<BoxCollider>().size = meshSize * scaleFactor;
            objParent.GetComponent<BoxCollider>().center += scaleFactor * meshBounds.center;
            //objParent.GetComponent<BoxCollider>().isTrigger = true;
        }

        //This needs to be fixed, not working currently
        if (tempTransform.parent != null)
        {
            currentParent = tempTransform.parent;
            objParent.transform.parent = currentParent;
        }

        // recomputing all coordinates
        //RecomputeAllChildrenCoord(objParent);
        
        // used to determine whether the objects are ready to move.
        currentModel.tag = "Finish";
        objParent.tag = "Finish";
        oneAtATime = false;
    }

    public void RecomputeAllChildrenCoord(GameObject obj)
    {
        RecomputeCoord(obj);
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            // Get a reference to the child gameobject
            GameObject child = obj.transform.GetChild(i).gameObject;
            RecomputeAllChildrenCoord(child);
        }
    }

    public void RecomputeCoord(GameObject obj)
    {
        // a dummy gameObject whose position is the new pivot point
        GameObject pivot = new GameObject("pivot");
        pivot.transform.position = getCenterCoord(obj.transform);
        pivot.transform.parent = obj.transform;
        Utils.SetParentPivot(pivot.transform);
        // delete the dummy pivot
        DestroyImmediate(pivot);
    }
    

    public void ScaleAndCenter(GameObject model, float scale)
    {
        StartCoroutine(ScaleAndCenterCoroutine(model, scale));
    }

    public IEnumerator ScaleAndCenterCoroutine(GameObject model, float scale)
    {
        while (ReadyToMove(model) == false)
        {
            yield return null;
        }

        if (ReadyToMove(model))
        {
            model.transform.parent.gameObject.transform.localScale *= scale;
            //re-centering
            CenterModel(model);
            yield return null;
        }
    }


    public void CenterModel(GameObject model)
    {
        //print("begin centering model");
        //GameObject objParent = new GameObject(model.name);

        Bounds meshBounds = GetRenderBounds(model);

        //Then reposition to center it, with bottom of the bounding box at (0,0,0)
        model.transform.position -= meshBounds.center;
        model.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, 1, 0));
    }

    private void DisplayModelNew(string path, GameObject model)
    {
        try
        {
            ImportGLTFAsync(path);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            StartCoroutine(PerformSearchNew(model.name, model));// Try again?
            return; // no need to continue here
        }
        
    }

    public bool ReadyToMove(GameObject obj)
    {
        if (obj.tag == "Finish")
            return true;
        else return false;
    }

    private Bounds GetRenderBounds(GameObject obj)
    {
        var r = obj.GetComponent<Renderer>();

        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (r == null)
            r = renderers[0];
        var bounds = r.bounds;
        foreach (Renderer render in renderers)
        {
            if (render != r)
                bounds.Encapsulate(render.bounds);
        }
        return bounds;
    }

    // Places objectToPlace on or next to originalObject, in the direction of the placement
    // vector, making sure that the objects don't overlap. If placement would be within the mesh bounds,
    // then the object is pushed out to the closest point on the surface 
    public void PlaceNextTo(GameObject originalObj, GameObject objToPlace, Vector3 placement)
    {

        // Play with rotation here
        objToPlace.transform.LookAt(originalObj.transform);

        Bounds originalObjBounds = GetRenderBounds(originalObj);
        Bounds objToPlaceBounds = GetRenderBounds(objToPlace);
        Vector3 objToPlaceCenterOffset = objToPlaceBounds.center - objToPlace.transform.position;
        Debug.Log(originalObj.name + " bounds center is " + originalObjBounds.center);
        Debug.Log(originalObj.name + " bounds size is " + originalObjBounds.size);
        Debug.Log(objToPlace.name + " bounds center is " + objToPlaceBounds.center);
        Debug.Log(objToPlace.name + " bounds size is " + objToPlaceBounds.size);

        //Experiment: we're forcing these objects to touch, so rescale placement to something smaller than the objects
        placement = placement.normalized;
        placement *= 0.1f* Math.Min(originalObjBounds.extents.magnitude, objToPlaceBounds.extents.magnitude);


        objToPlace.transform.position = originalObj.transform.position + placement;
        //Debug.Log(objToPlace.name + " position is " + objToPlace.transform.position);
        objToPlaceBounds = GetRenderBounds(objToPlace);
        if (originalObjBounds.Intersects(objToPlaceBounds))
        {
            //First reset
            objToPlace.transform.position = originalObj.transform.position;
            objToPlaceBounds = GetRenderBounds(objToPlace);
            //Shift by the bounds centers
            Vector3 relDisBtwCenters = originalObjBounds.center - objToPlaceBounds.center;
            Debug.Log("Distance between centers is " + relDisBtwCenters);
            objToPlace.transform.position += relDisBtwCenters;
            objToPlaceBounds = GetRenderBounds(objToPlace);
            //Now centers of bounds are at the  same point

            // Construct ray for intersections
            Ray ray = new Ray(originalObjBounds.center, placement);
            //Debug.Log("The ray direction is " + ray.direction);
            //Debug.Log("Going 1 unit along the ray from the bound center gets us to " + ray.GetPoint(1));

            //define distances of intersection of the ray for both of the boxes
            float originalObjDistance;
            float objToPlaceDistance;

            originalObjBounds.IntersectRay(ray, out originalObjDistance);
            objToPlaceBounds.IntersectRay(ray, out objToPlaceDistance);
            Debug.Log("first distance is " + originalObjDistance);
            Debug.Log("second distance is " + objToPlaceDistance);

            Debug.Log("Ray points to " + ray.GetPoint(-originalObjDistance - objToPlaceDistance));
            Debug.Log("relative distance between centers is " + relDisBtwCenters);

            objToPlace.transform.position += ray.GetPoint(-originalObjDistance - objToPlaceDistance) - originalObjBounds.center;//  - objToPlaceCenterOffset; // - relDisBtwCenters
            
            // Place on floor if the first object is on it and there was no upward component to placement
            if (placement.y == 0 && Mathf.Abs(originalObj.transform.position.y) <= 0.001f)
            {
                //Debug.Log("Resetting to floor");
                objToPlace.transform.position = Vector3.Scale(objToPlace.transform.position, new Vector3(1, 0, 1));
            }
        }

    }

    public void DestroyLoadedObjects()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Finish");

        foreach (GameObject obj in objs)
        {
            Destroy(obj);
        }
    }
               

    public void _AuthenticateUser()
    {
        StartCoroutine(AuthenticateUser());
    }

    #region OldStuff

    public class Model
    {
        public GameObject obj;
        public bool readyToMove;

        public Model(string name)
        {
            obj = new GameObject(name);
            readyToMove = false;
        }

        public Model(GameObject oldObj)
        {
            obj = oldObj;
            readyToMove = true;
        }
    }

    private IEnumerator PerformSearch(string modelName, Model model)
    {
        Debug.Log("Performing search for " + modelName);
        oneAtATime = true;

        _nextUrl = string.Empty;
        List<SFSearchResults> searchResults = new List<SFSearchResults>();
        currentPageNumber = 0;
        shouldLoadNextPage = true;

        //keywordToSearch = modelName;

        if (modelName.Equals(""))
        {
            Debug.Log("Please enter a valid keyword!");
            yield return null;
        }
        else
        {
            // string _Url = baseUrl + "tags=" + keywordToSearch + "&downloadable=true&archives_flavours=true";
            string _Url = "https://api.sketchfab.com/v3/search?type=models&q=" + modelName + "&animated=false&downloadable=true&archives_flavours=true";
            _Url = Uri.EscapeUriString(_Url);
            using (UnityWebRequest www = UnityWebRequest.Get(_Url))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Error
                    Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                    Debug.Log(www.error);
                }
                else
                {
                    if (www.isDone)
                    {
                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                        SFSearchResults _searchResults = (SFSearchResults)JsonUtility.FromJson(jsonResult, typeof(SFSearchResults));
                        searchResults.Add(_searchResults);
                        currentPageNumber++;
                        if (!string.IsNullOrEmpty(_searchResults.next))
                        {
                            _nextUrl = _searchResults.next;

                            while (currentPageNumber < maximumPageLimit && shouldLoadNextPage)
                            {
                                shouldLoadNextPage = false;
                                StartCoroutine(LoadNextPages(_nextUrl));

                                while (!shouldLoadNextPage)
                                    yield return null;
                            }
                        }

                        // Get model download details
                        int randomElementIndex = 0;// UnityEngine.Random.Range(0, maximumPageLimit);
                        int randomResultObjectIndex = UnityEngine.Random.Range(0, 24);
                        StartCoroutine(GetModelDetails(searchResults[randomElementIndex].results[randomResultObjectIndex].uid, model));
                    }
                }
            }
        }
    }

    private IEnumerator GetModelDetails(string UID, Model model)
    {
        // UID = "cf4194d4ce404273a62c7711447f5f51";
        if (string.IsNullOrEmpty(IAT.access_token))
        {
            StartCoroutine(AuthenticateUser());
            yield return new WaitForSeconds(2f);
            StartCoroutine(GetSpecificModelDetails(UID, model));
        }
        else
        {
            StartCoroutine(GetSpecificModelDetails(UID, model));
            yield return null;
        }
    }

    private IEnumerator GetSpecificModelDetails(string UID, Model model)
    {
        string downloadUrl = "https://api.sketchfab.com/v3/models/" + UID + "/download";
        using (UnityWebRequest www = UnityWebRequest.Get(downloadUrl))
        {
            www.SetRequestHeader("Authorization", "Bearer " + IAT.access_token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                // Error
                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                Debug.Log("Error fetching details: " + www.error);
            }
            else
            {
                if (www.isDone)
                {
                    string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                    _ModelDownloadMetaData modelMetaData = (_ModelDownloadMetaData)JsonUtility.FromJson(jsonResult, typeof(_ModelDownloadMetaData));
                    StartCoroutine(BeginDownloadProcess(modelMetaData.gltf.url, model));
                }
            }
        }
    }

    private IEnumerator BeginDownloadProcess(string url, Model model)
    {
        Debug.Log("Downloading from: " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                Debug.Log("Download err: " + www.error);
            }
            else
            {
                if (operation.isDone)
                {
                    string modelName = model.obj.name + ".zip";
                    string write_path = Application.persistentDataPath + "/" + modelName;
                    Debug.Log("Path: " + write_path);
                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
                    DisplayModel(write_path, model);
                }
            }
        }
    }

    private void DisplayModel(string path, Model model)
    {
        try
        {

            //var context = gltfImporter.Load(path);
            //context.ShowMeshes();
            Destroy(model.obj);
            //model.obj = context.Root;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            StartCoroutine(PerformSearch(model.obj.name, model));// Try again?
            return; // no need to continue here
            //oneAtATime = false; // Allows next model to be loaded, skip this one
        }

        GameObject objParent = new GameObject(model.obj.name);
        objParent.tag = "Finish";
        model.obj.transform.parent = objParent.transform;
        model.obj.name = model.obj.name + "_rescaled";
        model.obj.transform.position = Vector3.zero;
        model.obj.transform.localEulerAngles = new Vector3(0, 0, 0);
        model.obj.tag = "Finish";

        MeshRenderer collider = model.obj.GetComponentInChildren<MeshRenderer>();
        SkinnedMeshRenderer _smr = model.obj.GetComponentInChildren<SkinnedMeshRenderer>();

        Vector3 targetSize = Vector3.one * size;

        Bounds meshBounds = GetRenderBounds(model.obj);


        Vector3 meshSize = meshBounds.size;
        //Debug.Log("before rescaling the mesh size is " + meshSize);
        float xScale = targetSize.x / meshSize.x;
        float yScale = targetSize.y / meshSize.y;
        float zScale = targetSize.z / meshSize.z;
        float scaleFactor = Mathf.Min(xScale, yScale, zScale);
        model.obj.transform.localScale *= scaleFactor;


        //Then repoisition to center it, with bottom of the bounding box at (0,0,0)
        model.obj.transform.position -= scaleFactor * meshBounds.center;
        model.obj.transform.position += Vector3.Scale(meshBounds.extents, new Vector3(0, scaleFactor, 0));


        // Try a rotation towards the camera?
        //model.obj.transform.LookAt(GameObject.Find("Holodeck").transform);

        model.readyToMove = true;
        oneAtATime = false; // Allow next model to be searched for
    }


    public Model LoadModel(string modelName)
    {
        Model model = new Model(modelName);
        StartCoroutine(_LoadModel(modelName, model));
        return model;
    }

    public void AttachModelToGameObject(Model model, GameObject newParent)
    {
        StartCoroutine(AttachTest(model, newParent));
    }

    public void MoveModel(Model model, Vector3 move)
    {
        StartCoroutine(MoveTest(move, model));
    }
    public void ScaleModel(Model model, float scale)
    {
        StartCoroutine(RescaleTest(scale, model));
    }

    public void AddPhysics(Model model, float mass = 1)
    {
        StartCoroutine(PhysicsTest(model, mass));
    }

    public void PlaceModelNextTo(Model originalModel, Model modelToPlace, Vector3 move)
    {
        StartCoroutine(MoveTest2(move, originalModel, modelToPlace));
    }

    public void ReplaceModel(Model oldModel, string newModelName)
    {
        Model newModel = LoadModel(newModelName);
        StartCoroutine(ReplaceTest(newModel, oldModel));
    }
    public void ReplaceModel(GameObject oldModel, string newModelName)
    {
        Model newModel = LoadModel(newModelName);
        StartCoroutine(ReplaceTest(newModel, new Model(oldModel)));
    }

    private IEnumerator _LoadModel(string modelname, Model model)
    {
        while (oneAtATime)
        {
            yield return null;
        }
        if (oneAtATime == false)
        {
            yield return StartCoroutine(PerformSearch(modelname, model));
        }

    }

    private IEnumerator AttachTest(Model model, GameObject newParent)
    {
        while (model.readyToMove == false)
        {
            yield return null;
        }

        if (model.readyToMove)
        {
            model.obj.transform.parent.gameObject.transform.parent = newParent.transform;
            model.obj.transform.parent.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            yield return null;
        }

    }

    private IEnumerator ReplaceTest(Model model, Model oldObject)
    {
        while (model.readyToMove == false)
        {
            yield return null;
        }

        if (model.readyToMove)
        {
            model.obj.transform.position = oldObject.obj.transform.position;
            model.obj.transform.rotation = oldObject.obj.transform.rotation;
            Destroy(oldObject.obj);
            yield return null;
        }

    }

    private IEnumerator MoveTest(Vector3 move, Model model)
    {
        while (model.readyToMove == false)
        {
            yield return null;
        }

        if (model.readyToMove)
        {
            model.obj.transform.parent.gameObject.transform.position = move;
            yield return null;
        }

    }

    private IEnumerator PhysicsTest(Model model, float mass)
    {
        while (model.readyToMove == false)
        {
            yield return null;
        }

        if (model.readyToMove)
        {
            //First add colliders to all children
            foreach (Transform child in model.obj.transform.GetComponentsInChildren<Transform>())
            {
                MeshCollider col = child.gameObject.AddComponent<MeshCollider>();
                col.convex = true;
            }

            Rigidbody rb = model.obj.transform.parent.gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            yield return null;
        }

    }

    private IEnumerator RescaleTest(float scale, Model model)
    {
        while (model.readyToMove == false)
        {
            yield return null;
        }

        if (model.readyToMove)
        {
            model.obj.transform.parent.gameObject.transform.localScale *= scale;
            //Need to reposition it
            yield return null;
        }

    }

    private IEnumerator MoveTest2(Vector3 move, Model model1, Model Model2)
    {
        while (model1.readyToMove == false || Model2.readyToMove == false)
        {
            yield return null;
        }

        if (model1.readyToMove && Model2.readyToMove)
        {
            Debug.Log("moving apple");
            PlaceNextTo(model1.obj.transform.parent.gameObject, Model2.obj.transform.parent.gameObject, move);
            yield return null;
        }

    }



    #endregion


}


