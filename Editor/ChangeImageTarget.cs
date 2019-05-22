using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ChangeImageTarget : ScriptableWizard
{
    [Header("Image Target Settings")]
    public string patternLocation = "changeBackToDefaultHiro";
    public Texture2D spriteImage;

    [MenuItem("AR.js/ChangeImageTarget", false, 2)]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ChangeImageTarget>("Change Target Image", "Update", "Select .patt File");
    }

    void OnWizardCreate()
    {
        GameObject imageTarget = GameObject.FindGameObjectWithTag("ImageTarget");
        if (patternLocation == "changeBackToDefaultHiro")
        {
            byte[] hiroBytes = File.ReadAllBytes("Assets/ARjs_Unity/Icons/HIRO.jpg");
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(hiroBytes);

            imageTarget.GetComponent<MeshRenderer>().material.mainTexture = tex;
            imageTarget.GetComponent<ImageTarget>().patternName = "default";
            return;
        }
        if (patternLocation == "" || !patternLocation.Contains(".patt"))
        {
            Debug.LogError("You have to choose a .patt file.");
            Debug.LogError("If you don't have one, go here to generate a .patt file and image:\nhttps://jeromeetienne.github.io/AR.js/three.js/examples/marker-training/examples/generator.html");
            return;
        }
        if(spriteImage == null)
        {
            Debug.LogError("You have to add a Sprite.");
            return;
        }

        imageTarget.GetComponent<ImageTarget>().patternName = patternLocation.Split('/')[patternLocation.Split('/').Length-1];
        imageTarget.GetComponent<MeshRenderer>().material.mainTexture = RemoveWhiteBorder(spriteImage);
        AssetDatabase.Refresh();
    }

    void OnWizardOtherButton()
    {
        patternLocation = EditorUtility.OpenFilePanel("Choose .patt file as pattern", "", "patt");
        string destination = "Assets/AR.js-master/aframe/UnityExamples/" + patternLocation.Split('/')[patternLocation.Split('/').Length-1];
        File.Copy(patternLocation, destination, true);
    }

    void OnWizardUpdate()
    {
        //Updates the buttons when you change the size of the array.
        helpString = "Enter canvas details";
    }

    Texture2D RemoveWhiteBorder(Texture2D texture)
    {
        // /Users/LWeldon/WebGLAR/Assets/AR.js-master/aframe/UnityExamples/pattern-taylor.patt
        int x = 0;
        while (((texture.GetPixel(x,x).r+texture.GetPixel(x,x).g + texture.GetPixel(x,x).b)/3).Equals(1))
        {
            x++;

            if (x >= texture.width) return texture;
        }
        int cropWidth = texture.width - 2 * x;
        Texture2D returnTexture = new Texture2D(cropWidth, cropWidth);
        for (int i = 0; i<cropWidth; i++)
        {
            for(int j = 0; j<cropWidth; j++)
            {
                Color pixelColor = texture.GetPixel(i + x, j + x);
                returnTexture.SetPixel(i, j, pixelColor);
            }
        }
        returnTexture.Apply(true);
        return returnTexture;
    }

}
