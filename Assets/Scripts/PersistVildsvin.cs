using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistVildsvin : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Awake()
{
    transform.SetParent(null);
    DontDestroyOnLoad(gameObject);
    SceneManager.sceneLoaded += OnSceneLoaded;
}
public Material bronzeMaterial;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "EndScene")
    {
        transform.position = new Vector3(-75.011f, 0, 175.8f);
        transform.rotation = Quaternion.Euler(18.422f, 0, 0);
        transform.localScale = new Vector3(20.9f, 5.712463f, 5.712463f);

        // Skift materiale på alle celler
        Material bronzeMat = Resources.Load<Material>("Bronze 1");
        Transform vildsvin1 = transform.Find("VildsvinRoot/Vildsvin.1");
        if (vildsvin1 != null)
        {
            foreach (Transform celle in vildsvin1)
            {
                Renderer r = celle.GetComponent<Renderer>();
                if (r != null) r.material = bronzeMat;
            }
        }

        EndRotator rotator = gameObject.GetComponent<EndRotator>();
        if (rotator == null)
        {
            rotator = gameObject.AddComponent<EndRotator>();
            rotator.rotationSpeed = rotationSpeed;
        }
    }
}

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}