using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// this is a static class storing some useful mathmatic function,
/// attention,It's work by extending the defult Tool class like Mathf
/// </summary>
public class KMath {

    private static Vector3 e1, e2, p, q, t;
    private static float det, u, v, distance;
    //http://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    /// <summary>
    /// 检测一条射线是否穿过一个三角形，a，b，c为三角形三个顶点，如果穿过，返回交点及法线
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="hitPoint"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    public static bool rayIntersectsTriangle (ref Ray ray, ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 hitPoint, ref Vector3 normal) {
        e1 = b - a;
        e2 = c - a;
        p = Vector3.Cross (ray.direction, e2);
        det = Vector3.Dot (e1, p);
        if (det < Mathf.Epsilon) return false;
        q = ray.origin - a;
        u = Vector3.Dot (q, p);
        if (u < 0 || u > det) return false;
        t = Vector3.Cross (q, e1);
        v = Vector3.Dot (ray.direction, t);
        if (v < 0 || u + v > det) return false;
        distance = Vector3.Dot (e2, t) * (1 / det);
        if (distance > Mathf.Epsilon) {
            hitPoint = ray.GetPoint (distance);
            normal = Vector3.Cross (e1, e2);
            return true;
        }

        return false;
    }
    public static Ray InverseTransformRay (Transform transform, Ray InWorldRay) {
        Vector3 o = InWorldRay.origin;
        o -= transform.position;
        o = transform.worldToLocalMatrix * o;
        Vector3 d = transform.worldToLocalMatrix.MultiplyVector (InWorldRay.direction);
        return new Ray (o, d);
    }

}