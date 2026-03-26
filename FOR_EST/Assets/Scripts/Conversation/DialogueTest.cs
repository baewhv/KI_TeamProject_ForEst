using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DialogueTest : MonoBehaviour
{
    public TMP_Text dialogueText;

    Dictionary<int, string> textDict = new Dictionary<int, string>();
    Dictionary<int, int> nextDict = new Dictionary<int, int>();
    Dictionary<int, string> speakerDict = new Dictionary<int, string>();
    public RectTransform dialogueBox;
    public Vector3 offset;
    Transform currentTarget;
    int currentID;

    void Start()
    {
        LoadCSV();
        currentID = 100001; // CSV파일의 시작할 텍스트 ID를 입력해주세요.
        ShowDialogue();
    }

    void Update()
    {
        if (currentTarget == null)
        {
            Debug.Log("타겟 없음");
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

            string text = row[5];

            textDict[id] = text;
            nextDict[id] = nextId;
            speakerDict[id] = speaker;
        }
    }
    void ShowDialogue()
    {
        if (!textDict.ContainsKey(currentID)) return;

        dialogueText.text = textDict[currentID];

        if (!speakerDict.ContainsKey(currentID)) return;

        string speaker = speakerDict[currentID];

        currentTarget = GetTargetBySpeaker(speaker);
    }

    void NextDialogue()
    {
        if (!nextDict.ContainsKey(currentID)) return;

        int nextID = nextDict[currentID];

        if (nextID == 0)
        {
            dialogueText.text = "끝!";
            return;
        }

        currentID = nextID;
        ShowDialogue();
    }
    Transform GetTargetBySpeaker(string speaker)
    {
        //Debug.Log("speaker: [" + speaker + "]");

        if (speaker == "에스트")
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");

            if (go == null)
                Debug.Log("Player 태그 못 찾음");

            return go != null ? go.transform : null;
        }
        else if (speaker == "시드")
        {
            GameObject go = GameObject.FindGameObjectWithTag("Seed");

            if (go == null)
                Debug.Log("seed 태그 못 찾음");

            return go != null ? go.transform : null;
        }
        return null;
    }

    void UpdatePosition(Transform target)
    { 
        if(target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        dialogueBox.position = screenPos;
    }
}