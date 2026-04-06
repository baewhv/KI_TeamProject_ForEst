using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedBean3 : BaseSeedBean
{
    private void Awake()
    {
        Init();
    }
    
    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        
        if (!other.CompareTag("Player")) return;
        
        string sceneName = SceneManager.GetActiveScene().name;
        
        switch (sceneName)
        {
            case "StageT":
                if (transform.position.y > 5f)
                    targetData = _seedBeanDataList.Find(x => x.id == 4016);
                else if (transform.position.y < -5f)
                    targetData = _seedBeanDataList.Find(x => x.id == 4007);
                else 
                    targetData = _seedBeanDataList.Find(x => x.id == 4005);
                break;
            case "Stage1":
                if (transform.position.y < -5f)
                    targetData = _seedBeanDataList.Find(x => x.id == 4010);
                break;
            default:
                var textList = _seedBeanDataList.Where(x => x.stage == "all").ToList();
                if (textList.Count > 0) targetData = textList[UnityEngine.Random.Range(0, textList.Count)];
                break;
        }
        
        _text.text = GetTextLanguage(targetData);
    }

    private void Init()
    {
        base.Init();
        _anim.SetBool("Sit", RandomSetBool());
    }
}
