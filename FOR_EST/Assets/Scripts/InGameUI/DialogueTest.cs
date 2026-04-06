using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Dialogue : SingletonMonoBehaviour<Dialogue>
{
    public TMP_Text dialogueText;

    Dictionary<int, string> textDict = new Dictionary<int, string>();
    Dictionary<int, int> nextDict = new Dictionary<int, int>();
    Dictionary<int, string> speakerDict = new Dictionary<int, string>();
    public RectTransform dialogueBox;
    public Vector3 offset;
    Transform currentTarget;
    private int _currentID;
    public int languageIndex = 5; // CSV파일에서 텍스트가 있는 열의 인덱스 5 : 한국어, 6: 영어, 7: 일본어
    public const int minLang = 5;
    public const int maxLang = 7;
    public bool IsPlay { get; private set; }

    public void NextLanguage()
    {
        languageIndex++;

        if (languageIndex > maxLang)
            languageIndex = minLang;

        SetLanguage();
    }

    public void PrevLanguage()
    {
        languageIndex--;
        if (languageIndex < minLang)
            languageIndex = maxLang;

        SetLanguage();
    }
    private void SetLanguage()
    {
        textDict.Clear();
        nextDict.Clear();
        speakerDict.Clear();
        LoadCSV();

        if (IsPlay)
            ShowDialogue();
    }
    public int CurrentID
    {
        set => _currentID = value;
    }

    protected override void Awake()
    {
        base.Awake();
        LoadCSV();
    }

    public void StartDialog(int id)
    {
        _currentID = id; // CSV파일의 시작할 텍스트 ID를 입력해주세요.
        IsPlay = true;
        ShowDialogue();
    }

    void Update()
    {
        if (currentTarget == null)
        {
            //Debug.Log("타겟 없음");
            return;
        }

        UpdatePosition(currentTarget);
    }

    public void PressG(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            NextDialogue();
        }
    }

    void LoadCSV()
    {
        TextAsset csv = Resources.Load<TextAsset>("Tutorial 1"); //가져올 CSV 파일의 이름을 입력해주세요. 
        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split(',');

            if (row.Length < 6) continue;


            string idStr = row[0];
            string nextStr = row[1];
            string speaker = row[2];


            if (!int.TryParse(idStr, out int id)) continue;
            if (!int.TryParse(nextStr, out int nextId)) continue;

            string text = row[languageIndex];

            textDict[id] = text;
            nextDict[id] = nextId;
            speakerDict[id] = speaker;
        }
    }

    void ShowDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
        if (!textDict.ContainsKey(_currentID)) return;

        dialogueText.text = textDict[_currentID];

        if (!speakerDict.ContainsKey(_currentID)) return;

        string speaker = speakerDict[_currentID];

        currentTarget = GetTargetBySpeaker(speaker);
    }

    void NextDialogue()
    {
        if (!nextDict.ContainsKey(_currentID)) return;

        int nextID = nextDict[_currentID];

        if (nextID == 0)
        {
            //dialogueText.text = "끝!";
            dialogueBox.gameObject.SetActive(false);
            IsPlay = false;
            return;
        }

        _currentID = nextID;
        ShowDialogue();
    }

    Transform GetTargetBySpeaker(string speaker)
    {
        //Debug.Log("speaker: [" + speaker + "]");

        if (speaker == "에스트")
        {
            return CutSceneManager.Instance.Player.transform;
        }
        else if (speaker == "시드")
        {
            return CutSceneManager.Instance.Seed.transform;
        }

        return null;
    }

    void UpdatePosition(Transform target)
    {
        if (target == null) return;

        // Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        Vector3 screenPos = target.position + offset;
        Debug.Log(screenPos);
        dialogueBox.position = screenPos;
    }
}