using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.Samples.Tank 
{
    public class TankShellManager : MonoBehaviour 
    {
    #region Singleton
        public static TankShellManager Instance 
        {
            get; private set;
        }
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy (this.gameObject);
                return;
            }
            Instance = this;
        }
        private void OnDestroy()
        {
            if(Instance == this)
            {
                Instance = null;
            }
        }
    #endregion
        [SerializeField] private Transform poolParent;
        public Rigidbody m_Shell;                   // Prefab of the shell.
        private List<TankShell> shellPool;                  //Reuse from the same pool for all users
        private int currentShellIndex;
        /// <summary>
        /// Create enough shells before the game.
        /// </summary>
        internal void GenerateShellPool(int MemberCount)
        {
            shellPool = new List<TankShell>();
            for(int i = 0; i < MemberCount * 3; i++) 
            {
                shellPool.Add(Instantiate(m_Shell, poolParent).GetComponent<TankShell>());
                shellPool[i].Init(poolParent);
            }
            currentShellIndex = 0;
        }

        /// <summary>
        /// Call this from fire action.
        /// </summary>
        /// <param name="AttackerId">Who's fired?</param>
        /// <param name="data">Shooting information</param>
        internal void FireShell(UserId AttackerId, TankShootingData data)
        {
            shellPool[currentShellIndex].FireShell(AttackerId, data).Forget();
            UpdateShellIndex();
            // Play fire.
            TankAudioManager.Instance.PlayShootingClipOneshot(ShootingClips.Fire);
        }
        private void UpdateShellIndex()
        {
            currentShellIndex = currentShellIndex + 1 < shellPool.Count ? currentShellIndex + 1 : 0;
        }
    }
}