using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class BlackHole : MonoBehaviour
{
    [Header("Propiedades")]
    public Transform cameraTransform;
    public Shader blackHoleShader;
    public float raio = 1.0f;
    [Range(1, 10)] public float decaimento = 8.0f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    private Camera tempCamera;
    private Texture2D newTexture;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cria a malha
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
        tempCamera = CreateTemporaryCamera(60);

        // Atribui o shader
        if (blackHoleShader != null){
            MaterialSet();
        }else{
            Debug.LogError("Shader não atribuído!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!tempCamera.IsDestroyed()){
            LookAt();
            UpdateView();
        }
    }

    void Awake()
    {
        GetComponent<Renderer>().material.SetFloat("Decain", decaimento);
        Debug.Log("Test");
    }

    void OnDestroy()
    {
        Destroy(tempCamera.gameObject);
        if (newTexture != null)
            Destroy(newTexture);
    }

    void MaterialSet()
    {
        Material material = new Material(blackHoleShader);
        material.SetFloat("Decain", decaimento);
        material.SetFloat("_Mode", 2); // Modo transparente
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        GetComponent<Renderer>().material = material;
    }

    // Look at the Camera
    void LookAt()
    {
        if (cameraTransform != null)
        {
            // Calculate the direction to the target
            Vector3 direction = cameraTransform.position - transform.position;
            Vector3 cameraDirection = transform.position - cameraTransform.position;

            // Create a rotation that looks in the direction of the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion lookCamRotation = Quaternion.LookRotation(cameraDirection);
           
            // Apply the rotation to the object
            transform.rotation = lookRotation;
            tempCamera.transform.rotation = lookCamRotation;
            tempCamera.transform.position = transform.position;
        }
    }

    void CreateShape()
    {
        vertices = new Vector3[]
        {
            new Vector3(-0.5f * raio*2, -0.5f * raio*2, 0),
            new Vector3(-0.5f * raio*2,  0.5f * raio*2, 0),
            new Vector3( 0.5f * raio*2,  0.5f * raio*2, 0),
            new Vector3( 0.5f * raio*2, -0.5f * raio*2, 0)
        };

        uvs = new Vector2[]
        {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0)
        };

        triangles = new int[]
        {
            2, 1, 0,
            3, 2, 0
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void UpdateView()
    {
        Destroy(newTexture);
        newTexture = RTImage(tempCamera, (int)(100 * raio), (int)(100 * raio));
        GetComponent<Renderer>().material.mainTexture = newTexture;
    }

    // Creates a temporary camera gameobject.
	static Camera CreateTemporaryCamera(float fov, float near = .03f, float far = 1000)
	{
		GameObject cameraObject = new GameObject("__BlackHoleCamera");
		Camera camera = cameraObject.AddComponent<Camera>();
		camera.fieldOfView = fov;
		camera.nearClipPlane = near;
		camera.farClipPlane = far;
        camera.enabled = false;

		return camera;
	}

    private Texture2D RTImage(Camera tempCamera, int mWidth, int mHeight)
	{
		Rect rect = new Rect(0, 0, mWidth, mHeight);
		RenderTexture renderTexture = new RenderTexture(mWidth, mHeight, 24);
		Texture2D screenShot = new Texture2D(mWidth, mHeight, TextureFormat.RGBA32, false);

		tempCamera.targetTexture = renderTexture;
		tempCamera.Render();

		RenderTexture.active = renderTexture;
		screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

		tempCamera.targetTexture = null;
		RenderTexture.active = null;

		Destroy(renderTexture);
		renderTexture = null;
		return screenShot;
	}
}
