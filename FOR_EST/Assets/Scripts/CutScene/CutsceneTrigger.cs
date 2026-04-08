using System;
using CutScene;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    private string _cutsceneName;
    private string _triggerTargetTag;

    public void Init(CutsceneTriggerData data)
    {
        _cutsceneName = data.triggerSOName;
        transform.position = data.position;
        _triggerTargetTag = data.triggerTargetTag;
        GetComponent<BoxCollider2D>().size = data.triggerSize;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_triggerTargetTag))
        {
            CutSceneManager.Instance.EnqueueCutscene(_cutsceneName);
            Destroy(gameObject);
        }
    }
}