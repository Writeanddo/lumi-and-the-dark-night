using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerDialog : MonoBehaviour
{
    public string DialogText;

    public bool spawnDialogPoint;
    public float spawnVerticalOffset = 0;

    public PlayerUIScript playerUI;
    public GameObject dialogPointPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (spawnDialogPoint)
            {
                GameObject dp = Instantiate(dialogPointPrefab, transform.position + spawnVerticalOffset * Vector3.up, transform.rotation);
                dp.GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
                dp.GetComponentInChildren<Text>().fontSize = 20;
                DialoguePointScript script = dp.GetComponent<DialoguePointScript>();
                script.text[0] = DialogText;
                //script.ShowText(DialogText, 3f);
                Destroy(dp, script.textDuration+2f*script.fadeTime);
                Destroy(gameObject);
            }
            else
            {
                playerUI.ShowDialogText(DialogText);
                Destroy(gameObject);
            }
        }
    }
}
