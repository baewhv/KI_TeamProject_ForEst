using System;
using CutScene;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private string _cutsceneName;

    public void Init(CutsceneTriggerData data)
    {
        _cutsceneName = data.triggerSOName;
        transform.position = data.position;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CutSceneManager.Instance.PlayCutscene(_cutsceneName);
            Destroy(gameObject);
        }
    }
}