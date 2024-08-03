using UnityEngine;
using MemoryPack;
namespace SynicSugar.Samples.Tank {
    [MemoryPackable]
    public partial class TankMoveData {
        public Direction direction;
        public Vector3 currentPosition;
        public Quaternion currentQuaternion;

        [MemoryPackConstructor]
        public TankMoveData (Direction direction, Vector3 currentPosition, Quaternion currentQuaternion) {
            this.direction = direction;
            this.currentPosition = currentPosition;
            this.currentQuaternion = currentQuaternion;
        }
        public TankMoveData (Direction direction, Transform tankTransform){
            this.direction = direction;
            currentPosition = tankTransform.position;
            currentQuaternion = tankTransform.rotation;
        }
    }
}