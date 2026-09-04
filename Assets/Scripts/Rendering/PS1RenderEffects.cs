using UnityEngine;

namespace Lumber.Rendering
{
    /// Recreates the chunky PS1 look: the camera renders to a small, fixed-height
    /// RenderTexture that is then blit back up to the screen with point (nearest)
    /// filtering, producing the classic blocky, low-resolution pixelation.
    [RequireComponent(typeof(Camera))]
    public class PS1RenderEffects : MonoBehaviour
    {
        public int targetHeight = 240;

        private Camera cam;
        private RenderTexture lowResTexture;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            QualitySettings.antiAliasing = 0;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            EnsureRenderTexture();
            Graphics.Blit(source, lowResTexture);
            Graphics.Blit(lowResTexture, destination);
        }

        private void EnsureRenderTexture()
        {
            if (lowResTexture != null && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
                return;

            if (lowResTexture != null)
                lowResTexture.Release();

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            int h = Mathf.Max(64, targetHeight);
            int w = Mathf.Max(64, Mathf.RoundToInt(h * aspect));

            lowResTexture = new RenderTexture(w, h, 16)
            {
                filterMode = FilterMode.Point,
                antiAliasing = 1
            };

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        private void OnDestroy()
        {
            if (lowResTexture != null)
                lowResTexture.Release();
        }
    }
}
