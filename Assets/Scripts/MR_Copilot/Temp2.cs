//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//using UnityEngine.Networking;
////using UniGLTF;

//using Siccity.GLTFUtility;

//using System.IO;
//using System.Text;
//using Unity.VisualScripting;
//using UnityEngine.UIElements;


//public class Temp2 : MonoBehaviour
//{
//    //public GameObject toDisplay;
//    public GameObject room;
//    public GameObject clock;
//    public SketchfabLoader sketchfabLoader;
//    // Start is called before the first frame update

//    void Start()
//    {
//        sketchfabLoader.currentModel = room;
//        string exportLocation = Application.dataPath + "/SketchfabModels/Toy";
//        sketchfabLoader.ImportGLTFAsync(exportLocation + "/scene.gltf");
//        //string exportLocation = "C:\\Users\\t-billhuang\\Documents\\SketchfabExport\\test2\\Room.gltf";
//        //sketchfabLoader.ImportGLTFAsync(exportLocation);
//        //float scale = 1 / 0.004600065f;
//        //sketchfabLoader.ScaleAndCenter(room, scale);
//    }

//    //async void Start()
//    //{

//    //    var gltf = new GLTFast.GltfImport();
//    //    //var settings = new ImportSettings
//    //    //{
//    //    //    GenerateMipMaps = true,
//    //    //    AnisotropicFilterLevel = 3,
//    //    //    NodeNameMethod = NameImportMethod.OriginalUnique
//    //    //};
//    //    var success = await gltf.Load("file:///C:/Users/t-billhuang/Documents/SketchfabExport/Toybox_orig/scene.gltf");
//    //    //var success = await gltf.Load("file:///C:/Users/t-billhuang/Documents/SketchfabExport/Toybox_gltFast_exportFromOrig/glTF.gltf");
//    //    if (success)
//    //    {
//    //        var gameObject = new GameObject("glTF");
//    //        await gltf.InstantiateMainSceneAsync(gameObject.transform);
//    //    }
//    //    else
//    //    {
//    //        Debug.LogError("Loading glTF failed!");
//    //    }
//    //}


//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
