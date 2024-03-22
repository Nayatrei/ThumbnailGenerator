using UnityEngine;
using UnityEditor;
using System.IO;

public class CsThumbnailGenerator : EditorWindow
{

    private static bool updateBehaviour = false;
    private Color backGroundColor = Color.magenta; // The color to remove
    private Texture2D sourceTexture;

    [MenuItem("Tools/Toolkit/Thumbnail Generator")]
    public static void ShowWindow()
    {
        GetWindow<CsThumbnailGenerator>("Thumbnail Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Background Color for Thumbnail", EditorStyles.boldLabel);

        backGroundColor = EditorGUILayout.ColorField("Background Color", backGroundColor);

        GUILayout.Label("Generate Prefab Thumbnail with Background", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("128"))
        {
            GenerateThumbnailForSelectedPrefab(128, false);
        }

        if (GUILayout.Button("256"))
        {
            GenerateThumbnailForSelectedPrefab(256, false);
        }

        if (GUILayout.Button("512"))
        {
            GenerateThumbnailForSelectedPrefab(512, false);
        }

        if (GUILayout.Button("1024"))
        {
            GenerateThumbnailForSelectedPrefab(1024 ,false);
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Generate Prefab Clear Thumbnail", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("128"))
        {
            GenerateThumbnailForSelectedPrefab(128, true);
        }
        if (GUILayout.Button("256"))
        {
            GenerateThumbnailForSelectedPrefab(256, true);
        }
        if (GUILayout.Button("512"))
        {
            GenerateThumbnailForSelectedPrefab(512, true);
        }
        if (GUILayout.Button("1024"))
        {
            GenerateThumbnailForSelectedPrefab(1024, true);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Remove background color from texture", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
        if (GUILayout.Button("Process and Save"))
        {
            if (sourceTexture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
                string directoryPath = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath) + "_processed.png";
                string fullPath = Path.Combine(directoryPath, fileName);

                // Ensure the Fix directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                ProcessAndSaveTexture(sourceTexture, fullPath, backGroundColor);
            }
            else
            {
                EditorUtility.DisplayDialog("Texture not selected", "Please select a source texture to process.", "OK");
            }
        }
    }

    void GenerateThumbnailForSelectedPrefab(int size, bool transparency)
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject == null || !(selectedObject is GameObject))
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("The selected GameObject is not an asset or its path could not be determined.");
            return;
        }

        string directoryPath = Path.GetDirectoryName(assetPath);
        // Check if the directoryPath is valid or not empty. If not, default to a specific directory.
        if (string.IsNullOrEmpty(directoryPath))
        {
            directoryPath = "Assets/Thumbnails"; // Default directory if specific path can't be determined
        }
        else
        {
            directoryPath += "/Thumbnails"; // Append Thumbnails subfolder for valid paths
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileName = Path.GetFileNameWithoutExtension(assetPath) + $"_thumbnail_{size}.tga";
        string fullPath = Path.Combine(directoryPath, fileName);
        Texture2D texture = Capture(size, size, backGroundColor);
        if (!transparency)
        {
            SaveTexture(fullPath, texture);
        }
        else
        {
            ProcessAndSaveTexture(texture, fullPath, backGroundColor);
        }
        Debug.Log($"Thumbnail saved to {fullPath}");
        AssetDatabase.Refresh();



    }
    public void CaptureScreen(string filename, int size)
    {
        ScreenCapture.CaptureScreenshot(filename, size);
    }


    public void CaptureEditorScreenTransparent(string filename, int size)
    {
        Texture2D texture = Capture(size, size, backGroundColor);

        SaveTexture(filename, texture);
    }

    public static Texture2D Capture(int width, int height, Color backGroundColor)
    {
        Texture2D texture = null;

        Camera camera = SceneView.lastActiveSceneView.camera;

        // save data which we'll modify
        RenderTexture prevRenderTexture = RenderTexture.active;
        RenderTexture prevCameraTargetTexture = camera.targetTexture;
        bool prevCameraEnabled = camera.enabled;
        float prevFieldOfView = camera.fieldOfView;

 
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);

        try
        {

            camera.enabled = updateBehaviour;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backGroundColor; // Transparent background
            camera.targetTexture = renderTexture;

            // render a single frame
            camera.Render();

            texture = CreateTexture2D(renderTexture);

        }
        finally
        {
            RenderTexture.ReleaseTemporary(renderTexture);

            // restore modified data
            RenderTexture.active = prevRenderTexture;

            camera.targetTexture = prevCameraTargetTexture;
            camera.enabled = prevCameraEnabled;
            camera.fieldOfView = prevFieldOfView;

        }

        return texture;

    }

    private static Texture2D CreateTexture2D(RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

        RenderTexture prevRT = RenderTexture.active;
        {
            RenderTexture.active = renderTexture;

            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
        }
        RenderTexture.active = prevRT;

        return texture;
    }


    void SaveTexture(string fullPath, Texture2D texture)
    {

        Debug.Log("Saving: " + fullPath);

        byte[] bytes = ImageConversion.EncodeToTGA(texture);

        System.IO.File.WriteAllBytes(fullPath, bytes);

        // Import asset so that it is recognized by Unity
        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);

        // Change texture import settings to Sprite
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

    }
    void ProcessAndSaveTexture(Texture2D texture, string path, Color removeColor)
    {
        // Make sure the texture import settings are set to Read/Write Enabled
        if (!texture.isReadable)
        {
            Debug.LogError("Texture is not set to Read/Write Enabled in its import settings.");
            return;
        }

        Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            if (Mathf.Approximately(pixels[i].r, removeColor.r) &&
                Mathf.Approximately(pixels[i].g, removeColor.g) &&
                Mathf.Approximately(pixels[i].b, removeColor.b))
            {
                pixels[i].a = 0; // Set alpha to 0 to make transparent
            }
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        byte[] bytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        AssetDatabase.Refresh();

        Debug.Log($"Texture saved to: {path}");

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
//            importer.sRGBTexture = false;
//            importer.isReadable = true;
            importer.SaveAndReimport();
        }

    }
}
