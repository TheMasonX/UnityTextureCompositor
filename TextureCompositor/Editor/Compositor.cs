using UnityEngine;
using UnityEditor;
using System.IO;

public class Compositor : ScriptableWizard
{
    public CompositorData compositorData;


    //store the last values 
    public static CompositorData lastData;

    [MenuItem("Tools/Composite Textures")]
    public static void CreateWizard ()
    {
        var wizard = ScriptableWizard.DisplayWizard<Compositor>("Composite Textures", "Save To File");
        if (lastData != null)
            wizard.compositorData = new CompositorData(lastData);
        else
            wizard.compositorData = new CompositorData(512, "/Assets/");
    }

    void OnWizardCreate ()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Composited Data", "Composited Texture", "png", "Select Where To Save Composited Texture", compositorData.path);
        if (path.Length == 0)
        {
            if(EditorUtility.DisplayDialog("Canceling Composite", "Composite Incomplete. All Changed Settings Will Be Lost. Are You Sure You Wish To Cancel?", "Yes, Exit Without Saving", "No, Go Back"))
            {
                return;
            }
            else
            {
                CreateWizard();
                return;
            }
        }

        //selected a valid path, save the settings before continuing
        compositorData.path = path;
        lastData = compositorData;

        Texture2D tex = new Texture2D(compositorData.textureWidth, compositorData.textureHeight);
        Color[] pixels = new Color[compositorData.textureWidth * compositorData.textureHeight];

        float uvX, uvY;
        int pixelIndex;

        //go through the pixel grid, and get the bilinear filtered composited color for each pixel
        for (int x = 0; x < compositorData.textureWidth; x++)
        {
            uvX = x / (compositorData.textureWidth - 1f);
            for (int y = 0; y < compositorData.textureHeight; y++)
            {
                uvY = y / (compositorData.textureHeight - 1f);

                pixelIndex = x + y * compositorData.textureWidth;
                pixels[pixelIndex] = compositorData.GetColor(uvX, uvY);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false);

        byte[] bytes = tex.EncodeToPNG();

        File.WriteAllBytes(path, bytes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

[System.Serializable]
public class CompositorData
{
    [Header("--- Ouput Texture Settings ---")]
    public int textureWidth = 512;
    public int textureHeight = 512;

    [Space(20f), HideInInspector]
    public string path = "/Assets/";

    [Header("--- Input Channel Settings ---")]
    public CompositeChannel red;
    public CompositeChannel green;
    public CompositeChannel blue;
    public CompositeChannel alpha;

    public CompositorData (CompositorData otherData)
    {
        red = otherData.red;
        green = otherData.green;
        blue = otherData.blue;
        alpha = otherData.alpha;

        textureWidth = otherData.textureWidth;
        textureHeight = otherData.textureHeight;
        path = otherData.path;
    }

    public CompositorData (int textureResolution, string defaultPath)
    {
        textureWidth = textureHeight = textureResolution;
        path = defaultPath;
    }

    public Color GetColor (float u, float v)
    {
        return new Color(red.GetValue(u, v), green.GetValue(u, v), blue.GetValue(u, v), alpha.GetValue(u, v));
    }
}

[System.Serializable]
public struct CompositeChannel
{
    //custom attribute to ensure that the textures are readable, which means we have to have them as separate properties, not an array
    [ReadWriteEnabled]
    public Texture2D sourceTexture;
    [Tooltip("Channel Value If Texture Is Not Assigned")]
    public float defaultValue;

    public float GetValue (float u, float v)
    {
        return sourceTexture ? sourceTexture.GetPixelBilinear(u, v).r : defaultValue;
    }
}