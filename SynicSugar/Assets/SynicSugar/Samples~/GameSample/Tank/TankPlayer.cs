using UnityEngine;
using UnityEngine.UI;
using SynicSugar.P2P;
using MemoryPack;
namespace  SynicSugar.Samples {
    [NetworkPlayer(true)]
    public partial class TankPlayer : MonoBehaviour {
        public enum CurrentState{
            Stay, Move
        }
        CurrentState currentState;
        PositionInfo positionInfo;
        [SerializeField] Text playerName;
        void Start(){
            positionInfo = new PositionInfo(){
                playerPos = this.transform.position,
                direction = PositionInfo.Direction.Neutral
            };
        }
        void Update(){
            //Change local player state
            if(isLocal){
                if(!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))){
                    if(currentState == CurrentState.Move){
                        Stop();
                    }
                    return;
                    
                }
                if(currentState == CurrentState.Stay){
                    Move();
                }
                PositionInfo.Direction newDirection = positionInfo.direction;
                if(Input.GetKey(KeyCode.W)){
                    newDirection = PositionInfo.Direction.Up;
                }else if(Input.GetKey(KeyCode.A)){
                    newDirection = PositionInfo.Direction.Left;
                }else if(Input.GetKey(KeyCode.S)){
                    newDirection = PositionInfo.Direction.Down;
                }else if(Input.GetKey(KeyCode.D)){
                    newDirection = PositionInfo.Direction.Right;
                }
                //Need "new()" to Sync.
                //But new Vector3() is heacy, so instantiate only on changing direction.
                //In other player client, instance every three seconds (call syncVar).
                if(newDirection != positionInfo.direction){
                    PositionInfo newPosInfo = new PositionInfo(){
                        playerPos = this.transform.position,
                        direction = newDirection
                    };
                    UpdateCurrentDirection(newPosInfo);
                }
            }

            if(currentState == CurrentState.Move){
                float moveDelta = 5 * Time.deltaTime;
                //Sing
                moveDelta *= positionInfo.direction == PositionInfo.Direction.Up || positionInfo.direction == PositionInfo.Direction.Right ? 1 : -1;

                if(positionInfo.direction == PositionInfo.Direction.Up || positionInfo.direction == PositionInfo.Direction.Down){
                    this.transform.position += new Vector3(0f, 0f, moveDelta);
                }else{
                    this.transform.position += new Vector3(moveDelta, 0f, 0f);
                }
            }
        }

        [Rpc]
        public void Move(){
            currentState = CurrentState.Move;
        }
        [Rpc]
        public void Stop(){
            currentState = CurrentState.Stay;
            positionInfo.direction = PositionInfo.Direction.Neutral;
        }
        [Rpc]
        public void UpdateCurrentDirection(PositionInfo posInfo){
            positionInfo.direction = posInfo.direction;
            positionInfo.playerPos = new Vector3(posInfo.playerPos.x, posInfo.playerPos.y, posInfo.playerPos.z);
        }
        public void SetNameText(string name){
            playerName.text = name;
        }
    }
    [MemoryPackable]
    public partial class PositionInfo{
        public Vector3 playerPos;
        public enum Direction{
            Neutral, Up, Left, Down, Right
        }
        public Direction direction;
    }
}