using MemoryPack;

namespace SynicSugar {
    public class UserIdFormatter : MemoryPackFormatter<UserId>
    {
        public override void Serialize(ref MemoryPackWriter writer, ref UserId? value)
        {
            if (value == null || !value.IsValid()){
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WritePackable(new UserIdSerializable(value));
        }

        public override void Deserialize(ref MemoryPackReader reader, ref UserId? value)
        {
            if (reader.PeekIsNull())
            {
                value = null;
                return;
            }
            
            var wrapped = reader.ReadPackable<UserIdSerializable>();
            value = UserId.GetUserId(wrapped.value_s);
        }
    }
}