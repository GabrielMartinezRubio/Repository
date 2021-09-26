using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGeneration : MonoBehaviour
{
	[Header("Altura y Anchura del Mapa")]
    public int width;
    public int height;

	[Header("Generación de mapa a traves de la seed y el random Seed")]
	public string seed;
	public bool useRandomSeed;
	
	[Header("Otras cosas")]
    [Range(0,100)]public int randomFillPercent;
    int[,] map;

	void Start()
	{
		GenerateMap();
	}
	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			GenerateMap();
		}
	}
	void GenerateMap()
	{
		map = new int[width, height];
		RandomFillMap();

		for (int i = 0; i < 5; i++)
		{
			SmoothMap();
		}
		int borderSize = 1;
		int[,] borderMap = new int[width + borderSize * 2, height + borderSize * 2];
		for (int x = 0; x < borderMap.GetLength(0); x++)
		{
			for (int y = 0; y < borderMap.GetLength(1); y++)
			{
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
				{
					borderMap[x, y] = map[x - borderSize, y - borderSize];
				}
				else
				{
					borderMap[x, y] = 1;
				}
			}
		}
				MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
		meshGenerator.GenerateMesh(borderMap, 1);
	}
	void RandomFillMap()
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString();
			//print(seed);
		}
		System.Random psuedoRandom = new System.Random(seed.GetHashCode());
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					map[x, y] = 1;
				}
				else
				{
					map[x, y] = (psuedoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
				}
			}
		}
	}
	void SmoothMap()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				int neighbourWallTiles = GetSorroundingWallCount(x, y);
				if (neighbourWallTiles > 4)
				{
					map[x, y] = 1;
				}
				else if (neighbourWallTiles < 4)
				{
					map[x, y] = 0;
				}
			}
		}
	}
	int GetSorroundingWallCount(int gridX, int gridY)
	{
		//Este metodo lo hago para que se genere un espacio en entre el mapa y la pared
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += map[neighbourX, neighbourY];
					}
				}
				else
				{
					wallCount++;
				}
			}
		}
		return wallCount;
	}
	void OnDrawGizmos()
	{
		//Para poder representar el mapa de ruido por colores antes de generar la maya.
		if (map != null)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
					Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
					Gizmos.DrawCube(pos, Vector3.one);
				}
			}
		}
	}
}
