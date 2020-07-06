using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AddCustomModel : ScriptableWizard
{
    [Header("Model Files")]
    public Object objFile;
    public Object mtlFile;
    public Texture2D texture;

    [MenuItem("GameObject/AR.js/Custom Model", false, 15)]
    static void CreateWizard()
    {
        //ScriptableWizard.DisplayWizard<ChangeImageTarget>("Change Target Image", "Update", "Select .patt File");
        DisplayWizard<AddCustomModel>("Add Custom Model", "Place Model");
    }

    void OnWizardCreate()
    {
        GameObject imageTarget = GameObject.FindGameObjectWithTag("ImageTarget");


        if (objFile == null)
        {
            Debug.LogError("You need to at least have an object file to place in scene.");
            return;
        }

        string objName = AssetDatabase.GetAssetPath(objFile).Split('/')[AssetDatabase.GetAssetPath(objFile).Split('/').Length - 1];
        if (!objName.ToLower().Contains(".obj"))
        {
            Debug.LogError("Only .obj files are supported at this time.");
            return;
        }

        if (!Directory.Exists(GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination + "models/")) Directory.CreateDirectory(GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination + "models/");
        string objDest = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination + "models/" + objName;

        File.Copy(AssetDatabase.GetAssetPath(objFile), objDest, true);

        GameObject model = objFile as GameObject;
        GameObject sceneModel = Instantiate(model, Vector3.zero, Quaternion.identity, imageTarget.transform) as GameObject;
        sceneModel.AddComponent<CustomModelHelper>();
        Selection.activeObject = sceneModel;
        HelperFunctions.AddTag("Model");
        sceneModel.transform.tag = "Model";
        sceneModel.name = "Model";

        CustomModelHelper modelHelper = sceneModel.GetComponent<CustomModelHelper>();
        modelHelper.objName = objName;
        if (mtlFile!=null)
        {
            string mtlName = AssetDatabase.GetAssetPath(mtlFile).Split('/')[AssetDatabase.GetAssetPath(mtlFile).Split('/').Length - 1];
            string mtlDest = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination + "models/" + mtlName;

            modelHelper.mtlName = mtlName;

            File.Copy(AssetDatabase.GetAssetPath(mtlFile), mtlDest, true);
        }

        if (texture != null)
        {
            string texName = AssetDatabase.GetAssetPath(texture).Split('/')[AssetDatabase.GetAssetPath(texture).Split('/').Length - 1];
            string texDest = GameObject.FindWithTag("ImageTarget").GetComponent<ImageTarget>().destination + "models/" + texName;

            modelHelper.texName = texName;

            File.Copy(AssetDatabase.GetAssetPath(texture), texDest, true);
        }



        AssetDatabase.Refresh();
    }

    void OnWizardUpdate()
    {
        helpString = "The texture is optional, though maybe required by your .mtl file.";
    }
}
