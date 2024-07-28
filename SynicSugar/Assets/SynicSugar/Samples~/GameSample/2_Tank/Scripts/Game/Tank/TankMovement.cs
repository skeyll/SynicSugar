using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
namespace SynicSugar.Samples.Tank {
    [NetworkPlayer]
    public partial class TankMovement : MonoBehaviour {
        [SerializeField] Rigidbody m_Rigidbody;           // Reference used to move the tank.
        [SerializeField] ParticleSystem m_LeftDustTrail;  // The particle system of dust that is kicked up from the left track.
        [SerializeField] ParticleSystem m_RightDustTrail; // The particle system of dust that is kicked up from the rightt track.
        float m_Speed = 12f;             // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f; // How fast the tank turns in degrees per second.

        [Synic(0)] public Direction currentDirection;
        // Movement is managed by Move and Stop and calculated locally.
        // Periodically, this value is sent to correct the actual position.
        [SyncVar(3000)] public Vector3 truePlayerPosition;
        /// Movement is managed by UniTask.
        /// </summary>
        CancellationTokenSource moveTokenSource;
        void Start() {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        void OnDisable(){
            Stop();
        }
        /// <summary>
        /// Call this from TankPlayer after receiving basic data.
        /// </summary>
        /// <param name="speed"></param>
        internal void SetSpeed(float speed){
            m_Speed = speed;
        }
        /// <summary>
        /// Keep moving in loop until player press Stop.
        /// </summary>
        /// <param name="newDirection"></param>
        /// <returns></returns>
        internal async UniTask Move(Direction newDirection){
            GenerateNewToken();
            if(currentDirection != newDirection){
                await Turn(currentDirection, newDirection, moveTokenSource.Token);
            }

            int orientation = newDirection == Direction.Up || newDirection == Direction.Right ? 1 : -1;
            float moveRate = orientation * m_Speed;

            while(!moveTokenSource.Token.IsCancellationRequested){
                //Calc move amount in this frame.
                float moveDelta = moveRate * Time.deltaTime;

                //Fix the pos by true pos.
                //The actual pos data is sent every 3 seconds from each local.
                if(!isLocal){
                    m_Rigidbody.MovePosition(truePlayerPosition);
                }

                Vector3 newPosition = m_Rigidbody.position;
                //Add this frame movement
                if(newDirection == Direction.Up || newDirection == Direction.Down){
                    newPosition += new Vector3(0f, 0f, moveDelta);
                }else{
                    newPosition += new Vector3(moveDelta, 0f, 0f);
                }
                m_Rigidbody.MovePosition(newPosition);
                
                truePlayerPosition = newPosition;
                await UniTask.Yield(moveTokenSource.Token);
            }
        }
        /// <summary>
        /// Turn toward the new direction.
        /// </summary>
        /// <param name="currentAngle"></param>
        /// <param name="newAngle"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask Turn (Direction currentAngle, Direction newAngle, CancellationToken token){
            float rotationAmount = CalculateRotationAngle(currentAngle, newAngle);
            float currentRotation = 0f;

            while (Mathf.Abs(currentRotation) < Mathf.Abs(rotationAmount)){
                if (token.IsCancellationRequested) break;

                float turn = Mathf.Sign(rotationAmount) * m_TurnSpeed * Time.deltaTime;
                currentRotation += turn;

                if (Mathf.Abs(currentRotation) > Mathf.Abs(rotationAmount)){
                    turn = rotationAmount - (currentRotation - turn);
                    currentRotation = rotationAmount;
                }

                Quaternion inputRotation = Quaternion.Euler(0f, turn, 0f);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * inputRotation);

                await UniTask.Yield(token);
            }
        }
        /// <summary>
        /// Calc the amount needed for rotation.
        /// </summary>
        /// <param name="currentAngle"></param>
        /// <param name="newAngle"></param>
        /// <returns></returns>
        float CalculateRotationAngle(Direction currentAngle, Direction newAngle){
            float angleDelta = DirectionAngles.GetValue(newAngle) - DirectionAngles.GetValue(currentAngle);

            //Normalize the angle difference to be within the range [-180, 180]
            if (angleDelta < -180) angleDelta += 360;
            if (angleDelta > 180) angleDelta -= 360;
            
            return angleDelta;
        }

        /// <summary>
        /// Stop player movement task.
        /// </summary>
        internal void Stop(){
            if(moveTokenSource != null && moveTokenSource.Token.CanBeCanceled){
                moveTokenSource.Cancel();
            }
        }
        void GenerateNewToken(){
            Stop();
            moveTokenSource = new CancellationTokenSource();
        }
        // his function is called at the start of each round to make sure each tank is set up correctly.
        internal void SetDefaults(){
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_LeftDustTrail.Clear();
            m_LeftDustTrail.Stop();

            m_RightDustTrail.Clear();
            m_RightDustTrail.Stop();
        }
        
        public void ReEnableParticles(){
            m_LeftDustTrail.Play();
            m_RightDustTrail.Play();
        }
    }
}