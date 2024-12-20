using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
namespace SynicSugar.Samples.Tank
{
    [NetworkPlayer]
    public partial class TankMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody m_Rigidbody;           // Reference used to move the tank.
        [SerializeField] private ParticleSystem m_LeftDustTrail;  // The particle system of dust that is kicked up from the left track.
        [SerializeField] private ParticleSystem m_RightDustTrail; // The particle system of dust that is kicked up from the rightt track.
        private float m_Speed = 12f;             // How fast the tank moves forward and back.
        public float m_TurnSpeed = 60f; // How fast the tank turns in degrees per second.

        // Movement is managed by Move and Stop and calculated locally.
        // Periodically, this value is sent to correct the actual position.
        [SyncVar(2000)] public Vector3 correctedPlayerPosition;
        // Movement is managed by Move and Stop and calculated locally.
        // Periodically, this value is sent to correct the actual position.
        [SyncVar(1000)] public Quaternion correctedPlayerQuaternion;
        /// Movement is managed by UniTask.
        /// </summary>
        private CancellationTokenSource moveTokenSource;
        private void Start()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        private void OnDisable()
        {
            Stop();
        }
        /// <summary>
        /// Call this from TankPlayer after receiving basic data.
        /// </summary>
        /// <param name="speed"></param>
        internal void SetSpeed(float speed)
        {
            m_Speed = speed;
        }
        /// <summary>
        /// Keep moving in loop until player press Stop.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async UniTask Move(TankMoveData data)
        {
            GenerateNewToken();
            SetTransformData(data);

            int sign = data.direction is Direction.Up ? 1 : -1;
            Vector3 movement = sign * m_Rigidbody.transform.forward * m_Speed;

            while(!moveTokenSource.Token.IsCancellationRequested)
            {
                //Fix the pos by true pos.
                //The actual pos data is sent every 2 seconds from each local.
                m_Rigidbody.MovePosition(correctedPlayerPosition + movement * Time.deltaTime);
                m_Rigidbody.angularVelocity = Vector3.zero;
                
                correctedPlayerPosition = m_Rigidbody.position;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, moveTokenSource.Token);
            }
        }
        /// <summary>
        /// Turn toward the new direction.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal async UniTask Turn (TankMoveData data)
        {
            GenerateNewToken();
            SetTransformData(data);

            float turnSpeed = data.direction is Direction.Right ? m_TurnSpeed : -m_TurnSpeed;

            while (!moveTokenSource.Token.IsCancellationRequested)
            {
                Quaternion turn = Quaternion.Euler(0f, turnSpeed * Time.deltaTime, 0f);
                m_Rigidbody.MoveRotation(correctedPlayerQuaternion * turn);
                m_Rigidbody.velocity = Vector3.zero;

                correctedPlayerQuaternion = m_Rigidbody.rotation;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, moveTokenSource.Token);
            }
        }

        /// <summary>
        /// Stop player movement task.
        /// </summary>
        internal void Stop()
        {
            if(moveTokenSource != null && moveTokenSource.Token.CanBeCanceled)
            {
                moveTokenSource.Cancel();
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }
        private void SetTransformData(TankMoveData data)
        {
            correctedPlayerPosition = data.currentPosition;
            correctedPlayerQuaternion = data.currentQuaternion;
        }
        private void GenerateNewToken()
        {
            Stop();
            moveTokenSource = new CancellationTokenSource();
        }
        // called at the start of each round to make sure each tank is set up correctly.
        internal void SetDefaults()
        {
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_LeftDustTrail.Clear();
            m_LeftDustTrail.Play();

            m_RightDustTrail.Clear();
            m_RightDustTrail.Play();
        }
    }
}