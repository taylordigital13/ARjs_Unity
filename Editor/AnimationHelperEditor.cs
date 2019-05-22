using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationHelper))]
public class AnimationHelperEditor : Editor
{

    //public override void OnInspectorGUI()
    //{
    //    DrawDefaultInspector();

    //    AnimationHelper myScript = (AnimationHelper)target;
    //    if (GUILayout.Button("Record Point"))
    //    {
    //        myScript.RecordPoint();
    //    }

    //    if (GUILayout.Button("Clear Points"))
    //    {
    //        myScript.ClearCurrentPoints();
    //    }

    //    if (GUILayout.Button("Export Points"))
    //    {
    //        myScript.ExportPoints();
    //    }

    //    if (GUILayout.Button("KeyList Info"))
    //    {
    //        myScript.KeyListInfo();
    //    }
    //}
}
