using Godot;
using System;

[Tool]
public static class Noise
{
    private static float Scale;
    private static float Frequency;
    private static float Amplitude;
    private static float Lacunarity;
    private static float Persistence;
    private static int Layers;

    private static FastNoiseLite Perlin;
    private static float Width;
    private static float Depth;

    public static bool IsInitialized { get; private set; } = false;

    public static void Initialize(int layers, float scale, float width, float depth, float frequency=1f, float amplitude=1f, float lacunarity=0.2f, float persistence=2f)
    {
        Scale = scale;
        Frequency = frequency;
        Amplitude = amplitude;
        Lacunarity = lacunarity;
        Persistence = persistence;
        Layers = layers;
        Depth = depth;
        Width = width;

        if (Scale <= 0 || Scale==Frequency)
            Scale = 0.0001f;

        Perlin = new FastNoiseLite();
        Perlin.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        IsInitialized = true;
    }

    public static float GetNoise2D(float x, float z)
    {
        float y = 0f;
        x /= Width;
        z /= Depth;
        for(int layer=0; layer<Layers; layer++)
        {
            float sampleX = x/Scale*(Frequency * Mathf.Pow(Lacunarity, layer+1));
            float sampleZ = z/Scale*(Frequency * Mathf.Pow(Lacunarity, layer+1));

            y += Perlin.GetNoise2D(sampleX, sampleZ)*(Amplitude * Mathf.Pow(Persistence, layer+1));
        }

        return y;
    }
}
