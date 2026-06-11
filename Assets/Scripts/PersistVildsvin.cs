using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistVildsvin : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Awake()
{
    transform.SetParent(null);
    transform.rotation = Quaternion.identity;
    DontDestroyOnLoad(gameObject);
    SceneManager.sceneLoaded += OnSceneLoaded;
}
public Material bronzeMaterial;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "EndScene")
    {
        transform.position = new Vector3(-75.011f, 0, 175.8f);
        transform.eulerAngles = new Vector3(0f, 0, 0);
        Transform vildsvinRoot = transform.Find("VildsvinRoot");
if (vildsvinRoot != null)
{
    vildsvinRoot.localRotation = Quaternion.identity;
}
        Debug.Log("Rotation sat til: " + transform.rotation.eulerAngles);
        transform.localScale = new Vector3(20.9f, 5.712463f, 5.712463f);

        // Skift materiale på alle celler
        Transform vildsvin1 = transform.Find("VildsvinRoot/Vildsvin.1");
        if (vildsvin1 != null)
        {
            foreach (Transform celle in vildsvin1)
            {
                Renderer r = celle.GetComponent<Renderer>();
                if (r != null) r.material = bronzeMaterial;
            }
        }

        // Destroy eksisterende rotator og lav ny så den starter fra nul
        EndRotator existingRotator = gameObject.GetComponent<EndRotator>();
        if (existingRotator != null) Destroy(existingRotator);
        
        EndRotator rotator = gameObject.AddComponent<EndRotator>();
        rotator.rotationSpeed = rotationSpeed;
    }
}

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}