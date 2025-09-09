using UnityEngine;
using System.Collections;
using System.Threading;

namespace PurrLobby
{
    public class IntroView : View
    {
        public ViewManager viewManager;
        public RectTransform RT_BG;
        public CanvasGroup CG_BG;

        public override void OnShow()
        {
            base.OnShow();
            CG_BG.alpha = 1.0f;
            RT_BG.localScale = Vector3.one;
            StartCoroutine(Fade(CG_BG, false, 3f));
            StartCoroutine(OnShowCoroutine());
        }

        public IEnumerator OnShowCoroutine(float _dur = 8f)
        {
            float _timer = 0;
            while (_timer < 1)
            {
                RT_BG.localScale = Vector3.one * Mathf.Lerp(1f, 0.95f, _timer);
                yield return new WaitForEndOfFrame();

                _timer += Time.deltaTime / _dur;
                if (_timer > 0.1f)
                    if (Input.anyKey)
                        _timer = 1f;
            }
            viewManager.HideView<IntroView>(1f);
            viewManager.ShowView<MainMenuView>(false, 1f);
        }
    }
}
