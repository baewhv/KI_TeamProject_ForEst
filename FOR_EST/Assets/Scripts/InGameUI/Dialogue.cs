using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Dialogue : SingletonMonoBehaviour<Dialogue>
{
    public RectTransform dialogueBox;
    public TMP_Text dialogueText;
    private GameObject _textBox;
    Dictionary<int, string> textDict = new Dictionary<int, string>();
    Dictionary<int, int> nextDict = new Dictionary<int, int>();
    Dictionary<int, int> speakerDict = new Dictionary<int, int>();
    private Vector3 _offset;
    Transform currentTarget;
    private int _currentID;
    public int languageIndex = 5; // CSV파일에서 텍스트가 있는 열의 인덱스 5 : 한국어, 6: 영어, 7: 일본어
    public const int minLang = 5;
    public const int maxLang = 7;
    
    private UserInput _input;

    public bool IsPlay { get; private set; }
    public int CurrentID
    {
        set => _currentID = value;
    }

    protected override void Awake()
    {
        base.Awake();
        _input = new UserInput();
        LoadCSV();
        _offset = new Vector3(0f, 2f, 0f);
    }

    private void OnEnable()
    {
        _input.asset.Enable();
        _input.UI.NextDialogue.performed += PressKey;
    }

    private void OnDisable()
    {
        _input.UI.NextDialogue.performed -= PressKey;
        _input.asset.Disable();
    }

    public void CreateTextBox()
    {
        _textBox = Resources.Load<GameObject>("DialogueBox");
        _textBox = Instantiate(_textBox);
        Transform child = _textBox.transform.Find("Image");
        dialogueBox = child.GetComponent<RectTransform>();
        dialogueText = child.GetComponentInChildren<TMP_Text>();
        dialogueBox.gameObject.SetActive(false);
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

    public void PressKey(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            NextDialogue();
        }
    }

    void LoadCSV()
    {
        TextAsset csv = Resources.Load<TextAsset>("TableSheet"); //가져올 CSV 파일의 이름을 입력해주세요. 
        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split('\t');

            if (row.Length <= languageIndex) continue;


            string idStr = row[0];
            string nextStr = row[1];
            string speakerStr = row[3];


            if (!int.TryParse(idStr, out int id)) continue;
            if (!int.TryParse(nextStr, out int nextId)) continue;
            if (!int.TryParse(speakerStr, out int speaker)) continue;

            string text = row[languageIndex];

            textDict[id] = text;
            nextDict[id] = nextId;
            speakerDict[id] = speaker;
        }
    }

    void ShowDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
        
        if (!speakerDict.ContainsKey(_currentID)) return;

        int speaker = speakerDict[_currentID];
        
        UpdatePosition(GetTargetBySpeaker(speaker));
        
        if (!textDict.ContainsKey(_currentID)) return;

        dialogueText.text = textDict[_currentID];
        
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

    Transform GetTargetBySpeaker(int speaker)
    {
        Debug.Log("speaker: [" + speaker + "]");

        switch (speaker)
        {
            case 10101: //에스트
                return CutSceneManager.Instance.Player.transform;
            case 10201: //시드
                return CutSceneManager.Instance.Seed.transform;
            case 10301: //시드콩
                return CutSceneManager.Instance.Seed_B.transform;
            default:    //허공 대사
                return CutSceneManager.Instance.EmptyObject.transform;
        }
    }

    void UpdatePosition(Transform target)
    {
        if (target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + _offset);
        dialogueBox.position = screenPos;
    }

    public void SetLanguageIndex(int localeIndex)
    { 
        languageIndex = minLang + localeIndex; // CSV파일에서 텍스트가 있는 열의 인덱스 5 : 한국어, 6: 영어, 7: 일본어

        textDict.Clear();
        nextDict.Clear();
        speakerDict.Clear();

        LoadCSV(); // 바뀐 언어로 출력되어야 하기 때문에 CSV파일을 다시 불러옵니다.

        /*if (IsPlay)
        {
            ShowDialogue(); // 현재 대화가 진행 중이라면 언어 변경 후에도 대화가 계속 출력되어야 하기 때문에 ShowDialogue() 함수를 호출합니다.
        }*/
    }
}