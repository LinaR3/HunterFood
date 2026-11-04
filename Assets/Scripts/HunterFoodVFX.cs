using TMPro;
using UnityEngine;

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
