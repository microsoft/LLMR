//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(PolyPizzaAPIManager))]
//public class API : Editor
//{
//    private PolyPizzaAPIManager _target;

//    private void Awake()
//    {
//        _target = (PolyPizzaAPIManager) target;        
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Perform Search"))
//        {
//            _target._PerformSearch();
//        }

//        if (GUILayout.Button("Delete all files"))
//        {
//            _target.DeleteFiles();
//        }
//    }
//}
