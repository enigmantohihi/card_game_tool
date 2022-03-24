using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelect : MonoBehaviour
{
    public int type;
    public string path_deck;
    public string folder_name;

    [SerializeField] Text text_Name;
    Manager manager;

    public void Set(string path) 
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        path_deck = path;
        folder_name = Path.GetFileName(path_deck);
        text_Name.text = folder_name;
    }

    public void GetPath()
    {
        if (type == 0) manager.SetPath(path_deck);
        else manager.SetPath_Txt(path_deck);
    }
}
