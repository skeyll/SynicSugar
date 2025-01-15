using MemoryPack;

namespace SynicSugar {
    internal class UserIdFormatter : MemoryPackFormatter<UserId>
    {
        public override void Serialize(ref MemoryPackWriter writer, ref UserId value)
        {
            if (value == null || !value.IsValid()){
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteString(value.ToString());
        }

        public override void Deserialize(ref MemoryPackReader reader, ref UserId value)
        {
            if (reader.PeekIsNull())
            {
                value = null;
                return;
            }
            
            var value_s = reader.ReadString();
            value = UserId.GetUserId(value_s);
        }
    }
}