using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PurrLobby
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private List<View> allViews = new();
        [SerializeField] private View defaultView;

        private void Start()
        {
            foreach (var view in allViews)
            {
                HideViewInternal(view);
            }
            ShowViewInternal(defaultView);
        }

        public void ShowView<T>(bool hideOthers = true, float fadeTimer = 0) where T : View
        {
            foreach (var view in allViews)
            {
                if (!view)
                    continue;
                if (view.GetType() == typeof(T))
                {
                    ShowViewInternal(view, fadeTimer);
                }
                else
                {
                    if(hideOthers)
                        HideViewInternal(view, fadeTimer);
                }
            }
        }

        public void HideView<T>(float fadeTimer = 0) where T : View
        {
            foreach (var view in allViews)
            {
                if(view.GetType() == typeof(T))
                    HideViewInternal(view, fadeTimer);
            }
        }

        private void ShowViewInternal(View view, float fadeTimer = 0)
        {
            if (fadeTimer > 0)
                StartCoroutine(View.Fade(view.canvasGroup, true, fadeTimer));
            else
                view.canvasGroup.alpha = 1;
            view.canvasGroup.interactable = true;
            view.canvasGroup.blocksRaycasts = true;
            view.OnShow();
            view.OnViewShow?.Invoke();
        }

        private void HideViewInternal(View view, float fadeTimer = 0)
        {
            if(!view)
                return;

            if (view.canvasGroup)
            {
                if (fadeTimer > 0)
                    StartCoroutine(View.Fade(view.canvasGroup, false, fadeTimer));
                else
                    view.canvasGroup.alpha = 0;
                view.canvasGroup.interactable = false;
                view.canvasGroup.blocksRaycasts = false;
            }
            
            view.OnHide();
            view.OnViewHide?.Invoke();
        }

        #region Events

        public void OnRoomJoined()
        {
            ShowView<LobbyView>();
        }   
        
        public void OnRoomLeft()
        {
            ShowView<MainMenuView>();
        }

        public void OnBrowseClicked()
        {
            ShowView<BrowseView>();
        }
        
        public void OnRoomCreateClicked()
        {
            ShowView<CreatingRoomView>(false);
        }
        
        public void OnJoiningRoom()
        {
            ShowView<LoadingRoomView>(false);
        }

        public void OnLeaveBrowseClicked()
        {
            ShowView<MainMenuView>();
        }
        public void OnSettingsClicked()
        {
            ShowView<SettingsView>();
        }

        #endregion
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class View : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        public CanvasGroup canvasGroup => _canvasGroup;

        public UnityEvent OnViewShow = new();
        public UnityEvent OnViewHide = new();

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnShow() {}
        public virtual void OnHide() {}

        public static IEnumerator Fade(CanvasGroup _canvas, bool _fadeIn = true, float _speed = 0.1f)
        {
            if (_fadeIn && _canvas.alpha == 1)
                yield return null;
            if (!_fadeIn && _canvas.alpha == 0)
                yield return null;
            float _timer = 0f;
            float _start = _fadeIn ? 0f : 1f;
            float _end = _fadeIn ? 1f : 0f;
            while (_timer < 1f)
            {
                _canvas.alpha = Mathf.Lerp(_start, _end, _timer);
                yield return new WaitForEndOfFrame();
                _timer += Time.unscaledDeltaTime / _speed;
            }
            _canvas.alpha = _end;
        }
    }
}
