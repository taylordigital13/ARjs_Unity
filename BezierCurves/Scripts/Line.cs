using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour {

    public bool locked;
    public LineRenderer lr;
    public Transform p0;
    public Transform p1;
    public int layerOrder = 0;
    void Start() {
        lr.SetVertexCount(2);
        lr.sortingLayerID = layerOrder;
    }
    void Update() {
        lr.SetPosition(0,p0.position);
        lr.SetPosition(1,p1.position);
    }

    public void ManualUpdate()
    {
        if(p1 == null)
        {
            p1 = transform.GetChild(0);
        }
        lr.SetVertexCount(2);
        lr.sortingLayerID = layerOrder;
        lr.SetPosition(0, p0.position);
        lr.SetPosition(1, p1.position);
        MoveOtherLockedControlPoint();
    }

    public void MoveOtherLockedControlPoint()
    {
        Transform parent = transform.parent;
        if (locked && parent.childCount==2 && transform.name.Contains("-") && Selection.Contains(gameObject))
        {
            int siblingIndex = parent.GetChild(0) == transform ? 1 : 0;
            Transform sibling = parent.GetChild(siblingIndex);
            float sDist = Mathf.Sqrt(Mathf.Pow(sibling.position.x - parent.position.x, 2) + Mathf.Pow(sibling.position.y - parent.position.y, 2) + Mathf.Pow(sibling.position.z - parent.position.z, 2));
            sibling.gameObject.GetComponent<Line>().locked = true;

            Vector3 direction = parent.position - transform.position;
            float dist = Mathf.Sqrt(Mathf.Pow(transform.position.x - parent.position.x, 2) + Mathf.Pow(transform.position.y - parent.position.y, 2) + Mathf.Pow(transform.position.z - parent.position.z, 2));

            Vector3 newSibPos = parent.position + direction * sDist / dist;
            sibling.position = newSibPos;
        }
        else if(Selection.Contains(gameObject))
        {
            int siblingIndex = parent.GetChild(0) == transform ? 1 : 0;
            Transform sibling = parent.GetChild(siblingIndex);
            sibling.gameObject.GetComponent<Line>().locked = false;
        }
    }

    public void DeleteSelf()
    {
        int index = Convert.ToInt32(name.Remove(0, 1));
        Bezier bezier = transform.parent.gameObject.GetComponent<Bezier>();
        List<Transform> dontInclude = new List<Transform>();
        Transform destroyLater = null;
        if (index == 0)
        {
            if (transform.childCount == 2)
            {
                Transform p0_trans = transform;
                Transform p0cp1 = transform.Find("p0-cp1");
                Transform p0cp2 = transform.Find("p0-cp2");
                Transform p1_trans = bezier.controlPoints[3];
                Transform p1cp1 = bezier.controlPoints[2];
                Transform p1cp2 = bezier.controlPoints[4];

                bezier.controlPoints[0] = p1_trans;
                bezier.controlPoints[1] = p1cp2;
                bezier.controlPoints[2] = p0cp1;
                bezier.controlPoints[3] = p0_trans;
                bezier.controlPoints[4] = p0cp2;
                bezier.controlPoints[bezier.controlPoints.Length - 2] = p1cp1;
                bezier.controlPoints[bezier.controlPoints.Length - 1] = p1_trans;
            }
            else
            {
                Transform p0_trans = transform;
                Transform p0cp2 = transform.Find("p0-cp2");
                Transform p1_trans = bezier.controlPoints[3];
                Transform p1cp1 = bezier.controlPoints[2];
                Transform p1cp2 = bezier.controlPoints[4];

                bezier.controlPoints[0] = p1_trans;
                bezier.controlPoints[1] = p1cp2;
                bezier.controlPoints[2] = p1cp1;
                bezier.controlPoints[3] = p0_trans;
                bezier.controlPoints[4] = p0cp2;
                dontInclude.Add(p1cp1);
            }
        }
        if(bezier.controlPoints[bezier.controlPoints.Length-1].Equals(transform) && index!=0)
        {
            dontInclude.Add(bezier.controlPoints[bezier.controlPoints.Length-3]);
            destroyLater = bezier.controlPoints[bezier.controlPoints.Length - 3];
        }


        dontInclude.Add(transform);
        for(int i = 0; i<transform.childCount; i++)
        {
            dontInclude.Add(transform.GetChild(i));
        }
        Transform[] newPointsList = new Transform[bezier.controlPoints.Length - dontInclude.Count];

        int count = 0;
        for(int i = 0; i<bezier.controlPoints.Length; i++)
        {
            int bezIndex = dontInclude.FindIndex((obj) => obj == bezier.controlPoints[i]);
            if(bezIndex == -1)
            {
                newPointsList[count] = bezier.controlPoints[i];
                count++;
            }
        }
        for(int i = 0; i < newPointsList.Length; i++)
        {
            if (!newPointsList[i].name.Contains("-") && !(i!=0 && newPointsList[i].name=="p0"))
            {
                newPointsList[i].name = "p" + i/3;
                for (int j = 0; j < newPointsList[i].childCount; j++)
                {
                    string[] nameStrings = newPointsList[i].GetChild(j).name.Split('-');
                    newPointsList[i].GetChild(j).name = "p" + i/3 + "-" + nameStrings[1];
                }
            }
        }
        bezier.controlPoints = newPointsList;
        if(index ==0 && transform.childCount == 1)
        {
            DestroyImmediate(bezier.controlPoints[0].Find("p0-cp1").gameObject, true);
        }
        if (destroyLater != null) DestroyImmediate(destroyLater.gameObject, true);
        Selection.activeGameObject = bezier.gameObject;
        DestroyImmediate(gameObject, true);
    }

    public void InsertAfter()
    {
        int index = Convert.ToInt32(name.Remove(0, 1));
        Bezier bezier = transform.parent.gameObject.GetComponent<Bezier>();
    }

    public void InsertBefore()
    {
        int index = Convert.ToInt32(name.Remove(0, 1));
        Bezier bezier = transform.parent.gameObject.GetComponent<Bezier>();
    }
}
