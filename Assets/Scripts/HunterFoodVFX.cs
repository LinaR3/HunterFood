using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Small low-poly style visual feedback used by Hunter Food gameplay.</summary>
public static class HunterFoodVFX
{
    private static Material particleMaterial;

    public static void AppleImpact(Vector3 position)
    {
        Burst(position, new Color(1f, 0.76f, 0.12f, 1f), 7, 0.12f, 1.3f, 0.48f);
    }

    public static void AnimalSatisfied(Vector3 position)
    {
        ShowHeart(position + Vector3.up * 1.4f);
        Burst(position + Vector3.up * 0.6f, new Color(1f, 0.9f, 0.24f, 1f), 14, 0.16f, 1.8f, 0.72f);
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

    private static void ShowHeart(Vector3 position)
    {
        GameObject heartObject = new GameObject("Satisfied Heart Reaction");
        heartObject.transform.position = position;

        TextMeshPro heart = heartObject.AddComponent<TextMeshPro>();
        heart.text = "♥";
        heart.fontSize = 5f;
        heart.color = new Color(1f, 0.82f, 0.16f, 1f);
        heart.alignment = TextAlignmentOptions.Center;
        heart.outlineColor = Color.black;
        heart.outlineWidth = 0.12f;
        heart.raycastTarget = false;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            heartObject.transform.rotation = mainCamera.transform.rotation;

        Object.Destroy(heartObject, 0.8f);
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
