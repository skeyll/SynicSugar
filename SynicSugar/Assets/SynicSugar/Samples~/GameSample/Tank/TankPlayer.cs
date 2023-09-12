using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SynicSugar.P2P;
using System;
using Epic.OnlineServices;
namespace  SynicSugar.Samples {
    [NetworkPlayer(true)]
    public partial class TankPlayer : MonoBehaviour {
        public enum CurrentState{
            Stay, Move
        }
        CurrentState currentState;
        public enum Direction{
            Up, Left, Down, Right, Neutral
        }
        public enum Key{
            W, A, S, D
        }
        Direction direction = Direction.Neutral;
        //Fix to exact position every 3 seconds.
        [SyncVar(3000)] Vector3 playerPos;

        [SerializeField] Text playerName;
        void Start(){
            //In local, register player event to EventTrigger
            if(isLocal){
                RegisterButtonEvents();
            }
        }
        void RegisterButtonEvents(){
            GameObject padParent = GameObject.Find("UICanvas").gameObject.transform.Find("InputPads").gameObject;

            foreach(var k in Enum.GetValues(typeof(Key))){
                Key tmp = (Key)k;
                RegisterEventTrigger(padParent, tmp);
            }
        }
        void RegisterEventTrigger(GameObject parent, Key key){
            GameObject keyObject = parent.transform.Find(key.ToString()).gameObject;
            keyObject.AddComponent<EventTrigger>();

            EventTrigger trigger = keyObject.GetComponent<EventTrigger>();

            EventTrigger.Entry move = new EventTrigger.Entry();
            move.eventID = EventTriggerType.PointerDown;
            move.callback.AddListener((d) => {Move((int)key);});
            trigger.triggers.Add(move);

            EventTrigger.Entry stop = new EventTrigger.Entry();
            stop.eventID = EventTriggerType.PointerUp;
            stop.callback.AddListener((d) => {Stop();});
            trigger.triggers.Add(stop);
        }
        void Update(){
            if(currentState == CurrentState.Move){
                //Reflects current value
                playerPos = this.transform.position;

                float moveDelta = 5 * Time.deltaTime;
                //+ or -?
                moveDelta *= direction == Direction.Up || direction == Direction.Right ? 1 : -1;

                if(direction == Direction.Up || direction == Direction.Down){
                    playerPos += new Vector3(0f, 0f, moveDelta);
                }else{
                    playerPos += new Vector3(moveDelta, 0f, 0f);
                }

                this.transform.position = playerPos;
            }
        }

        [Rpc]
        public void Move(int direction){
            currentState = CurrentState.Move;
            this.direction = (Direction)direction;
        }
        [Rpc]
        public void Stop(){
            currentState = CurrentState.Stay;
            direction = Direction.Neutral;
        }
        public void SetNameText(string name){
            playerName.text = name;
        }
    }
}
