using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction moveAction;
    public Vector2 moveInput;
    public float speed = 10.0f;
    public float xRange = 10.0f;
    public float minZ = -10.0f;
    public float maxZ = 30.0f;
    public float screenEdgePadding = 1.5f;
    public GameObject projectilePrefab;
    public InputAction fireAction;

    private Camera gameCamera;
    private Animator playerAnimator;
    private static readonly int ThrowStateHash = Animator.StringToHash("Movement.GrenadeThrow");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction.Enable();
        fireAction.Enable();
        gameCamera = Camera.main;
        playerAnimator = GetComponentInChildren<Animator>();
    }

    void OnDisable()
    {
        moveAction.Disable();
        fireAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        Vector3 position = transform.position;
        float nextX = position.x + moveInput.x * speed * Time.deltaTime;
        float nextZ = position.z + moveInput.y * speed * Time.deltaTime;

        if (TryGetVisibleGroundBounds(position.y, out Vector3[] groundCorners, out float groundMinZ, out float groundMaxZ))
        {
            float minVisibleZ = groundMinZ + screenEdgePadding;
            float maxVisibleZ = groundMaxZ - screenEdgePadding;
            nextZ = minVisibleZ <= maxVisibleZ
                ? Mathf.Clamp(nextZ, minVisibleZ, maxVisibleZ)
                : (groundMinZ + groundMaxZ) * 0.5f;

            if (TryGetVisibleXBoundsAtZ(groundCorners, nextZ, out float minVisibleX, out float maxVisibleX))
            {
                float minX = minVisibleX + screenEdgePadding;
                float maxX = maxVisibleX - screenEdgePadding;
                nextX = minX <= maxX
                    ? Mathf.Clamp(nextX, minX, maxX)
                    : (minVisibleX + maxVisibleX) * 0.5f;
            }
            else
            {
                nextX = Mathf.Clamp(nextX, -xRange, xRange);
            }
        }
        else
        {
            // Valores de respaldo si la escena no tiene una cámara MainCamera.
            nextX = Mathf.Clamp(nextX, -xRange, xRange);
            nextZ = Mathf.Clamp(nextZ, minZ, maxZ);
        }

        position.x = nextX;
        position.z = nextZ;
        transform.position = position;

        if (fireAction.triggered)
        {
            if (projectilePrefab == null) return;
            PlayAppleThrowAnimation();
            Debug.Log("Fired an Apple!");
            Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        }
    }

    void PlayAppleThrowAnimation()
    {
        if (playerAnimator != null && playerAnimator.HasState(0, ThrowStateHash))
        {
            playerAnimator.CrossFade(ThrowStateHash, 0.08f, 0, 0f);
        }
    }

    bool TryGetVisibleGroundBounds(float groundY, out Vector3[] corners, out float minZ, out float maxZ)
    {
        corners = null;
        minZ = float.PositiveInfinity;
        maxZ = float.NegativeInfinity;

        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera == null)
        {
            return false;
        }

        Vector2[] viewportCorners =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };
        corners = new Vector3[viewportCorners.Length];

        for (int i = 0; i < viewportCorners.Length; i++)
        {
            Ray ray = gameCamera.ViewportPointToRay(viewportCorners[i]);
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
            {
                return false;
            }

            float distance = (groundY - ray.origin.y) / ray.direction.y;
            if (distance < 0f)
            {
                return false;
            }

            corners[i] = ray.GetPoint(distance);
            minZ = Mathf.Min(minZ, corners[i].z);
            maxZ = Mathf.Max(maxZ, corners[i].z);
        }

        return true;
    }

    bool TryGetVisibleXBoundsAtZ(Vector3[] corners, float z, out float minX, out float maxX)
    {
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        int intersections = 0;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % corners.Length];
            float segmentMinZ = Mathf.Min(start.z, end.z);
            float segmentMaxZ = Mathf.Max(start.z, end.z);

            if (z < segmentMinZ - 0.0001f || z > segmentMaxZ + 0.0001f)
            {
                continue;
            }

            if (Mathf.Abs(end.z - start.z) < 0.0001f)
            {
                minX = Mathf.Min(minX, Mathf.Min(start.x, end.x));
                maxX = Mathf.Max(maxX, Mathf.Max(start.x, end.x));
                intersections += 2;
                continue;
            }

            float t = (z - start.z) / (end.z - start.z);
            float x = Mathf.Lerp(start.x, end.x, t);
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            intersections++;
        }

        return intersections >= 2;
    }
}
