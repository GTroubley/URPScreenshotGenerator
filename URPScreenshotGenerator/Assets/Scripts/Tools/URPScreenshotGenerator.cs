using System.Collections;
using UnityEditor;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

#if UNITY_EDITOR
public class URPScreenshotGenerator : MonoBehaviour
{
    [Header("Screenshot Size")]
    [SerializeField] private Vector2 _size = new Vector2(1024, 1024);
    [SerializeField] private bool _AddTransparency = false;
    [Header("Objects To Screenshot")] 
    [SerializeField] private GameObject[] _gameObjectsToScreenshot = null;

    [SerializeField] private Transform _placeholder = null;
    private bool CanMoveToNextItem;

    private void Start() => StartCoroutine(GenerateScreenShots());

    // For each item of the GameObject list,
    // instantiate that GameObject, start the CaptureScreenshot Coroutine
    // and at last destroy the instantiaded GameObject before moving to the next one.
    public IEnumerator GenerateScreenShots()
    {
        yield return new WaitForSeconds(1f);
        foreach (var item in _gameObjectsToScreenshot)
        {
            var instantiatedObject = Instantiate(item, _placeholder.localPosition, _placeholder.localRotation);
            yield return new WaitForSeconds(.1f);
            CanMoveToNextItem = true;
            StartCoroutine(CaptureScreenshot(instantiatedObject));
            yield return new WaitForSeconds(.1f);
            Destroy(instantiatedObject);
        }
    }
    
    // Capture the screen, encode and save!
    private IEnumerator CaptureScreenshot(GameObject instantiatedObject)
    {
        
        if (CanMoveToNextItem)
        {yield return new WaitForEndOfFrame();
            int width = (int) _size.x;
            int height = (int) _size.y;

            CanMoveToNextItem = false;
            Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, width, height);
            rect.center = new Vector2(Screen.width / 2, Screen.height / 2);
            screenshotTexture.ReadPixels(rect, 0, 0);

            if (_AddTransparency)
            {
                Color[] pixels = screenshotTexture.GetPixels(0, 0, width, height);
                for (int i = 0; i < pixels.Length; i++)
                    if (pixels[i] == Color.black)
                        pixels[i] = Color.clear;

                screenshotTexture.SetPixels(0, 0, screenshotTexture.width, screenshotTexture.height, pixels);
            }

            screenshotTexture.Apply();

            byte[] byteArray = screenshotTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(
                Application.dataPath + "/Resources/ScreenshotGenerator/" + instantiatedObject.name + ".png", byteArray);
            Debug.Log(instantiatedObject.name + " Captured!");
            AssetDatabase.Refresh();
        }
    }
}
#endif