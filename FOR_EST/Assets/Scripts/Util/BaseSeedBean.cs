using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public abstract class BaseSeedBean : MonoBehaviour
{
    public static Language currentLanguage = Language.KR;
    protected TextAsset _seedbeanTextFile;
    protected List<SeedBeanDialogueData> _seedBeanDataList;
    private string regex = new string(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
    protected Animator _anim;
    protected SpriteRenderer _renderer;
    protected Collider2D _collider;
    protected GameObject textBox;
    protected Canvas textBoxCanvas;
    protected TextMeshProUGUI _text;
    protected SeedBeanDialogueData targetData;

    
    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (SceneManager.GetActiveScene().name != "StageT" || SceneManager.GetActiveScene().name != "Stage1")
            {
                var textList = _seedBeanDataList.Where(x => x.stage == "all").ToList();
                if (textList.Count > 0)
                {
                    int randomIndex = Random.Range(0, textList.Count);
                    _text.text = GetTextLanguage(textList[randomIndex]);
                }
            }
            
            textBox.SetActive(true);
        }
    }

    public virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            textBox.SetActive(false);
        }
    }
    
    public virtual void Init()
    {
        _seedbeanTextFile = Resources.Load<TextAsset>("BeanText");
        _seedBeanDataList = new List<SeedBeanDialogueData>();
        _anim = GetComponentInChildren<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        textBoxCanvas = GetComponentInChildren<Canvas>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _text.text = GetTextLanguage(targetData);
        
        LoadCSV();
        
        if (textBoxCanvas != null)
        {
            textBox = textBoxCanvas.gameObject;
            textBox.SetActive(false);
        }
        
        if (transform.position.y < -1)
        {
            _anim.SetBool("Reverse", true);
            _renderer.flipY = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + 1.2f);
            _collider.offset = new Vector2(_collider.offset.x, _collider.offset.y - 1f);
            textBox.transform.position = new Vector2(transform.position.x, transform.position.y - 2.5f);
        }
        else transform.position = new Vector2(transform.position.x, transform.position.y + -0.2f);
    }

    public void LoadCSV()
    {
        string[] lines = _seedbeanTextFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            string[] row = Regex.Split(lines[i], regex);

            if(row.Length < 2) continue;
            
            SeedBeanDialogueData data = new SeedBeanDialogueData();
            
            data.id = int.Parse(row[0].Trim().Replace("\uFEFF", ""));
            data.type = int.Parse(row[1].Trim());
            data.textKR = row[2].Trim(); 
            data.textEN = row[3].Trim('\"');
            data.textJP = row[4].Trim();
            data.stage = row[5].Trim();
        
            _seedBeanDataList.Add(data);
            
            _seedBeanDataList.Add(data);
            
        }
    }
    
    protected string GetTextLanguage(SeedBeanDialogueData data)
    {
        if (data == null) return "";

        switch (currentLanguage)
        {
            case Language.KR: return data.textKR;
            case Language.EN: return data.textEN;
            case Language.JP: return data.textJP;
            default: return data.textKR;
        }
    }
    
    public virtual bool RandomSetBool()
    {
        int check = UnityEngine.Random.Range(0, 1);
        if(check == 0) return true;
        return false;
    }

    public virtual int RandomTextNumber()
    {
        int textNumber = UnityEngine.Random.Range(8, 20);
        return textNumber;
    }
}

[System.Serializable] public class SeedBeanDialogueData
{
    public int id;
    public int type;
    public string textKR;
    public string textEN;
    public string textJP;
    public string stage;
}

public enum Language
{
    KR,
    EN,
    JP
}
