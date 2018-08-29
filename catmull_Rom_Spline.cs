using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catmull_Rom_Spline : MonoBehaviour {
    public Transform path;
    Transform[] pathPoint;
    int index = 1;
    float step = 30.0f;
    int interval = 0;
    void Start () {
        path = GameObject.Find("path").GetComponent<Transform>();
        pathPoint = path.GetComponentsInChildren<Transform> ();
    }

    void Update () {

        interval++;
        if (interval >= step) {
            interval = 0;
            // index++;
        }
        // if (index >= pathPoint.Length-1) {
        //     index = 1;
        // }

        transform.position = catmull_Rom_Spline (pathPoint[index].position, pathPoint[index + 1].position, pathPoint[index + 2].position, pathPoint[index + 3].position, interval / step);

    }

    Vector3 catmull_Rom_Spline (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) //t=0时返回p1,t=1时返回p2
    {
        Vector3 a = 2 * p1;
	Vector3 b = p2 - p0;
	Vector3 c = 2 * p0 - 5 * p1 + 4 * p2 - p3;
	Vector3 d = -p0 + 3 * p1 - 3 * p2 + p3;
	Vector3 ret = (a + (b * t) + (c * t * t) + (d * t * t * t)) / 2;

	return ret;
    }
}
