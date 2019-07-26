using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;

[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    private string pathToUse = "Assets/ARjs_Unity/Prefabs/";

    [HideInInspector]
    public Transform[] controlPoints;
    [HideInInspector]
    public LineRenderer lineRenderer;

    public float speed = 25;
    
    private int curveCount = 0;	
    private int layerOrder = 0;
    private int SEGMENT_COUNT = 50;
    private Transform parent;
    private Transform imageTarget;
    private float startTime;
    
		
    void Start()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int)(controlPoints.Length - 1)/ 3;

        parent = transform.parent;
        imageTarget = parent.parent;
        transform.SetParent(imageTarget);

        startTime = Time.time;
    }

    void Update()
    {
       
        DrawCurve();
        FollowCurve();

    }

    public void UpdateManually()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int)(controlPoints.Length - 1)/ 3;
        DrawCurve();
    }

    public void AddPoint(bool loopAdd, GameObject firstPoint)
    {
        GameObject controlPoint1_temp = AssetDatabase.LoadAssetAtPath(pathToUse + "BezierControlPoint.prefab", typeof(GameObject)) as GameObject;
        GameObject controlPoint2_temp = AssetDatabase.LoadAssetAtPath(pathToUse + "BezierControlPoint.prefab", typeof(GameObject)) as GameObject;
        GameObject point_temp = AssetDatabase.LoadAssetAtPath(pathToUse + "BezierPoint.prefab", typeof(GameObject)) as GameObject;


        GameObject lastPoint = controlPoints[controlPoints.Length - 1].gameObject;

        GameObject controlPoint1 = Instantiate(controlPoint1_temp, new Vector3(lastPoint.transform.position.x, lastPoint.transform.position.y + 1, lastPoint.transform.position.z), 
                    Quaternion.identity, lastPoint.transform) as GameObject;
        controlPoint1.name = "p" + (transform.childCount-1) + "-cp2";


        GameObject newPoint = loopAdd ? firstPoint : Instantiate(point_temp, new Vector3(lastPoint.transform.position.x, lastPoint.transform.position.y + 3, lastPoint.transform.position.z), Quaternion.identity, transform) as GameObject;
        if(!loopAdd) newPoint.name = "p" + (transform.childCount - 1);

        GameObject controlPoint2 = Instantiate(controlPoint2_temp, new Vector3(newPoint.transform.position.x, newPoint.transform.position.y - 1, newPoint.transform.position.z),
                    Quaternion.identity, newPoint.transform) as GameObject;
        controlPoint2.name = "p" + (loopAdd ? 0 : (transform.childCount - 1)) + "-cp1";

        Transform[] newPointsList = new Transform[controlPoints.Length+3];
        int index = 0;
        for(int i = 0; i<controlPoints.Length; i++)
        {
            newPointsList[i] = controlPoints[i];
            index = i;
        }
        newPointsList[index + 1] = controlPoint1.transform;
        newPointsList[index + 2] = controlPoint2.transform;
        newPointsList[index + 3] = newPoint.transform;

        Line cp1Line = controlPoint1.GetComponent<Line>();
        cp1Line.lr = cp1Line.GetComponent<LineRenderer>();
        cp1Line.p0 = lastPoint.transform;
        cp1Line.p1 = cp1Line.transform;

        Line cp2Line = controlPoint2.GetComponent<Line>();
        cp2Line.lr = cp2Line.GetComponent<LineRenderer>();
        cp2Line.p0 = newPoint.transform;
        cp2Line.p1 = cp2Line.transform;


        if (!loopAdd)
        {
            Line newPointLine = newPoint.GetComponent<Line>();
            newPointLine.lr = newPoint.GetComponent<LineRenderer>();
            newPointLine.p0 = newPoint.transform;
            newPointLine.p1 = cp2Line.transform;
        }

        controlPoints = newPointsList;

    }

    public void LoopUnloop()
    {
        GameObject lastPoint = transform.GetChild(transform.childCount-1).gameObject;
        GameObject firstPoint = controlPoints[0].gameObject;

        if (controlPoints.Length == transform.childCount * 3 - 2) 
        {
            AddPoint(true, firstPoint);
        }
        else
        {
            Transform[] newPointsList = new Transform[controlPoints.Length - 3];
            int index = 0;
            for (int i = 0; i < newPointsList.Length; i++)
            {
                newPointsList[i] = controlPoints[i];
                index = i;
            }
            controlPoints = newPointsList;
            DestroyImmediate(firstPoint.transform.Find("p0-cp1").gameObject, true);
            DestroyImmediate(lastPoint.transform.Find("p" + (transform.childCount - 1) + "-cp2").gameObject, true);

        }
    }

    public void HideShow()
    {
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr.enabled)
        {
            lr.enabled = false;
            for(int i=0;i<transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            lr.enabled = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void PrintPointsList()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("var pointsArray = [");
        for (int i=0; i<controlPoints.Length; i++)
        {
            Transform cp = controlPoints[i];
            sb.Append("new THREE.Vector3(" + (-cp.position.x/10) + "," + cp.position.y/10 + "," + cp.position.z/10 + "),");
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append("];");
        Debug.Log(sb.ToString());
    }

    public string PointsListString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Transform cp = controlPoints[i];
            sb.Append("new THREE.Vector3(" + (-cp.position.x / 10) + "," + cp.position.y / 10 + "," + cp.position.z / 10 + "),");
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append("]");

        return sb.ToString();
    }

    void DrawCurve()
    {
        for (int j = 0; j <curveCount; j++)
        {
            for (int i = 1; i <= SEGMENT_COUNT; i++)
            {
                float t = i / (float)SEGMENT_COUNT;
                int nodeIndex = j * 3;
                if (nodeIndex +3 >= controlPoints.Length)
                {
                    break;
                }

                Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints [nodeIndex].position, controlPoints [nodeIndex + 1].position, controlPoints [nodeIndex + 2].position, controlPoints [nodeIndex + 3].position);
                lineRenderer.positionCount = (((j * SEGMENT_COUNT) + i));
                lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
            }
            
        }
    }

    void FollowCurve()
    {
        float currentTime = Time.time-startTime;
        if((int)(currentTime*speed) >= lineRenderer.positionCount)
        {
            startTime = Time.time;
            return;
        }

        Vector3 position = lineRenderer.GetPosition((int)(currentTime*speed));
        parent.position = position;
    }

    public Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
		
        Vector3 p = uuu * p0; 
        p += 3 * uu * t * p1; 
        p += 3 * u * tt * p2; 
        p += ttt * p3; 
		
        return p;
    }
}
