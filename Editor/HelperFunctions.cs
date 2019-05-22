using UnityEditor;

public static class HelperFunctions
{
    //Adds a tag to the list of available tags if it doesn't already exist. This prevents an error from creating 
    //an object like a cube that's supposed to have the cube tag, but the cube tag isn't in the list of available tags yet.
    public static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Adding a Tag
        string s = tag;

        // First check if it is not already present
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { found = true; break; }
        }

        // if not found, add it
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = s;
        }

        // Setting a Layer (Let's set Layer 10)
        string layerName = "the_name_want_to_give_it";

        SerializedProperty sp = tagManager.FindProperty("User Layer 10");
        if (sp != null) sp.stringValue = layerName;

        tagManager.ApplyModifiedProperties();
    }
}
