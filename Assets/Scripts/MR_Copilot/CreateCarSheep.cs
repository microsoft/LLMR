//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;
//using TMPro;


//public class CreateCarSheep : MonoBehaviour
//{
//    // a reference to the Find3DModelsSingle script
//    private Find3DModelsSingle finder;

//    // a method to find the script and call its methods
//    public void Start()
//    {
//        // find the game object with the Find3DModelsSingle script attached
//        GameObject finderObject = GameObject.FindObjectOfType<Find3DModelsSingle>().gameObject;

//        // get the script component from the game object
//        finder = finderObject.GetComponent<Find3DModelsSingle>();

//        // check if the script is not null
//        if (finder != null)
//        {
//            // disable the canvas to hide the previous image
//            finder.imageCanvas.enabled = false;

//            // create a list of models with labels and positions
//            finder.models = new List<ModelData>() {
//                new ModelData("", "rooster", new Vector3(-0.826f, 0, -7.855f), 1f),
//                new ModelData("", "red chair", new Vector3(0.826f, 0, -7.855f), 0.5f),
//                new ModelData("", "siamese cat", new Vector3(0, 0, -7.855f), 0.2f)
//            };

//            // call the Find3DModelsClosestToLabels method with the list and a callback
//            finder.Find3DModelsClosestToLabels(finder.models, (uid, index) =>
//            {
//                // do something with the uid and the index, for example, update the model data
//                finder.models[index].uid = uid;
//                Debug.Log("UID for label: " + uid);
//            });
//        }
//        else
//        {
//            // log an error if the script is not found
//            Debug.LogError("Find3DModelsSingle script not found!");
//        }
//    }
//}