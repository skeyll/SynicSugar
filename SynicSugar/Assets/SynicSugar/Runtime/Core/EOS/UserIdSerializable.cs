using MemoryPack;

namespace SynicSugar {
    [MemoryPackable]
    internal partial class UserIdSerializable {

        [MemoryPackIgnore] internal UserId userId;
        [MemoryPackInclude] internal string value_s => userId.ToString();

        /// <summary>
        /// For formatter's constructer.
        /// </summary>
        public UserIdSerializable(){}
        /// <summary>
        /// For MemoryPack.
        /// </summary>
        /// <param name="value_s"></param>
        [MemoryPackConstructor]
        public UserIdSerializable(string value_s)
        {
            userId = UserId.GetUserId(value_s);
        }

        /// <summary>
        /// For serilaizer.
        /// </summary>
        /// <param name="userId"></param>
        internal UserIdSerializable SetValue(UserId userId)
        {
            this.userId = userId;
            return this;
        }
    }
}