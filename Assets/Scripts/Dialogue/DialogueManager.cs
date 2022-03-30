using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;
    private Vector3 open;
    private Vector3 close;

    private Queue<string> messages;
    private Queue<Sprite> sprites;
    Vector3 prevPos;
    bool active;

    Image character;

    void Start()
    {
        close = new Vector3(0f, Screen.height * 3, 0f);
        prevPos = transform.position;
        transform.position = close;
        messages = new Queue<string>();
        sprites = new Queue<Sprite>();
        character = transform.Find("Character").GetComponent<Image>();

    }
    public void StartDialogue (Dialogue dialogue)
    {
        transform.position = prevPos;
        active = true;
        foreach (var message in dialogue.messages) { messages.Enqueue(message); }
        foreach (var sprite in dialogue.sprites) { sprites.Enqueue(sprite); }
        DisplayNextMessage();
    }

    void OnAdvanceDialogue(InputValue value)
    {
        Debug.Log("here");
        if (active) { DisplayNextMessage(); }
    }

    public void DisplayNextMessage()
    {
        if (messages.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = messages.Dequeue();
        Sprite sprite = sprites.Dequeue();
        dialogueText.text = sentence;
        nameText.text = sprite.name;
        character.sprite = sprite;
    }

    void EndDialogue()
    {
        active = false;
        transform.position = close;
    }
}
