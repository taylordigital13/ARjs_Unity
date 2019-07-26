using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Bezier))]
public class CustomBezierEditor : Editor
{
    public Bezier bezier;
    public GameObject[] lineObjects;

    private void OnEnable()
    {
        bezier = (Bezier)target;
        GameObject obj = bezier.gameObject;
        lineObjects = GameObject.FindGameObjectsWithTag("BezierLine");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        bezier = (Bezier)target;
        GameObject obj = bezier.gameObject;
        lineObjects = GameObject.FindGameObjectsWithTag("BezierLine");

        bezier.UpdateManually();
        foreach (GameObject lineThing in lineObjects)
        {
            lineThing.GetComponent<Line>().ManualUpdate();
        }

        if (GUILayout.Button("New Point", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            bezier.AddPoint(false, null);
        }
        if (GUILayout.Button("Loop/Unloop Path", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            bezier.LoopUnloop();
        }
        if (GUILayout.Button("Show/Hide", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        {
            bezier.HideShow();
        }
        //if (GUILayout.Button("PrintPoints", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
        //{
        //    bezier.PrintPointsList();
        //}
    }
}
