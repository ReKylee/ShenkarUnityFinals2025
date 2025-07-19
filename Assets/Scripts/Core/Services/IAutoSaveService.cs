using System;
using UnityEngine;

namespace Core.Services
{
    public interface IAutoSaveService
    {
        bool IsEnabled { get; set; }
        float SaveInterval { get; set; }
        void Update();
        void RequestSave();
        void ForceSave();
        void OnApplicationPause(bool pauseStatus);
        void OnApplicationFocus(bool hasFocus);
        event Action OnSaveRequested;
    }

    public class AutoSaveService : IAutoSaveService
    {
        private bool _hasPendingSave;
        private float _lastSaveTime;

        public bool IsEnabled { get; set; } = true;
        public float SaveInterval { get; set; } = 30f;

        public event Action OnSaveRequested;

        public void Update()
        {
            if (!IsEnabled) return;

            if (_hasPendingSave || Time.time - _lastSaveTime >= SaveInterval)
            {
                RequestSave();
            }
        }

        public void RequestSave()
        {
            if (!IsEnabled) return;

            _hasPendingSave = false;
            _lastSaveTime = Time.time;
            OnSaveRequested?.Invoke();
        }

        public void ForceSave()
        {
            _lastSaveTime = Time.time;
            OnSaveRequested?.Invoke();
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsEnabled)
            {
                ForceSave();
            }
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsEnabled)
            {
                _hasPendingSave = true;
            }
        }
    }
}
