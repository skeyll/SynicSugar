using MemoryPack;

namespace SynicSugar {
    [MemoryPackable]
    public partial class UserIdSerializable {

        [MemoryPackIgnore] public readonly UserId userId;
        [MemoryPackInclude] internal string value_s => userId.ToString();


        [MemoryPackConstructor]
        public UserIdSerializable(string value_s)
        {
            userId = UserId.GetUserId(value_s);
        }

        public UserIdSerializable(UserId userid)
        {
            userId = userid;
        }
    }
}