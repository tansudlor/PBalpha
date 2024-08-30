using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public class UniversalDeeplinkingRuntimeScript : MonoBehaviour
    {
        public static UniversalDeeplinkingRuntimeScript Instance;

        private List<Action> _tasks = new List<Action>();

        void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (_tasks.Count == 0)
                return;

            Action t;
            lock (_tasks)
            {
                t = _tasks[0];
                _tasks.RemoveAt(0);
            }

            try
            {
                t();
            }
            catch (Exception e)
            {
            }
        }

        public void Schedule(Action a)
        {
            lock (_tasks)
            {
                _tasks.Add(a);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                StartCoroutine(CallDeepLinkManagerAfterDelay());
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                StartCoroutine(CallDeepLinkManagerAfterDelay());
            }
        }

        private bool _onJob = false;
        public IEnumerator CallDeepLinkManagerAfterDelay()
        {
            if (_onJob)
                yield break;

            _onJob = true;

            yield return new WaitForSeconds(.2f);
            try
            {
                DeepLinkManager.Instance.GameCameFromPause();
            }
            catch (Exception e)
            {
                Debug.LogError("RuntimeScript " + e, gameObject);
            }
            finally
            {
                _onJob = false;
            }
        }
    }
}
