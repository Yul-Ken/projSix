using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KMesh {
    Mesh mesh;
    public string name = "";
    public List<Vector3> vertices = null;
    public int[] triangles = null;
    public List<Vector4> uv0 = null;
    public List<Vector4> uv1 = null;
    public List<Vector4> uv2 = null;
    public List<Vector4> uv3 = null;

    public KMesh (Mesh m) {
        this.name = m.name;
        vertices = new List<Vector3>();
        uv0 = new List<Vector4>();
        m.GetVertices (this.vertices);
        this.triangles = m.GetTriangles (0); //未考虑其他拓扑
        m.GetUVs (0, uv0);
    }

    public void ApplyTo (Mesh m) {
        m.SetVertices (this.vertices);
        m.SetTriangles (this.triangles.ToArray (), 0); //未考虑其他拓扑
        m.SetUVs (0, this.uv0);
        // m.SetUVs (1, this.uv1);
        // m.SetUVs (2, this.uv2);
        // m.SetUVs (3, this.uv3);
        m.RecalculateBounds ();
        m.RecalculateNormals ();
        m.RecalculateTangents ();
    }

    // public int vertexCount { get { return vertices != null ? vertices.Count : 0; } }

    public List<Vector4> GetUVs (int index) {
        if (index == 0) return uv0;
        else if (index == 1) return uv1;
        else if (index == 2) return uv2;
        else if (index == 3) return uv3;
        return null;
    }

    public void SetUVs (int index, List<Vector4> uvs) {
        if (index == 0) uv0 = uvs;
        else if (index == 1) uv1 = uvs;
        else if (index == 2) uv2 = uvs;
        else if (index == 3) uv3 = uvs;
    }

    public void Clear () {
        vertices = null;
        triangles = null;
        uv0 = null;
        uv1 = null;
        uv2 = null;
        uv3 = null;
    }

    public int[] GetTriangles () {
        if (triangles == null) Debug.LogFormat ("no mesh verts here!");
        return triangles;
    }

    public void SetTriangles (int[] triangles, int index) {

    }

    /**
     * Apply the vertex attributes to a UnityEngine mesh (does not set triangles)
     */
    //     public void ApplyAttributesToUnityMesh (Mesh m, z_MeshChannel attrib = z_MeshChannel.All) {
    //         // I guess the default value for attrib makes the compiler think that else is never
    //         // activated?
    // #pragma warning disable 0162
    //         if (attrib == z_MeshChannel.All) {
    //             m.vertices = vertices;
    //             m.normals = normals;
    //             m.colors32 = colors;
    //             m.tangents = tangents;

    //             m.SetUVs (0, uv0);
    //             m.SetUVs (1, uv1);
    //             m.SetUVs (2, uv2);
    //             m.SetUVs (3, uv3);
    //         } else {
    //             if ((attrib & z_MeshChannel.Position) > 0) m.vertices = vertices;
    //             if ((attrib & z_MeshChannel.Normal) > 0) m.normals = normals;
    //             if ((attrib & z_MeshChannel.Color) > 0) m.colors32 = colors;
    //             if ((attrib & z_MeshChannel.Tangent) > 0) m.tangents = tangents;
    //             if ((attrib & z_MeshChannel.UV0) > 0) m.SetUVs (0, uv0);
    //             if ((attrib & z_MeshChannel.UV2) > 0) m.SetUVs (1, uv1);
    //             if ((attrib & z_MeshChannel.UV3) > 0) m.SetUVs (2, uv2);
    //             if ((attrib & z_MeshChannel.UV4) > 0) m.SetUVs (3, uv3);
    //         }

    //     }
}