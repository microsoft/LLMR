using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadBathroom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject bathroom = Instantiate(Resources.Load("Models/bathroom-interior2/source/bathroom02", typeof(GameObject))) as GameObject;
        bathroom.transform.position = new Vector3(0, 0, 0);
        ProcessBathroomObjects();
    }

    void ProcessBathroomObjects()
    {
        // bathtub
        GameObject bathtub = GameObject.Find("ceramic_objects");
        bathtub.name = "toilet_bathtub_sink";
        Vector3 bathtub_pos = new Vector3(5.13f, -0.5f, -9.19f);
        SetPivotAtLocalPos(bathtub, bathtub_pos);
        bathtub.AddComponent<MeshCollider>();

        // faucet
        GameObject faucet = GameObject.Find("faucet&handle");
        Vector3 sink_faucet_pos = new Vector3(-23.95f, 5.62f, 22.1f);
        SetPivotAtLocalPos(faucet, sink_faucet_pos);
        faucet.AddComponent<MeshCollider>();

        // paintings
        GameObject paintings = GameObject.Find("decoreplate");
        paintings.name = "paintings";
        paintings.AddComponent<MeshCollider>();
    }


    void SetPivotAtLocalPos(GameObject obj, Vector3 pos)
    {
        GameObject pivot = new GameObject();
        pivot.transform.localRotation = Quaternion.identity;
        pivot.transform.SetParent(obj.transform);
        pivot.transform.localPosition = pos;
        Utils.SetParentPivot(pivot.transform);
        DestroyImmediate(pivot);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
