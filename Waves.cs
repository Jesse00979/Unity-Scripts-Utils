using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Waves : MonoBehaviour
{
    private Mesh mesh;
    private float time;

    public float Itencidade = 0.5f;
    public float Velocidade = 1.0f;

    void Start()
    {
        time = 0;
        mesh = GetComponent<MeshFilter>().mesh;
        WavesMod();
    }

    void Update()
    {
        time += Time.deltaTime * Velocidade;
        WavesMod();
    }

    void WavesMod()
    {
        Vector3[] vertices = mesh.vertices;
        for (int i=0; i<vertices.Length; i++)
        {
            vertices[i].y = math.cos(math.sqrt( math.pow(vertices[i].x,2) + math.pow(vertices[i].z,2) ) - time) * Itencidade;
        }
        mesh.SetVertices(vertices);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
