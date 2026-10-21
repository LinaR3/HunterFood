using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] animalPrefabs;
    [Header("Zona de spawn")]
    [SerializeField] private float spawnRangeX = 20f;
    [SerializeField] private float spawnRangeZ = 20f;
    public float startDelay = 2;
    public float spawnInterval = 1.5f;
    public float animalSpeed = 5f;
    public int applesToSatisfyAnimal = 3;
    [Header("Zorro")]
    public GameObject foxPrefab;
    public float foxStartDelay = 5f;
    public float foxSpawnInterval = 3.333333f;
    public float foxSpeed = 5f;
    public float foxHorizontalLimit = 20f;
    public float foxMinZ = -10f;
    public float foxMaxZ = 25f;

    private bool warnedAboutMissingAnimals;

    void Start()
    {
        UpdateSpawnSchedule();
    }

    public void UpdateSpawnSchedule()
    {
        CancelInvoke(nameof(SpawnRandomAnimal));
        CancelInvoke(nameof(SpawnFox));
        InvokeRepeating(nameof(SpawnRandomAnimal), startDelay, Mathf.Max(0.1f, spawnInterval));
        InvokeRepeating(nameof(SpawnFox), foxStartDelay, Mathf.Max(0.1f, foxSpawnInterval));
    }

    void SpawnRandomAnimal()
    {
        if (animalPrefabs == null || animalPrefabs.Length == 0)
        {
            WarnMissingAnimals();
            return;
        }

        int animalIndex = Random.Range(0, animalPrefabs.Length);
        for (int i = 0; i < animalPrefabs.Length && animalPrefabs[animalIndex] == null; i++)
            animalIndex = (animalIndex + 1) % animalPrefabs.Length;
        if (animalPrefabs[animalIndex] == null)
        {
            WarnMissingAnimals();
            return;
        }

        Vector3 spawnPosition = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, spawnRangeZ);
        GameObject animal = Instantiate(animalPrefabs[animalIndex], spawnPosition, animalPrefabs[animalIndex].transform.rotation);
        ConfigureAnimalCollision(animal);

        MoveFoward animalMovement = animal.GetComponent<MoveFoward>();
        if (animalMovement != null)
        {
            animalMovement.speed = animalSpeed;
        }

        AnimalHunger animalHunger = animal.GetComponent<AnimalHunger>();
        if (animalHunger == null)
        {
            animalHunger = animal.AddComponent<AnimalHunger>();
        }

        animalHunger.Initialize(applesToSatisfyAnimal);
    }

    private void WarnMissingAnimals()
    {
        if (warnedAboutMissingAnimals) return;
        warnedAboutMissingAnimals = true;
        Debug.LogWarning("SpawnManager no puede crear animales: asigna al menos un prefab válido en Animal Prefabs.", this);
    }

    void ConfigureAnimalCollision(GameObject animal)
    {
        Renderer[] renderers = animal.GetComponentsInChildren<Renderer>();
        bool foundRenderer = false;
        Bounds worldBounds = new Bounds();

        foreach (Renderer animalRenderer in renderers)
        {
            if (!animalRenderer.enabled || !animalRenderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!foundRenderer)
            {
                worldBounds = animalRenderer.bounds;
                foundRenderer = true;
            }
            else
            {
                worldBounds.Encapsulate(animalRenderer.bounds);
            }
        }

        if (!foundRenderer)
        {
            return;
        }

        // Los bounds de los prefabs originales son de tamaño 1 aunque el modelo
        // usa escala 0.1. Convertimos el volumen visible a espacio local del root.
        Vector3 localMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 localMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int corner = 0; corner < 8; corner++)
        {
            Vector3 worldCorner = new Vector3(
                (corner & 1) == 0 ? worldBounds.min.x : worldBounds.max.x,
                (corner & 2) == 0 ? worldBounds.min.y : worldBounds.max.y,
                (corner & 4) == 0 ? worldBounds.min.z : worldBounds.max.z);
            Vector3 localCorner = animal.transform.InverseTransformPoint(worldCorner);
            localMin = Vector3.Min(localMin, localCorner);
            localMax = Vector3.Max(localMax, localCorner);
        }

        BoxCollider animalCollider = animal.GetComponent<BoxCollider>();
        if (animalCollider == null)
        {
            animalCollider = animal.AddComponent<BoxCollider>();
        }

        animalCollider.center = (localMin + localMax) * 0.5f;
        animalCollider.size = (localMax - localMin) * 1.25f;
        animalCollider.isTrigger = true;
    }

    void SpawnFox()
    {
        // El intervalo es tres veces menor para aumentar el ritmo de aparición.
        // Se permiten varios zorros en pantalla al mismo tiempo.
        if (foxPrefab == null)
        {
            return;
        }

        float horizontalDirection = Random.value < 0.5f ? 1f : -1f;
        float spawnX = horizontalDirection > 0f ? -foxHorizontalLimit - 2f : foxHorizontalLimit + 2f;

        float horizontalDistance = foxHorizontalLimit * 2f + 2f;
        float slopeMagnitude = Mathf.Min(0.35f, (foxMaxZ - foxMinZ) / horizontalDistance);
        float zSlope = Random.value < 0.5f ? slopeMagnitude : -slopeMagnitude;
        float zTravel = horizontalDistance * Mathf.Abs(zSlope);
        float spawnZ = zSlope > 0f
            ? Random.Range(foxMinZ, foxMaxZ - zTravel)
            : Random.Range(foxMinZ + zTravel, foxMaxZ);
        Vector3 spawnPosition = new Vector3(spawnX, 0f, spawnZ);

        GameObject fox = Instantiate(foxPrefab, spawnPosition, Quaternion.identity);
        FoxController foxController = fox.GetComponent<FoxController>();
        if (foxController == null)
        {
            foxController = fox.AddComponent<FoxController>();
        }

        foxController.Initialize(new Vector3(horizontalDirection, 0f, zSlope), foxSpeed, foxHorizontalLimit);
    }
}
