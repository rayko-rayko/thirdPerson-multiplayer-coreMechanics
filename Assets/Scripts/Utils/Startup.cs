using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    
    public static void InstantiatePrefabs()
    {
        Debug.Log("-- Instantiating object --");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach (GameObject prefab in prefabsToInstantiate)
        {
            Debug.Log($"Creating {prefab.name}");

            GameObject.Instantiate(prefab);
        }
        
        Debug.Log("-- Instantiating object done --");
    }
}
