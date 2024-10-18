using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

namespace SynicSugar.Samples.Tank
{
    /// <summary>
    /// This script is just for local players, only result are sent via RPC.
    /// </summary>
    public class TankActions : MonoBehaviour
    {
        [SerializeField] private Transform Turret;          // Objects from which shell are fired
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.

        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        
        private CancellationTokenSource chargeTokeSource;
        private void OnEnable()
        {
            Init();
        }
        private void Awake()
        {
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }
        private void OnDisable()
        {
            // Stop the charge process when a player dead.
            if(chargeTokeSource != null && chargeTokeSource.Token.CanBeCanceled)
            {
                chargeTokeSource.Cancel();
            }
        }
        internal void Init()
        {
            if(chargeTokeSource != null && chargeTokeSource.Token.CanBeCanceled)
            {
                chargeTokeSource.Cancel();
            }

            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }
        /// <summary>
        /// Start preparing to fire.
        /// </summary>
        public void StartCharge ()
        {
            //Startcharging power to fire.
            m_CurrentLaunchForce = m_MinLaunchForce;

            chargeTokeSource = new CancellationTokenSource();
            ChargeFireForce(chargeTokeSource.Token).Forget();
        }
        /// <summary>
        /// Charge power for as long as it takes to release the button
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask ChargeFireForce(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
                await UniTask.Yield(token);
            }
        }
        /// <summary>
        /// Call RPC with data on when the button is released.
        /// </summary>
        internal void ReleaseTheTriger(bool isLocal)
        {
            if(chargeTokeSource == null || !chargeTokeSource.Token.CanBeCanceled)
                return;

            chargeTokeSource.Cancel();

            if(isLocal)
            {
                TankPlayer player = ConnectHub.Instance.GetUserInstance<TankPlayer>(p2pInfo.Instance.LocalUserId);
                //Not fired when local player is dead.
                if(player.status.CurrentHP <= 0)
                    return;

                TankAudioManager.Instance.StopShootingSource();
                //Fired via local Rpc.
                player.Fire(TankShootingData.GenerateShootingPacket(m_CurrentLaunchForce, Turret));
            }

            Init();
        }
    }
}