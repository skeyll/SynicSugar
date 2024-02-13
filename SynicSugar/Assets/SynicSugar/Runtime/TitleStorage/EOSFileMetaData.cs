using MemoryPack;

namespace SynicSugar.TitleStorage {
    /// <summary>
    /// Use this with file name key as Dictionary.
    /// </summary>
    [MemoryPackable]
    public partial class EOSFileMetaData {
        public string MD5Hash;
        public bool NeedUpdate;
        public uint SizeByte;
        public EOSFileMetaData(string md5hash, bool needupdate,uint sizebyte){
            MD5Hash = md5hash;
            NeedUpdate = needupdate;
            SizeByte = sizebyte;
        }
    }
}