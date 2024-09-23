using System.IO;
using UnityEngine;

public class LocationSaveFile
{
    public readonly FileInfo SaveFile;
    public readonly FileInfo PreviewPicture;
    public readonly string SaveName;

    public DirectoryInfo Directory => SaveFile.Directory;

    public LocationSaveFile(FileInfo saveFile, FileInfo savePreview, string saveName)
    {
        SaveFile = saveFile;
        PreviewPicture = savePreview;
        SaveName = saveName;
    }

    public LocationSaveData GetLocationSaveData()
    {
        var readedLocationData = File.ReadAllText(SaveFile.FullName);

        return JsonUtility.FromJson<LocationSaveData>(readedLocationData);
    }

    public Texture2D GetLocationPreviewTexture()
    {
        if (PreviewPicture == null) return null;

        var readedPreviewImage = File.ReadAllBytes(PreviewPicture.FullName);
        var texturePreview = new Texture2D(1, 1);
        texturePreview.LoadImage(readedPreviewImage);

        return texturePreview;
    }

    public void Clear()
    {
        foreach (var file in Directory.GetFiles())
            file.Delete();

        Directory.Delete();
    }
}
