using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureColorRemover : EditorWindow
{
    private Texture2D sourceTexture;
    private Color removeColor = Color.magenta; // The color to remove
    private float colorTolerance = 0.1f; // Tolerance for aggressive removal
    private bool flipHorizontal = false;
    private bool flipVertical = false;
    private bool convertToSprite = true; // New option to convert to sprite

    [MenuItem("Tools/AdvancedToolkit/Remove Color")]
    public static void ShowWindow()
    {
        GetWindow<TextureColorRemover>("Color Remover");
    }

    void OnGUI()
    {
        // Title Header
        GUILayout.Label("Background Remover", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        removeColor = EditorGUILayout.ColorField("Remove Color", removeColor);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);

        // Display Read/Write status and enable button
        if (sourceTexture != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null)
            {
                bool isReadable = textureImporter.isReadable;
                EditorGUILayout.LabelField("Read/Write Enabled:", isReadable.ToString());

                if (!isReadable)
                {
                    if (GUILayout.Button("Enable Read/Write"))
                    {
                        textureImporter.isReadable = true;
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        // Repaint to update the status label immediately
                        this.Repaint(); 
                    }
                    EditorGUILayout.HelpBox("Read/Write must be enabled to process the texture.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Could not get texture import settings. Texture might not be an asset.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Transform Options", EditorStyles.boldLabel);
        flipHorizontal = EditorGUILayout.Toggle("Flip Horizontal", flipHorizontal);
        flipVertical = EditorGUILayout.Toggle("Flip Vertical", flipVertical);

        EditorGUILayout.Space();
        GUILayout.Label("Output Options", EditorStyles.boldLabel);
        convertToSprite = EditorGUILayout.Toggle("Convert to Sprite", convertToSprite);

        EditorGUILayout.Space();

        // Standard processing button
        if (GUILayout.Button("Process and Save (Precise)"))
        {
            if (sourceTexture != null)
            {
                // Ensure texture is readable before processing
                if (!IsTextureReadable(sourceTexture))
                {
                    EditorUtility.DisplayDialog("Read/Write Disabled", "Please enable Read/Write for the source texture first.", "OK");
                    return;
                }

                string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
                string directoryPath = Path.GetDirectoryName(assetPath) + "/Fix";
                string fileName = Path.GetFileNameWithoutExtension(assetPath) + "_processed_precise.png";
                string fullPath = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                ProcessAndSaveTexture(sourceTexture, fullPath, false, 0f, flipHorizontal, flipVertical, convertToSprite);
            }
            else
            {
                EditorUtility.DisplayDialog("Texture not selected", "Please select a source texture to process.", "OK");
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Aggressive Removal Settings", EditorStyles.boldLabel);
        colorTolerance = EditorGUILayout.Slider("Color Tolerance", colorTolerance, 0f, 1f);

        // Aggressive processing button
        if (GUILayout.Button("Process and Save (Aggressive)"))
        {
            if (sourceTexture != null)
            {
                // Ensure texture is readable before processing
                if (!IsTextureReadable(sourceTexture))
                {
                    EditorUtility.DisplayDialog("Read/Write Disabled", "Please enable Read/Write for the source texture first.", "OK");
                    return;
                }

                string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
                string directoryPath = Path.GetDirectoryName(assetPath) + "/Fix";
                string fileName = Path.GetFileNameWithoutExtension(assetPath) + "_processed_aggressive.png";
                string fullPath = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                ProcessAndSaveTexture(sourceTexture, fullPath, true, colorTolerance, flipHorizontal, flipVertical, convertToSprite);
            }
            else
            {
                EditorUtility.DisplayDialog("Texture not selected", "Please select a source texture to process.", "OK");
            }
        }
    }

    // Helper function to check if texture is readable
    private bool IsTextureReadable(Texture2D tex)
    {
        if (tex == null) return false;
        string assetPath = AssetDatabase.GetAssetPath(tex);
        TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        return textureImporter != null && textureImporter.isReadable;
    }

    private void ProcessAndSaveTexture(Texture2D texture, string path, bool isAggressive, float tolerance, bool doFlipHorizontal, bool doFlipVertical, bool makeSprite)
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        bool originalReadableStateWas = false;

        if (textureImporter != null)
        {
            originalReadableStateWas = textureImporter.isReadable;
            if (!originalReadableStateWas)
            {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }
        }
        else
        {
            Debug.LogError("Could not get TextureImporter. Make sure the texture is part of the project assets.");
            EditorUtility.DisplayDialog("Error", "Could not get TextureImporter. Ensure the texture is a project asset.", "OK");
            return;
        }
        
        Texture2D readableTexture = texture;
        if (!originalReadableStateWas) {
            readableTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (readableTexture == null) {
                 Debug.LogError("Failed to reload readable texture.");
                if (textureImporter != null && !originalReadableStateWas)
                {
                    textureImporter.isReadable = false;
                    AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                }
                EditorUtility.DisplayDialog("Error", "Failed to reload readable texture after enabling Read/Write.", "OK");
                return;
            }
        }

        Texture2D newTexture = new Texture2D(readableTexture.width, readableTexture.height, TextureFormat.ARGB32, false);
        Color[] pixels = readableTexture.GetPixels();
        int width = readableTexture.width;
        int height = readableTexture.height;

        for (int i = 0; i < pixels.Length; i++)
        {
            bool removePixel = false;
            if (isAggressive)
            {
                if (Mathf.Abs(pixels[i].r - removeColor.r) <= tolerance &&
                    Mathf.Abs(pixels[i].g - removeColor.g) <= tolerance &&
                    Mathf.Abs(pixels[i].b - removeColor.b) <= tolerance)
                {
                    removePixel = true;
                }
            }
            else
            {
                if (Mathf.Approximately(pixels[i].r, removeColor.r) &&
                    Mathf.Approximately(pixels[i].g, removeColor.g) &&
                    Mathf.Approximately(pixels[i].b, removeColor.b))
                {
                    removePixel = true;
                }
            }

            if (removePixel)
            {
                pixels[i].a = 0; 
            }
        }

        if (doFlipHorizontal)
        {
            FlipPixelsHorizontal(pixels, width, height);
        }
        if (doFlipVertical)
        {
            FlipPixelsVertical(pixels, width, height);
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        byte[] bytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Revert original texture import settings
        if (textureImporter != null && !originalReadableStateWas)
        {
            textureImporter.isReadable = false;
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        }
        
        if (newTexture != readableTexture && newTexture != texture) { 
            DestroyImmediate(newTexture);
        }

        AssetDatabase.Refresh();

        // Configure the new texture's import settings
        ConfigureOutputTexture(path, makeSprite);

        Debug.Log($"Texture saved to: {path}");
        EditorUtility.DisplayDialog("Processing Complete", $"Texture saved to:\n{path}", "OK");
    }

    private void ConfigureOutputTexture(string texturePath, bool makeSprite)
    {
        // Wait for the asset to be imported
        AssetDatabase.Refresh();
        
        // Get the relative path for AssetDatabase
        string relativePath = texturePath;
        if (texturePath.StartsWith(Application.dataPath))
        {
            relativePath = "Assets" + texturePath.Substring(Application.dataPath.Length);
        }

        TextureImporter newTextureImporter = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (newTextureImporter != null)
        {
            // Always enable Alpha is Transparency for transparent images
            newTextureImporter.alphaIsTransparency = true;
            
            // Set texture type based on user preference
            if (makeSprite)
            {
                newTextureImporter.textureType = TextureImporterType.Sprite;
                newTextureImporter.spriteImportMode = SpriteImportMode.Single;
            }
            else
            {
                newTextureImporter.textureType = TextureImporterType.Default;
            }

            // Apply the import settings
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Configured texture import settings: Alpha is Transparency = true, Texture Type = {(makeSprite ? "Sprite" : "Default")}");
        }
        else
        {
            Debug.LogWarning($"Could not configure import settings for texture at path: {relativePath}");
        }
    }

    private void FlipPixelsHorizontal(Color[] pixels, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width / 2; x++)
            {
                int index1 = y * width + x;
                int index2 = y * width + (width - 1 - x);
                Color temp = pixels[index1];
                pixels[index1] = pixels[index2];
                pixels[index2] = temp;
            }
        }
    }

    private void FlipPixelsVertical(Color[] pixels, int width, int height)
    {
        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index1 = y * width + x;
                int index2 = (height - 1 - y) * width + x;
                Color temp = pixels[index1];
                pixels[index1] = pixels[index2];
                pixels[index2] = temp;
            }
        }
    }
}