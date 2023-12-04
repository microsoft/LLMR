using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class Screenshotter : MonoBehaviour
{
    // A key to trigger the screenshot
    //public KeyCode screenshotKey = KeyCode.F12;

    public string snapshot_dir;

    // A counter to name the screenshots
    private int screenshotCount = 0;

    // The path to the img folder
    private string imgPath;

    void Awake()
    {
        // Create the img folder if it does not exist
        string img_folder_name = "Images";
        imgPath = Path.Combine(Application.dataPath, img_folder_name);
        Directory.CreateDirectory(imgPath);
        snapshot_dir = imgPath + "/current_scene.png";
    }

    //public async Task TakeSnapshotAsync()
    //{
    //    await ScreenCapture.CaptureScreenshot(snapshot_dir);
    //}

    public void TakeSnapshot()
    {
        // Capture the screenshot and save it to the file
        ScreenCapture.CaptureScreenshot(snapshot_dir);

        // Log a message
        //Debug.Log("Screenshot saved to " + filePath);
    }

    public byte[] GetSnapshot()
    {
        RenderTexture rt = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 24, RenderTextureFormat.ARGB32);
        // Set the camera to render to the RenderTexture
        Camera.main.targetTexture = rt;
        // Render the camera view
        Camera.main.Render();
        // Create a Texture2D with the same dimensions and format as the RenderTexture
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        // Read the pixels from the RenderTexture into the Texture2D
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        // Reset the camera and the active RenderTexture
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        // Encode the Texture2D as a byte array, for example as PNG
        byte[] bytes = tex.EncodeToPNG();
        // Optionally, destroy the Texture2D
        Destroy(tex);

        return bytes;
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    // Check if the screenshot key is pressed
    //    if (Input.GetKeyDown(screenshotKey))
    //    {
    //        // Generate a file name based on the counter
    //        string fileName = "screenshot_" + screenshotCount + ".png";
    //        screenshotCount++;

    //        // Combine the path and the file name
    //        string filePath = Path.Combine(imgPath, fileName);

    //        // Capture the screenshot and save it to the file
    //        ScreenCapture.CaptureScreenshot(filePath);

    //        // Log a message
    //        Debug.Log("Screenshot saved to " + filePath);
    //    }
    //}
}