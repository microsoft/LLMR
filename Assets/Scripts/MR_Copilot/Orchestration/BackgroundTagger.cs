using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class BackgroundTagger : MonoBehaviour
{
    public bool only_run_once;
    public bool needs_to_run;
    public List<GameObject> obj_placeholders;
    public DepthAnalyzer depth_analyzer;
    public Screenshotter screen_shotter;
    public BackgroundObjectDetector obj_detector;
    public GameObject placeholder_parent;
    public GameObject background_parent;
    public GameObject background;

    public List<GameObject> disabled_obj;
    public List<GameObject> ignored_obj;
    public Dictionary<GameObject, bool> ignored_dict;

    public bool use_cube_for_placeholders;

    //public Texture2D debug_img;

    void Awake()
    {
        //base.Start();
        obj_placeholders = new List<GameObject>();
        disabled_obj = new List<GameObject>();
        ConstructIgnoredDict();
        needs_to_run = true; // needs to process the background at least once

        // TODO: see if this is the right thing to do
        if (background.GetComponent<MeshCollider>() == null)
        {
            background.AddComponent<MeshCollider>();
        }

        // so that the scene hierarchy cleaner ignores everything in the background
        //background_parent.AddComponent<Background>();

        placeholder_parent.AddComponent<Placeholder>();
    }

    void ConstructIgnoredDict()
    {
        ignored_dict = new Dictionary<GameObject, bool>();
        foreach (GameObject game_obj in ignored_obj)
        {
            ignored_dict[game_obj] = true;
        }
    }

    public async Task ProcessBackground()
    {
        // flag the module as having ran at least once
        if (only_run_once) { needs_to_run = false; }

        // TODO: Refresh mesh. Necessary if e.g., new objects are introduced into the background
        // TODO: think about what is the best way of doing this for HL2

        // take a snapshot of only the background
        DisableAllButBackground();
        byte[] background_img = screen_shotter.GetSnapshot();
        //screen_shotter.TakeSnapshot();

        // pass the snapshot to SEEM, get back a list of detected objects
        //await obj_detector.ProcessBackgroundObjects(screen_shotter.snapshot_dir);
        await obj_detector.ProcessBackgroundObjects(background_img);
        DetectedObject[] detected_obj = obj_detector.detected_objects;

        // get object depths
        //List<float> depths = depth_analyzer.GetDepths(detected_obj);
        List<Vector3> placeholder_coords = depth_analyzer.GetObjCoords(detected_obj);

        // destory previous placeholders
        DestoryPlaceholders();

        // create placeholders on the object locations
        CreatePlaceholders(detected_obj, placeholder_coords);
        EnableAllDisabledObjects();
    }

    //void RefreshMesh()
    //{
    //    if (background.GetComponent<MeshCollider>() != null)
    //    {
    //        background.RemoveComponent<MeshCollider>()
    //    }
    //}

    void DestoryPlaceholders()
    {
        foreach(GameObject ph in obj_placeholders)
        {
            DestroyImmediate(ph);
        }

        obj_placeholders = new List<GameObject>();
    }

    void CreatePlaceholders(DetectedObject[] detected_obj, List<Vector3> obj_coords)
    {
        for (int i = 0; i < detected_obj.Length; i++)
        {
            DetectedObject obj = detected_obj[i];
            //float depth = depths[i];
            Vector3 coord = obj_coords[i];
            GameObject placeholder = new GameObject(obj.description);
            if (use_cube_for_placeholders)
            {
                placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = obj.description;
            }
            // so that the placeholder does not get deleted during the scene hierarchy clean up
            placeholder.AddComponent<Placeholder>();

            //placeholder.transform.position = Utils.image_to_world_space(obj.center, depth);
            placeholder.transform.position = coord;
            placeholder.transform.SetParent(placeholder_parent.transform);
        }
    }

    void DisableAllButBackground()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!ignored_dict.ContainsKey(root)) 
            {
                //print(root.name);
                disabled_obj.Add(root);
                root.SetActive(false);
            }

        }
    }

    void EnableAllDisabledObjects()
    {
        foreach (GameObject dis_o in disabled_obj)
        {
            dis_o.SetActive(true);
        }

        // clear the list of disabled objects
        disabled_obj = new List<GameObject>();
    }

    // for debugging
    public async Task ProcessBackgroundDebug(string snapshot_dir)
    {
        // take a snapshot of the background
        //screen_shotter.TakeSnapshot();
        //string snapshot_dir = Application.dataPath + "/Images/table.png";

        // pass the snapshot to SEEM, get back a list of detected objects
        await obj_detector.ProcessBackgroundObjects(snapshot_dir);
        DetectedObject[] detected_obj = obj_detector.detected_objects;

        // get object depths
        List<Vector3> obj_coords = depth_analyzer.GetObjCoords(detected_obj);

        // destory previous placeholders
        DestoryPlaceholders();

        // create placeholders on the object locations
        CreatePlaceholders(detected_obj, obj_coords);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
