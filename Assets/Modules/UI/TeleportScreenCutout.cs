using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui
{
    public class TeleportScreenCutout
    {
        private const float MAX_SIZE = 1.4f;
        private const string SHADER_SIZE_PROPERTY = "_Size";
        private const string SHADER_SCREEN_SIZE_PROPERTY = "_Screen";
        private const string SHADER_TARGET_PROPERTY = "_TargetPosition";

        private readonly int sizeId = Shader.PropertyToID(SHADER_SIZE_PROPERTY);
        private readonly int targetPositionId = Shader.PropertyToID(SHADER_TARGET_PROPERTY);
        private readonly int screenSizeId = Shader.PropertyToID(SHADER_SCREEN_SIZE_PROPERTY);
        private readonly Image image;
        private readonly Camera camera;
        private readonly UICanvas uiCanvas;
        private readonly Material shaderMaterial;
        private readonly RectTransform rectTransform;

        private Tween tween;

        public TeleportScreenCutout(Image image, UICanvas uiCanvas, RectTransform rectTransform)
        {
            this.image = image;
            camera = Camera.main;
            this.uiCanvas = uiCanvas;
            this.rectTransform = rectTransform;
            shaderMaterial = image.materialForRendering;
            this.image.transform.SetParent(this.uiCanvas.RectTransform);
            this.image.transform.localScale = Vector3.one;
            this.image.gameObject.SetActive(true);
            shaderMaterial.SetFloat(sizeId, MAX_SIZE);
            shaderMaterial.SetVector(screenSizeId, new Vector4(Screen.width, Screen.height));
            shaderMaterial.SetVector(targetPositionId, new Vector4(Screen.width, Screen.height) * 0.5f);
        }

        public void FadeIn(Transform target)
        {
            shaderMaterial.SetVector(screenSizeId, new Vector4(Screen.width, Screen.height));
            image.enabled = true;
            var screenPosition = camera.WorldToScreenPoint(target.position + Vector3.up);
            shaderMaterial.SetVector(targetPositionId, screenPosition);
            shaderMaterial.SetFloat(sizeId, MAX_SIZE);
            tween?.Kill();
            float value = MAX_SIZE;
            tween = DOTween.To(() => value, x => value = x, 0f, 1).OnUpdate(() =>
            {
                shaderMaterial.SetFloat(sizeId, value);
            }).SetEase(Ease.InOutQuart);
        }

        public async UniTask FadeOut(Transform target)
        {
            shaderMaterial.SetVector(screenSizeId, new Vector4(Screen.width, Screen.height));
            image.enabled = true;
            var screenPosition = camera.WorldToScreenPoint(target.position + Vector3.up * 3);
            shaderMaterial.SetVector(targetPositionId, screenPosition);
            shaderMaterial.SetFloat(sizeId, 0f);
            tween?.Kill();
            float value = 0;
            tween = DOTween.To(() => value, x => value = x, MAX_SIZE, 1).OnUpdate(() =>
            {
                shaderMaterial.SetFloat(sizeId, value);
            }).SetEase(Ease.InOutQuart);
            await UniTask.WaitWhile(() => value < MAX_SIZE - 0.01f);
        }
    }
}