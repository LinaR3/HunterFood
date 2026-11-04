using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Small low-poly style visual feedback used by Hunter Food gameplay.</summary>
public static class HunterFoodVFX
{
    private static Material particleMaterial;
    private static ParticleSystem ambientLeaves;
    private static Mesh leafMesh;

    public static void InitializeAmbient(bool enabled)
    {
        if (ambientLeaves == null)
            ambientLeaves = CreateAmbientLeaves();
        SetAmbientEnabled(enabled);
    }

    public static void SetAmbientEnabled(bool enabled)
    {
        if (ambientLeaves == null)
        {
            if (!enabled) return;
            ambientLeaves = CreateAmbientLeaves();
        }

        if (enabled && !ambientLeaves.isPlaying)
        {
            ambientLeaves.Play();
            ambientLeaves.Emit(14);
        }
        else if (!enabled && ambientLeaves.isPlaying)
        {
            ambientLeaves.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public static void AppleImpact(Vector3 position)
    {
        Burst(position, new Color(1f, 0.76f, 0.12f, 1f), 7, 0.12f, 1.3f, 0.48f);
    }

    public static void AnimalSatisfied(Vector3 position)
    {
        ShowHeart(position + Vector3.up * 1.4f);
        Burst(position + Vector3.up * 0.6f, new Color(1f, 0.9f, 0.24f, 1f), 14, 0.16f, 1.8f, 0.72f);
    }

    public static void LevelCompleted(Vector3 position)
    {
        Burst(position, new Color(1f, 0.78f, 0.08f, 1f), 36, 0.22f, 2.2f, 1.8f);
        Burst(position + Vector3.up * 0.35f, new Color(1f, 0.96f, 0.72f, 1f), 20, 0.14f, 1.6f, 1.5f);
    }

    public static void FoxDust(Vector3 position)
    {
        Burst(position, new Color(0.55f, 0.48f, 0.34f, 0.55f), 4, 0.2f, 0.35f, 0.55f);
    }

    public static void FoxWarning(Vector3 position)
    {
        Color warning = new Color(1f, 0.16f, 0.08f, 1f);
        ShowText(position, "!", warning, 0.65f);
        Burst(position, warning, 6, 0.12f, 0.7f, 0.42f);
    }

    public static void AppleStolen(Vector3 position)
    {
        Burst(position, new Color(0.12f, 0.09f, 0.065f, 0.9f), 11, 0.18f, 1.1f, 0.56f);

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        CameraShakeEffect shake = mainCamera.GetComponent<CameraShakeEffect>();
        if (shake == null) shake = mainCamera.gameObject.AddComponent<CameraShakeEffect>();
        shake.Trigger(0.12f, 0.095f);
    }

    public static void FoodCollected(Vector3 worldPosition, TMP_Text counter)
    {
        if (counter == null) return;

        Canvas canvas = counter.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        RectTransform canvasRect = canvas.rootCanvas.transform as RectTransform;
        if (canvasRect == null) return;

        Camera uiCamera = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.rootCanvas.worldCamera;
        Camera mainCamera = Camera.main;
        Vector2 sourceScreen = mainCamera != null
            ? mainCamera.WorldToScreenPoint(worldPosition)
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, counter.rectTransform.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sourceScreen, uiCamera, out Vector2 sourceLocal) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, targetScreen, uiCamera, out Vector2 targetLocal))
            return;

        for (int i = 0; i < 3; i++)
        {
            GameObject particle = new GameObject("Food Counter Spark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FoodHudParticle));
            particle.transform.SetParent(canvasRect, false);
            RectTransform rect = particle.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(13f, 13f);
            rect.anchoredPosition = sourceLocal + Random.insideUnitCircle * 24f;
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            particle.GetComponent<Image>().color = new Color(1f, 0.78f, 0.02f, 1f);
            particle.GetComponent<Image>().raycastTarget = false;
            particle.GetComponent<FoodHudParticle>().Initialize(targetLocal, Random.Range(0.48f, 0.72f));
        }
    }

    public static void Burst(Vector3 position, Color color, int count, float size, float speed, float lifetime)
    {
        GameObject effect = new GameObject("Hunter Food Particle Burst");
        effect.transform.position = position;

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = lifetime;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.65f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.55f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.65f, size);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        main.maxParticles = Mathf.Max(1, count);
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;

        ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(color.a, 0.15f),
                new GradientAlphaKey(0f, 1f)
            });
        fade.color = gradient;

        ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetParticleMaterial();

        particles.Emit(count);
        particles.Play();
    }

    private static ParticleSystem CreateAmbientLeaves()
    {
        GameObject ambientObject = new GameObject("Ambient Low Poly Leaves");
        ambientObject.transform.position = new Vector3(0f, 0.9f, 7.5f);

        ParticleSystem particles = ambientObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = 12f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.28f);
        main.startColor = new Color(0.92f, 0.88f, 0.61f, 0.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 32;
        main.useUnscaledTime = true;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 1.5f;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(38f, 0.7f, 34f);
        shape.randomDirectionAmount = 0f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(0.05f, 0.22f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        ParticleSystem.RotationOverLifetimeModule rotation = particles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-0.55f, 0.55f);

        ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.9f, 0.86f, 0.62f, 1f), 0f),
                new GradientColorKey(new Color(0.76f, 0.82f, 0.5f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.52f, 0.16f),
                new GradientAlphaKey(0f, 1f)
            });
        fade.color = gradient;

        ParticleSystemRenderer renderer = ambientObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.mesh = GetLeafMesh();
        renderer.material = GetParticleMaterial();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return particles;
    }

    private static Mesh GetLeafMesh()
    {
        if (leafMesh != null) return leafMesh;

        leafMesh = new Mesh { name = "Low Poly Leaf Particle Mesh" };
        leafMesh.vertices = new[]
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(0.15f, 0.28f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.15f, -0.28f, 0f)
        };
        leafMesh.uv = new[] { new Vector2(0f, 0.5f), new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f) };
        leafMesh.triangles = new[] { 0, 1, 3, 1, 2, 3, 3, 1, 0, 3, 2, 1 };
        leafMesh.RecalculateNormals();
        leafMesh.RecalculateBounds();
        return leafMesh;
    }

    private static void ShowHeart(Vector3 position)
    {
        ShowText(position, "♥", new Color(1f, 0.82f, 0.16f, 1f), 0.8f);
    }

    private static void ShowText(Vector3 position, string value, Color color, float lifetime)
    {
        GameObject heartObject = new GameObject("Satisfied Heart Reaction");
        heartObject.transform.position = position;

        TextMeshPro heart = heartObject.AddComponent<TextMeshPro>();
        heart.text = value;
        heart.fontSize = value == "!" ? 4f : 5f;
        heart.color = color;
        heart.alignment = TextAlignmentOptions.Center;
        heart.outlineColor = Color.black;
        heart.outlineWidth = 0.12f;
        heart.raycastTarget = false;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            heartObject.transform.rotation = mainCamera.transform.rotation;

        Object.Destroy(heartObject, lifetime);
    }

    private static Material GetParticleMaterial()
    {
        if (particleMaterial != null) return particleMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        particleMaterial = new Material(shader) { name = "Hunter Food VFX Material" };
        return particleMaterial;
    }
}

public sealed class FoodHudParticle : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 start;
    private Vector2 destination;
    private float duration;
    private float elapsed;

    public void Initialize(Vector2 target, float travelDuration)
    {
        rect = GetComponent<RectTransform>();
        start = rect.anchoredPosition;
        destination = target;
        duration = Mathf.Max(0.1f, travelDuration);
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float eased = t * t * (3f - 2f * t);
        rect.anchoredPosition = Vector2.LerpUnclamped(start, destination, eased);
        rect.localScale = Vector3.one * (1f - t * 0.65f);
        if (t >= 1f) Destroy(gameObject);
    }
}

public sealed class CameraShakeEffect : MonoBehaviour
{
    private Vector3 originalLocalPosition;
    private float duration;
    private float remaining;
    private float strength;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    public void Trigger(float shakeDuration, float shakeStrength)
    {
        duration = Mathf.Max(duration, shakeDuration);
        remaining = Mathf.Max(remaining, shakeDuration);
        strength = Mathf.Max(strength, shakeStrength);
    }

    private void LateUpdate()
    {
        if (remaining <= 0f) return;
        remaining -= Time.unscaledDeltaTime;
        float fade = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
        transform.localPosition = originalLocalPosition + Random.insideUnitSphere * (strength * fade);
        if (remaining <= 0f)
        {
            transform.localPosition = originalLocalPosition;
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        transform.localPosition = originalLocalPosition;
    }
}
