using UnityEngine;

public class FoxController : MonoBehaviour
{
    private Vector3 travelDirection;
    private float speed;
    private float exitX;
    private Rigidbody foxBody;

    void Awake()
    {
        foxBody = GetComponent<Rigidbody>();
        if (foxBody == null)
        {
            foxBody = gameObject.AddComponent<Rigidbody>();
        }

        // El zorro se mueve por código; el Rigidbody cinemático permite que sus
        // triggers se detecten de forma fiable contra las manzanas dinámicas.
        foxBody.isKinematic = true;
        foxBody.useGravity = false;
        foxBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        foxBody.interpolation = RigidbodyInterpolation.Interpolate;
        AddCollisionCollider();
    }

    public void Initialize(Vector3 direction, float movementSpeed, float horizontalLimit)
    {
        travelDirection = direction.normalized;
        speed = movementSpeed;
        exitX = travelDirection.x > 0f ? horizontalLimit : -horizontalLimit;

        Animator foxAnimator = GetComponent<Animator>();
        if (foxAnimator != null)
        {
            foxAnimator.applyRootMotion = false;
        }

        // El modelo mira hacia su eje Z local; lo orientamos hacia el lado que va a cruzar.
        transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);
    }

    void FixedUpdate()
    {
        Vector3 nextPosition = foxBody.position + travelDirection * (speed * Time.fixedDeltaTime);
        foxBody.MovePosition(nextPosition);

        if ((travelDirection.x > 0f && nextPosition.x >= exitX) ||
            (travelDirection.x < 0f && nextPosition.x <= exitX))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // La manzana lleva MoveFoward en el mismo objeto que su Rigidbody.
        Rigidbody appleBody = other.attachedRigidbody;
        if (appleBody != null && appleBody.GetComponent<MoveFoward>() != null)
        {
            Destroy(appleBody.gameObject);
        }
    }

    void AddCollisionCollider()
    {
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider != null)
        {
            existingCollider.isTrigger = true;
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("El prefab del zorro no tiene Renderer para calcular su collider.", this);
            return;
        }

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int corner = 0; corner < 8; corner++)
        {
            Vector3 worldCorner = new Vector3(
                (corner & 1) == 0 ? worldBounds.min.x : worldBounds.max.x,
                (corner & 2) == 0 ? worldBounds.min.y : worldBounds.max.y,
                (corner & 4) == 0 ? worldBounds.min.z : worldBounds.max.z);
            Vector3 localCorner = transform.InverseTransformPoint(worldCorner);
            min = Vector3.Min(min, localCorner);
            max = Vector3.Max(max, localCorner);
        }

        BoxCollider foxCollider = gameObject.AddComponent<BoxCollider>();
        foxCollider.center = (min + max) * 0.5f;
        foxCollider.size = max - min;
        foxCollider.isTrigger = true;
    }
}
