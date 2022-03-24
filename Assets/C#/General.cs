using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class General : MonoBehaviour
{
    public static Vector2 GetPngSizeFromPath(string path)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Seek(16, SeekOrigin.Begin);
        byte[] buf = new byte[8];
        fs.Read(buf, 0, 8);
        fs.Dispose();
        uint width = ((uint)buf[0] << 24) | ((uint)buf[1] << 16) | ((uint)buf[2] << 8) | (uint)buf[3];
        uint height = ((uint)buf[4] << 24) | ((uint)buf[5] << 16) | ((uint)buf[6] << 8) | (uint)buf[7];
        Debug.Log("width = " + width + ", height = " + height);
        return new Vector2(width, height);
    }
}
