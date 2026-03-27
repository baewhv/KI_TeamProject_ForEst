using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPrefab
    {
        public GameObject prefab;
        public string layer;
    }
    
    public List<SpawnPrefab> spawnPrefabs;
    public Transform tileMap;

    private void Awake()
    {
        Spawnning();
    }

    private void Spawnning()
    {
        SpriteRenderer[] dummys = tileMap.GetComponentsInChildren<SpriteRenderer>();

        foreach (var dummy in dummys)
        {
            string dummyLayer = LayerMask.LayerToName(dummy.gameObject.layer);
            
            SpawnPrefab prefab = spawnPrefabs.Find(prefab => prefab.layer == dummyLayer);

            if (prefab.prefab != null)
            {
                GameObject spawnObj = Instantiate(prefab.prefab, dummy.transform.position, Quaternion.identity);
                if(dummy.transform.position.y < -1 && spawnObj.TryGetComponent<Obstacle.Obstacle>(out Obstacle.Obstacle obstacle))
                {
                    obstacle._isThisObjBelongsToTheReverseWorld = true;
                    obstacle.ReversingState();
                };
                
                Destroy(dummy.gameObject);
            }
        }
    }
}
