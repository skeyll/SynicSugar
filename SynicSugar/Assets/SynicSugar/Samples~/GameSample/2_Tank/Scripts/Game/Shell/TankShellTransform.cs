using MemoryPack;
using UnityEngine;
namespace SynicSugar.Samples.Tank 
{
    [MemoryPackable]
    public partial class TankShellTransform 
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 forward;

        [MemoryPackConstructor]
        public TankShellTransform(Vector3 position, Quaternion rotation, Vector3 forward) 
        {
            this.position = position;
            this.rotation = rotation;
            this.forward = forward;
        }
        internal TankShellTransform(Transform shellTransform)
        {
            position = shellTransform.position;
            rotation = shellTransform.rotation;
            forward = shellTransform.forward;
        }
    }
}