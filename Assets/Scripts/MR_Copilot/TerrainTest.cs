// Plan:
// 1. Create a terrain game object with a terrain component
// 2. Set the terrain's texture to a gray color for the moon landscape
// 3. Modify the terrain heightmap to create hills
// 4. Set the terrain collider for proper physics interactions

using UnityEngine;

public class HillyMoonLandscape : MonoBehaviour
{
    private void Start()
    {
        // Create terrain
        GameObject terrainObject = new GameObject("HillyMoonLandscape");
        Terrain terrain = terrainObject.AddComponent<Terrain>();
        terrainObject.AddComponent<TerrainCollider>();

        // Set terrain texture
        Texture2D moonTexture = new Texture2D(1, 1);
        moonTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f));
        moonTexture.Apply();

        TerrainData terrainData = new TerrainData();
        terrainData.size = new Vector3(1000, 600, 1000);
        terrainData.heightmapResolution = 513;
        terrainData.baseMapResolution = 1024;
        terrainData.SetHeights(0, 0, GenerateHills(513, 513));

        terrainData.alphamapResolution = 512;
        terrainData.splatPrototypes = new SplatPrototype[] { new SplatPrototype { texture = moonTexture } };
        terrainData.RefreshPrototypes();

        terrain.terrainData = terrainData;
        terrain.GetComponent<TerrainCollider>().terrainData = terrainData;
    }

    private float[,] GenerateHills(int width, int height)
    {
        float[,] hills = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                hills[x, y] = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
            }
        }

        return hills;
    }
}