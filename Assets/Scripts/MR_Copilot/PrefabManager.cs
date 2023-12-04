using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public List<GameObject> prefab_list;
    public Dictionary<string, GameObject> prefab_dict;
    //public Dictionary<string, int> prefab_idx_dict;

    void Awake()
    {
        ConstructPrefabDict();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ConstructPrefabDict()
    {
        prefab_dict = new Dictionary<string, GameObject> ();
        for (int i = 0; i < prefab_list.Count; i++)
        {
            GameObject prefab = prefab_list[i];
            prefab_dict[prefab.name] = prefab;
        }
    }

    public GameObject GetPrefab(string prefab_name)
    {
        if (prefab_dict.ContainsKey(prefab_name))
        {
            return prefab_dict[prefab_name];
        }

        return null;
    }


}
