using UnityEngine;
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
namespace  SynicSugar.Samples.Tank {
    [NetworkPlayer(true)]
    [RequireComponent(typeof(Rigidbody))]
    public partial class TankPlayer : MonoBehaviour {
        //object
        [SerializeField] GameObject Crown;
        //ref
        TankHealth health;
        TankActions actions;
        TankMovement movement;
        [SerializeField] Text PlayerName;
        //data
        [Synic(0)] public TankPlayerStatus status = new();

        void Awake(){
            health = GetComponent<TankHealth>();
            actions = GetComponent<TankActions>();
            movement = GetComponent<TankMovement>();
        }
        /// <summary>
        /// Sync User data.
        /// </summary>
        /// <param name="status">Can sync via network</param>
        [Rpc]
        public void SetPlayerStatus(TankPlayerStatus status){
            this.status = status;
            this.transform.localPosition = status.RespawnPos;

            PlayerName.text = status.Name;
            movement.SetSpeed(status.Speed);
        }
        /// <summary>
        /// Init All status to default.
        /// Call this before round.
        /// </summary>
        internal void InitStatus(){
            movement.SetDefaults();
            health.SetHealth(status, status.MaxHP);
        }
        [Rpc]
        public void Ready(){
            status.isReady = true;
            InitStatus();
            ConnectHub.Instance.GetInstance<TankGameManager>().CheckReadyState();
        }
        /// <summary>
        /// Called from move button and as RPC.
        /// </summary>
        /// <param name="newDirection">Up or Down</param>
        [Rpc]
        public void Move(Direction newDirection){
            //Simplified because it's hard work. Sound only locally.
            if(isLocal){
                TankAudioManager.Instance.PlayTankClip(TankClips.Driving);
            }
            if(newDirection is Direction.Up or Direction.Down){
                movement.Move(newDirection).Forget();
            }else{
                movement.Turn(newDirection).Forget();
            }

        }
        /// <summary>
        /// Called from move button and as RPC.
        /// </summary>
        [Rpc]
        public void Stop(){
            //Simplified because it's hard work. Sound only locally.
            if(isLocal){
                TankAudioManager.Instance.PlayTankClip(TankClips.Idling);
            }
            movement.Stop();
        }
        [Rpc]
        public void StartCharge(){
            if(isLocal){
                TankAudioManager.Instance.PlayShootingClip(ShootingClips.Charge);
            }
            actions.StartCharge();
        }
        /// <summary>
        /// Stop charge and fire.
        /// </summary>
        internal void ReleaseTheTrigger(){
            actions.ReleaseTheTriger(isLocal);
        }
        [Rpc]
        public void Fire(TankShootingData data){
            TankShellManager.Instance.FireShell(OwnerUserID, data);
        }
        [Rpc]
        public void TakeDamage(TankDamageData damage){
            health.Damage(damage.Damage);
            
            if(status.CurrentHP <= 0){
                OnDeath();
            }
        }
        void OnDeath(){
            ConnectHub.Instance.GetInstance<TankGameManager>().CheckRoundState(p2pInfo.Instance.GetUserIndex(OwnerUserID));
        }
        internal void SwitchClownActive(bool isActivate){
            Crown.SetActive(isActivate);
        }
    }
}
