using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageTarget : MonoBehaviour
{
    [HideInInspector]
    public string patternName = "default";
    [HideInInspector]
    public string destination = "Assets/AR.js-master/aframe/";

    private void OnValidate()
    {
        destination = "Assets/AR.js-master/aframe/" + SceneManager.GetActiveScene().name + "/";
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
            AssetDatabase.Refresh();
        }
    }

}
