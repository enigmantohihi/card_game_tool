using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public bool on;
    Manager manager;

    private void Start()
    {
        manager = GetComponent<Manager>();
    }

    void Update()
    {
        if (!on) return;

        if (Input.GetKeyDown(KeyCode.P)) manager.ChangeDrawPos(1);
        else if (Input.GetKeyDown(KeyCode.Z)) manager.Zoom_Card();
        else if (Input.GetKeyDown(KeyCode.D)) manager.Draw_Top(0);
        else if (Input.GetKeyDown(KeyCode.R)) manager.Draw_Top(1);
    }
}
