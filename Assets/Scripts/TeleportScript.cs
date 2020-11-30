using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{

    private Transform destination;
    public string TagList = "|Player|MovableObject|";

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.Find("Destination");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (TagList.Contains(string.Format("|{0}|",other.tag)))
        {
            if (destination != null)
            {
                other.transform.position = destination.transform.position;
            }

        }
    }
}