using UnityEngine;
using System.IO;

/// <summary>
/// Standalone MonoBehaviour to generate white tile template sprites.
/// Attach to any GameObject and run in Play mode to save sprites to disk.
/// </summary>
public class PrintWhiteTileSprite : MonoBehaviour {
    [Header("Tile Settings")]
    [SerializeField] private int tileTextureSize = 256;
    [SerializeField] private int sideTextureWidth = 256;
    [SerializeField] private int sideTextureHeight = 128;
    [SerializeField] private int borderWidth = 2;
    
    [Header("Colors")]
    [SerializeField] private Color fillColor = Color.white;
    [SerializeField] private float borderDarken = 0.4f;
    [SerializeField] private float leftSideDarken = 0.30f;
    [SerializeField] private float rightSideDarken = 0.15f;
    
    [Header("Output")]
    [SerializeField] private string outputFolder = "Assets/GeneratedSprites";
    [SerializeField] private bool generateOnStart = true;

    void Start() {
        if (generateOnStart) {
            GenerateAllSprites();
        }
    }

    [ContextMenu("Generate All Sprites")]
    public void GenerateAllSprites() {
        // Ensure output folder exists
        if (!Directory.Exists(outputFolder)) {
            Directory.CreateDirectory(outputFolder);
        }

        // Generate top tile
        var topTexture = CreateSquareTileTexture(fillColor);
        SaveTexture(topTexture, "TileTop_White.png");
        Debug.Log($"Saved: {outputFolder}/TileTop_White.png");

        // Generate left side
        var leftColor = Darken(fillColor, leftSideDarken);
        var leftTexture = CreateSideTexture(leftColor);
        SaveTexture(leftTexture, "TileSideLeft_White.png");
        Debug.Log($"Saved: {outputFolder}/TileSideLeft_White.png");

        // Generate right side
        var rightColor = Darken(fillColor, rightSideDarken);
        var rightTexture = CreateSideTexture(rightColor);
        SaveTexture(rightTexture, "TileSideRight_White.png");
        Debug.Log($"Saved: {outputFolder}/TileSideRight_White.png");

        Debug.Log("All tile sprites generated!");
    }

    private Texture2D CreateSquareTileTexture(Color fill) {
        int size = tileTextureSize;
        var texture = new Texture2D(size, size, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Point
        };

        Color border = Darken(fill, borderDarken);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                bool isBorder = x < borderWidth || x >= size - borderWidth ||
                                y < borderWidth || y >= size - borderWidth;
                pixels[y * size + x] = isBorder ? border : fill;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private Texture2D CreateSideTexture(Color baseColor) {
        int width = sideTextureWidth;
        int height = sideTextureHeight;
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
            filterMode = FilterMode.Point
        };

        Color darkColor = Darken(baseColor, 0.2f);
        Color borderColor = Darken(baseColor, 0.5f);
        var pixels = new Color[width * height];
        int border = 1;

        for (int y = 0; y < height; y++) {
            float t = (float)y / height;
            Color rowColor = Color.Lerp(darkColor, baseColor, t);

            for (int x = 0; x < width; x++) {
                bool isBorder = x < border || x >= width - border ||
                                y < border || y >= height - border;
                pixels[y * width + x] = isBorder ? borderColor : rowColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private Color Darken(Color color, float amount) {
        return new Color(
            color.r * (1f - amount),
            color.g * (1f - amount),
            color.b * (1f - amount),
            color.a
        );
    }

    private void SaveTexture(Texture2D texture, string filename) {
        byte[] pngData = texture.EncodeToPNG();
        string path = Path.Combine(outputFolder, filename);
        File.WriteAllBytes(path, pngData);
        
        // Cleanup
        DestroyImmediate(texture);
    }
}