using UnityEngine;
using UnityEngine.UI;
using ZXing;

// Requires the ZXing.Net library (import via NuGet/UnityNuGet or a free QR scanner asset).
// Optional secondary entry point — primary entry is the external QR scan -> deep link
// handled by the landing page (see /webar/index.html and the Android App Links setup).
public class QRScannerController : MonoBehaviour
{
    [SerializeField] private RawImage cameraPreview;
    [SerializeField] private string expectedSceneCode = "mihintale";

    private WebCamTexture camTexture;
    private IBarcodeReader reader = new BarcodeReader();
    private bool sceneLoaded = false;

    void Start()
    {
        camTexture = new WebCamTexture();
        cameraPreview.texture = camTexture;
        camTexture.Play();
    }

    void Update()
    {
        if (sceneLoaded || camTexture == null || !camTexture.didUpdateThisFrame) return;

        try
        {
            var result = reader.Decode(camTexture.GetPixels32(), camTexture.width, camTexture.height);
            if (result != null && result.Text.Contains(expectedSceneCode))
            {
                sceneLoaded = true;
                camTexture.Stop();
                UnityEngine.SceneManagement.SceneManager.LoadScene("ARMihintaleScene");
            }
        }
        catch { /* no QR in frame this tick — ignore */ }
    }
}
