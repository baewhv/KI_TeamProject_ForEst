using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class BaseFruitTextData : BaseInteractionObject
{
    [Header("말풍선 등장 속도")]
    [SerializeField] protected float _textSpeed = 0.2f;
    
    private string regex = new string(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
    protected TextAsset _fruitTextFile;
    protected GameObject _textBox;
    protected Canvas _textBoxCanvas;
    protected TextMeshProUGUI _text;
    protected List<FruitDialogueData> _fruitDataList = new List<FruitDialogueData>();
    protected FruitDialogueData _targetData;

    public override void Init()
    {
        _fruitTextFile = Resources.Load<TextAsset>("SadFruit");
        _fruitDataList = new List<FruitDialogueData>();
        _textBoxCanvas = GetComponentInChildren<Canvas>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _text.text = GetTextLanguage(_targetData);
        base.Init();
        
        LoadCSV();
        
        if (_textBoxCanvas != null)
        {
            _textBox = _textBoxCanvas.gameObject;
            _textBox.SetActive(false);
        }
    }

    private void Update()
    {
        _text.text = GetTextLanguage(_targetData);
    }
    
    public void LoadCSV()
    {
        string[] lines = _fruitTextFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            string[] row = Regex.Split(lines[i], regex);

            if(row.Length < 2) continue;
            
            FruitDialogueData data = new FruitDialogueData();
            
            data.id = int.Parse(row[0].Trim().Replace("\uFEFF", ""));
            data.textKR = row[1].Trim('\"'); 
            data.textEN = row[2].Trim('\"');
            data.textJP = row[3].Trim();
        
            _fruitDataList.Add(data);
        }
    }
    
    protected string GetTextLanguage(FruitDialogueData data)
    {
        if (data == null) return "";

        switch (LanguageSetting.currentLanguage)
        {
            case Language.KR: return data.textKR;
            case Language.EN: return data.textEN;
            case Language.JP: return data.textJP;
            default: return data.textEN;
        }
    }
    
    public void SetDataWithID(int id)
    {
        if(_fruitDataList == null) LoadCSV();
        
        _targetData = _fruitDataList.Find(x => x.id == id);
        if  (_targetData != null) _text.text = GetTextLanguage(_targetData);
    }
    
    public IEnumerator TextVisibleRoutine()
    {
        yield return  YieldContainer.WaitForSeconds(0.01f);
        
        _renderer.enabled = false;
        _collider.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePosition; 
        _rb.linearVelocity = Vector2.zero;
        _textBox.SetActive(true);
        _text.ForceMeshUpdate();
        int visibleText = _text.textInfo.characterCount;
        _text.maxVisibleCharacters = 0;

        for (int i = 0; i <= visibleText; i++)
        {
            _text.maxVisibleCharacters = i;
            yield return YieldContainer.WaitForSeconds(_textSpeed);
        }
        
        yield return YieldContainer.WaitForSeconds(1f);
        
        gameObject.SetActive(false);
        if (!gameObject.activeSelf)
        {
            GameManager.Instance.FruitCount--;
            GameManager.Instance.CheckClear();
        }
    }
    
    public override void Respawn()
    {
        if (_textRoutine != null)
        {
            StopCoroutine(_textRoutine);
            _textRoutine = null;
        }
        
        _textBox.SetActive(false);
        
        base.Respawn();
    }
}
