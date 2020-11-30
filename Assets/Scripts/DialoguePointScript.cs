using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePointScript : MonoBehaviour
{
    public enum Type
    {
        ONCE,       // say lines in sequence, only once
        CYCLE,      // say lines in sequence, repeat
        RANDOM,     // say lines chosen randomly
        PROPERTY    // query player for property value
    }

    public Type type;
    public string property;
    public string[] text;
    public float textDuration = 3f;
    public float fadeTime = 0.5f;

    private Text textComponent;
    private int textIndex = 0;
    private float textAlpha = 0;
    private float busyTimer = 0;
    private float fadeDelta = 0;


    void Start()
    {
        textComponent = GetComponentInChildren<Text>();
        textAlpha = textComponent.color.a;
        textComponent.enabled = false;
    }

    // This function can be called directly when controlled by event sequencer
    public void ShowText(string s, float duration)
    {
        if (s != "")
        {
            busyTimer = duration + 2*fadeTime;
            textComponent.enabled = true;
            textComponent.text = s;
            Color color = textComponent.color;
            fadeDelta = textAlpha / (fadeTime/Time.deltaTime);
            color.a = 0;
            textComponent.color = color;
        }
    }

    public void ShowText(string s)
    {
        // use default duration
        ShowText(s, textDuration);
    }

    void Update()
    {
        busyTimer -= Time.deltaTime;
        Color color = textComponent.color;

        if (busyTimer > 0)
        {
            if (busyTimer > fadeTime)
            {
                // fade in or hold
                if (color.a < textAlpha)
                {
                    color.a += fadeDelta;
                    if (color.a > 1) color.a = 1;
                    textComponent.color = color;
                }
            }
            else
            {
                // fade out
                color.a -= fadeDelta;
                if (color.a <= 0) color.a = 0;
                textComponent.color = color;
            }
        }
        else
        {
            textComponent.enabled = false;
        }
    }

    // COLLISION TRIGGERED SUPPORT

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            int value = 0;
            if (type == Type.PROPERTY)
            {
                PlayerController playerController = collider.gameObject.GetComponent<PlayerController>();
                value = playerController.GetProperty(property);
                //Debug.Log("Property value=" + value);
            }
            Trigger(value);
        }
    }

    public void Trigger(int propertyValue = 0)
    {
        // do not trigger if already busy
        if (busyTimer > 0) return;

        // determine which text to display
        string s = "";
        if (type == Type.ONCE)
        {
            if (textIndex < text.Length)
            {
                s = text[textIndex];
                textIndex++;
            }
        }
        else if (type == Type.CYCLE)
        {
            if (textIndex < text.Length)
            {
                s = text[textIndex];
                textIndex++;
                if (textIndex >= text.Length) textIndex = 0;
            }            
        }
        else if (type == Type.RANDOM)
        {
            textIndex = Random.Range(0,text.Length);
            if (textIndex < text.Length)
            {
                s = text[textIndex];
            }
        }
        else if (type == Type.PROPERTY)
        {
            if (propertyValue < text.Length)
            {
                s = text[propertyValue];
            }
        }

        ShowText(s);
    }
}
