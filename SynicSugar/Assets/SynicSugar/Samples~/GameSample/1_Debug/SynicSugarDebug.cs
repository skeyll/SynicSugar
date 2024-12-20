using UnityEngine.UI;
using UnityEngine;
using System;

namespace SynicSugar.Samples 
{
    public class SynicSugarDebug : MonoBehaviour 
    {
    #region Singleton Instance
        public static SynicSugarDebug Instance { get; private set; }
        private void Awake() 
        {
            if( Instance != null ) 
            {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }
        private void OnDestroy() 
        {
            if( Instance == this )
            {
                Instance = null;
            }
        }
    #endregion
        [SerializeField] private Text debug;
        public void Log(string log)
        {
            #if UNITY_EDITOR
            Debug.Log(log);
            #endif
            debug.text += (DateTime.Now.ToLongTimeString() + " " + log + System.Environment.NewLine);
        }
        public void Log(string log, Result result)
        {
            #if UNITY_EDITOR
            Debug.Log(log + " ErrorCode: " + result);
            #endif
            DateTime dt = DateTime.Today;
            debug.text += (DateTime.Now.ToLongTimeString() + " " + log + " ErrorCode: " + result + System.Environment.NewLine);
        }
        public void DeleteLog()
        {
            debug.text = System.String.Empty;
        }
        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = debug.text;
        }
    }
}