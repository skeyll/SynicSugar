using UnityEngine;
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;
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
            this.transform.position = status.RespawnPos;

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
        /// <param name="direction">Up or Down</param>
        public void Move(Direction direction){
            //Simplified because it's hard work. Sound only locally.
            TankAudioManager.Instance.PlayTankClip(TankClips.Driving);

            if(direction is Direction.Up or Direction.Down){
                Move(new TankMoveData(direction, transform));
            }else{
                Turn(new TankMoveData(direction, transform));
            }

        }
        [Rpc]
        public void Move(TankMoveData data){
            movement.Move(data).Forget();
        }
        [Rpc]
        public void Turn(TankMoveData data){
            movement.Turn(data).Forget();
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

        internal void ActivatePlayer(){
            gameObject.SetActive(true);
        }
        void OnDeath(){
            ConnectHub.Instance.GetInstance<TankGameManager>().CheckRoundState(p2pInfo.Instance.GetUserIndex(OwnerUserID));
        }
        internal void SwitchClownActive(bool isActivate){
            Crown.SetActive(isActivate);
        }
    }
}