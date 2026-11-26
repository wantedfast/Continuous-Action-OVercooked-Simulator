using UnityEngine;
using System.IO;

public class CameraCapture : MonoBehaviour
{
    public Camera targetCamera;
    public int width = 3840;  // 4K
    public int height = 2160;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(Capture());
        }
    }

    System.Collections.IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        targetCamera.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine("/home/geralt/Env", $"CameraShot_{System.DateTime.Now:HHmmss}.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"📸 Saved camera shot: {path}");
    }
}
