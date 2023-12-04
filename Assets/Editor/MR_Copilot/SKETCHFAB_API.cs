//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(SketchfabAPIManager))]
//public class SKETCHFAB_API : Editor
//{
//    private SketchfabAPIManager _target;

//    private void Awake()
//    {
//        _target = (SketchfabAPIManager) target;        
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Perform Search"))
//        {
//            _target._PerformSearch();
//        }

//        if (GUILayout.Button("Authenticate User"))
//        {
//            _target._AuthenticateUser();
//        }

//        if (GUILayout.Button("Remove Files"))
//        {
//            _target.DeleteFiles();
//        }
//    }
//}
