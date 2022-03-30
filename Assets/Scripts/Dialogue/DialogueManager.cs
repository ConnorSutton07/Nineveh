using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;
    private Vector3 open;
    private Vector3 close;

    private Queue<string> message;

    void Start()
    {
        open = new Vector3(0f, -Screen.height / 2, 0f);
        close = new Vector3(0f, Screen.height * 3, 0f);
        message = new Queue<string>();
    }
    public void StartDialogue (Dialogue dialogue)
    {

        transform.localPosition = open;
        nameText.text = dialogue.name;
        message.Clear();
        foreach(string sentence in dialogue.messages)
        {
            message.Enqueue(sentence);
        }
        DisplayNextMessage();
    }

    public void DisplayNextMessage()
    {
        if(message.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = message.Dequeue();
        dialogueText.text = sentence;
    }

    void EndDialogue()
    {
        transform.position = close;
    }
}
