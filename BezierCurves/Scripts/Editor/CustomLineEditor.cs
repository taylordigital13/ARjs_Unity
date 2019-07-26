using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(Line))]
public class CustomLineEditor : Editor
{
    public Bezier bezier;
    public GameObject[] lineObjects;

    private void OnEnable()
    {
        bezier = GameObject.FindWithTag("Bezier").GetComponent<Bezier>();
        lineObjects = GameObject.FindGameObjectsWithTag("BezierLine");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        bezier = GameObject.FindWithTag("Bezier").GetComponent<Bezier>();
        GameObject[] bezierObjects = GameObject.FindGameObjectsWithTag("Bezier");
        lineObjects = GameObject.FindGameObjectsWithTag("BezierLine");
        foreach (GameObject bezObj in bezierObjects)
        {
            bezier = bezObj.GetComponent<Bezier>();
            bezier.UpdateManually();
        }

        foreach(GameObject lineThing in lineObjects)
        {
            lineThing.GetComponent<Line>().ManualUpdate();
        }

        if (!target.name.Contains("-"))
        {
            if (GUILayout.Button("Delete Point", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
            {
                Line line = (Line)target;
                line.DeleteSelf();
            }
        }
    }
}
