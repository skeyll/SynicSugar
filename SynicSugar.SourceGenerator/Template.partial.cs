namespace SynicSugarGenerator {
    public partial class ConnecthubTemplate {
        internal string? SyncList;
        internal string? Register;
        internal string? Reference;
        internal string? PlayeInstance;
        internal string? CommonsInstance;
        internal string? PacketConvert;
        internal bool needSyncSynic;
        internal string? GenerateSynicContainer;
        internal string? SyncedInvoker;
        internal string? SyncedItems;
    }
    public partial class AdditionalPlayerTemplate {
        internal string? NameSpace;
        internal string? ClassName;
        internal string? SyncVar;
        internal string? Rpcs;
        internal bool useGetInstance;
    }
    public partial class AdditionalCommonsTemplate {
        internal string? NameSpace;
        internal string? ClassName;
        internal string? SyncVar;
        internal string? Rpcs;
        internal bool useGetInstance;
    }
    public partial class SynicItemsTemplate {
        internal int hierarchyIndex;
        internal string? items;
    }
}
