using UnityEngine;

public class InvertColor : MonoBehaviour
{
    public Shader invertColorShader;

    void Start()
    {
        if (invertColorShader != null)
        {
            GetComponent<Renderer>().material.shader = invertColorShader;
        }
        else
        {
            Debug.LogError("Shader não atribuído!");
        }
    }

    void Update()
    {
        
    }
}
