using System;
using LibNoise;
using LibNoise.Modifiers;

namespace vMap.Voronoi
{
	public class NoiseGenerator
	{
		public static double[,] GenerateNoise(NoiseType noiseType, int width, int height, double frequency, NoiseQuality quality, int seed, int octaves, double lacunarity, double persistence, float scale)
		{
			var scaledWidth = (int)(width * scale);
			var scaledHeight = (int)(height * scale);
			IModule noiseModule;
			var noiseMap = new double[scaledWidth, scaledHeight];

			switch (noiseType)
			{
				case NoiseType.Billow:
					noiseModule = GetBillowNoiseModule(frequency, quality, seed, octaves, lacunarity, persistence);
					break;
				case NoiseType.Combined:
					noiseModule = GetCombinedNoiseModule(frequency, quality, seed, octaves, lacunarity, persistence);
					break;
				case NoiseType.Perlin:
					noiseModule = GetPerlinNoiseModule(frequency, quality, seed, octaves, lacunarity, persistence);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(noiseType), noiseType, null);
			}

			// generate noise 
			if ((noiseMap.GetLength(0) != scaledWidth) || (noiseMap.GetLength(1) != scaledHeight))
				noiseMap = new double[scaledWidth, scaledHeight];

			for (var y = 0; y < scaledHeight; y++)
			{
				for (var x = 0; x < scaledWidth; x++)
				{
					noiseMap[x, y] = (noiseModule.GetValue(x, y, 0) + 1) / 2.0F;
					if (noiseMap[x, y] < 0) noiseMap[x, y] = 0;
					if (noiseMap[x, y] > 1.0F) noiseMap[x, y] = 1.0F;
					noiseMap[x, y] = (byte)(noiseMap[x, y] * 255.0);
				}
			}
			return noiseMap;
		}
		private static IModule GetBillowNoiseModule(double frequency, NoiseQuality quality, int seed, int octaves, double lacunarity, double persistence)
		{
			return new Billow
			{
				Frequency = frequency,
				NoiseQuality = quality,
				Seed = seed,
				OctaveCount = octaves,
				Lacunarity = lacunarity,
				Persistence = persistence
			};
		}
		private static IModule GetCombinedNoiseModule(double frequency, NoiseQuality quality, int seed, int octaves, double lacunarity, double persistence)
		{
			var billow = new Billow
			{
				Frequency = frequency,
				NoiseQuality = quality,
				Seed = seed,
				OctaveCount = octaves,
				Lacunarity = lacunarity,
				Persistence = persistence
			};
			var scaledBillow = new ScaleBiasOutput(billow)
			{
				Bias = -0.75,
				Scale = 0.125
			};

			// Ridged Multi-Fractal
			var ridgedMultiFractal = new RidgedMultifractal()
			{
				Frequency = frequency / 2.0,
				NoiseQuality = quality,
				Seed = seed,
				OctaveCount = octaves,
				Lacunarity = lacunarity
			};

			// Perlin
			var perlin = new Perlin
			{
				Seed = seed,
				Frequency = frequency / 10.0,
				Lacunarity = lacunarity,
				Persistence = persistence,
				OctaveCount = octaves,
				NoiseQuality = quality
			};

			// selector
			var selector = new Select(perlin, ridgedMultiFractal, scaledBillow);
			selector.SetBounds(0, 1000);
			selector.EdgeFalloff = 0.5;

			// Final combined noise
			return selector;
		}
		private static IModule GetPerlinNoiseModule(double frequency, NoiseQuality quality, int seed, int octaves, double lacunarity, double persistence)
		{
			return new Perlin
			{
				Seed = seed,
				Frequency = frequency,
				Lacunarity = lacunarity,
				Persistence = persistence,
				OctaveCount = octaves,
				NoiseQuality = quality
			};
		}
	}
}