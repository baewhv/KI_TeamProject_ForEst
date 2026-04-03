using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPrefab
    {
        public GameObject prefab;
        public string layer;
    }
    
    public List<SpawnPrefab> spawnPrefabs;
    private List<IRespawnable> _respawnable;
    public Transform tileMap;
    private UserInput _input;
    private bool isRespawning = false;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        _input.asset.Enable();
        _input.Player.Restart.performed += OnRespawn;
    }

    private void OnDisable()
    {
        _input.Player.Restart.performed -= OnRespawn;
        _input.asset.Disable();
    }

    private void OnRespawn(InputAction.CallbackContext ctx)
    {
        Respawn();
    }

    private void Init()
    {
        _input = new UserInput();
        _respawnable = new List<IRespawnable>();
        SetSpawnData();
    }

    private void Respawn()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            foreach (var gameObj in _respawnable)
            {
             
                gameObj?.Respawn();
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return YieldContainer.WaitForSeconds(1f);
        isRespawning = false;
    }

    private void SetSpawnData()
    {
        SpriteRenderer[] dummys = tileMap.GetComponentsInChildren<SpriteRenderer>();

        foreach (var dummy in dummys)
        {
            string dummyLayer = LayerMask.LayerToName(dummy.gameObject.layer);
            
            SpawnPrefab prefab = spawnPrefabs.Find(prefab => prefab.layer == dummyLayer);

            if (prefab.prefab != null)
            {
                GameObject spawnObj = null;
                Transform spawnTransform = dummy.transform.Find("point");
                if (spawnTransform != null)
                {
                    Debug.Log(dummy.transform.Find("point"));
                    Vector2 spawnPos = spawnTransform.position;
                    Debug.Log(spawnPos);
                    spawnObj = Instantiate(prefab.prefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    spawnObj = Instantiate(prefab.prefab, dummy.transform.position, Quaternion.identity);
                }
                
                IRespawnable respawnable = spawnObj.GetComponent<IRespawnable>();
                if (respawnable != null) _respawnable.Add(respawnable);
                
                Destroy(dummy.gameObject);
            }
        }
    }
}
