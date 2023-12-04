//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
////using UniGLTF;
//using System.IO;
//using UnityEngine.UI;
//using TMPro;

//public class PolyPizzaAPIManager : MonoBehaviour
//{
//    [System.Serializable]
//    public class Creator
//    {
//        public string Username;
//        public string DPURL;
//    }

//    [System.Serializable]
//    public class Orbit
//    {
//        public string phi;
//        public string radius;
//        public string theta;
//    }

//    [System.Serializable]
//    public class Result
//    {
//        public string ID;
//        public string Title;
//        public string Attribution;
//        public string Thumbnail;
//        public string Download;
//        public int TriCount;
//        public Creator Creator;
//        public string Category;
//        public List<string> Tags;
//        public string Licence;
//        public bool Animated;
//        public Orbit Orbit;
//        public string Description;
//    }

//    [System.Serializable]
//    public class SearchResults
//    {
//        public int total;
//        public List<Result> results;
//    }

//    private string keywordToSearch;
//    private GameObject spawnedModel;
//    public string baseSearchUrl = "https://api.poly.pizza/v1/search/";
//    private string API_KEY = "e1718e5af93447c68f417cbe4922773a";
//    public SearchResults searchResults;
//    public Button searchBtn;
//    public TMP_InputField inputField;

//    private void Start()
//    {
//        DeleteFiles();
//        spawnedModel = null;
//        searchBtn.onClick.AddListener(() => StartCoroutine(PerformSearch()));
//    }

//    private IEnumerator PerformSearch()
//    {
//        keywordToSearch = inputField.text.ToString();
//        if (keywordToSearch.Equals(""))
//        {
//            Debug.Log("Please enter a valid keyword!");
//            yield return null;
//        }
//        else
//        {
//            if (spawnedModel != null)
//            {
//                Destroy(spawnedModel);
//            }

//            DeleteFiles();

//            string _Url = baseSearchUrl + keywordToSearch;

//            using (UnityWebRequest www = UnityWebRequest.Get(_Url))
//            {
//                www.SetRequestHeader("Content-Type", "application/json");
//                www.SetRequestHeader("x-Auth-Token", API_KEY);
//                yield return www.SendWebRequest();
//                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    // Error
//                    // Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                    // Debug.Log(www.error);
//                }
//                else
//                {
//                    if (www.isDone)
//                    {
//                        string jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
//                        searchResults =  (SearchResults)JsonUtility.FromJson(jsonResult, typeof(SearchResults));

//                        GetRandomModelAndDisplay();
//                    }
//                }
//            }
//        }
//    }

//    public void _PerformSearch()
//    {
//        StartCoroutine(PerformSearch());
//    }

//    private void GetRandomModelAndDisplay()
//    {
//        int randomModel = Random.Range(0, searchResults.results.Count);
//        StartCoroutine(DownloadModel(searchResults.results[randomModel].Download));
//    }

//    private IEnumerator DownloadModel(string modelUrl)
//    {
//        using (UnityWebRequest www = UnityWebRequest.Get(modelUrl))
//        {
//            yield return www.SendWebRequest();
            
//            while (!www.isDone)
//            {
//                Debug.Log("Download Stat: " + www.downloadProgress);
//                yield return null;
//            }

//            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//            {
//                Debug.Log(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
//                Debug.Log("Custom action Error: " + www.error);
//            }
//            else
//            {
//                if (www.isDone)
//                {
//                    string modelName = keywordToSearch+".glb";
//                    string write_path = Application.persistentDataPath + "/" + modelName;
//                    System.IO.File.WriteAllBytes(write_path, www.downloadHandler.data);
//                    DisplayModel(write_path);
//                }
//            }
//        }
//    }

//    public float size;

//    //private void DisplayModel(string path)
//    //{ 
//    //    var context = gltfImporter.Load(path);
//    //    context.ShowMeshes();
//    //    spawnedModel = context.Root;
//    //    spawnedModel.transform.position = Vector3.zero;
//    //    spawnedModel.transform.localEulerAngles = new Vector3(0, 30, 0);

//    //    MeshRenderer collider = spawnedModel.GetComponentInChildren<MeshRenderer>();
//    //    SkinnedMeshRenderer _smr = spawnedModel.GetComponentInChildren<SkinnedMeshRenderer>();

//    //    Vector3 targetSize = Vector3.one * size;
        
//    //    Bounds meshBounds;

//    //    if (collider != null)
//    //    {
//    //        meshBounds = collider.bounds;
//    //    }
//    //    else
//    //    {
//    //        meshBounds = _smr.bounds;
//    //    }

//    //    Vector3 meshSize = meshBounds.size;
//    //    float xScale = targetSize.x / meshSize.x;
//    //    float yScale = targetSize.y / meshSize.y;
//    //    float zScale = targetSize.z / meshSize.z;
//    //    spawnedModel.transform.localScale = new Vector3(xScale, yScale, zScale);
//    //}

//    public void DeleteFiles()
//    {
//        string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
//        foreach (string filePath in filePaths)
//            File.Delete(filePath); 
//    }
//}
