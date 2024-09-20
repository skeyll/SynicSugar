using System.Collections.Generic;
using Epic.OnlineServices;

namespace SynicSugar {
    public class UserId {
    #region Cache
        static Dictionary<string, UserId> idCache = new();
        internal static void CacheClear(){
            idCache.Clear();
        #if UNITY_EDITOR
            if(!UnityEngine.Application.isPlaying){
                return;
            }
        #endif
            //Set LocalUserid to the cache.
            string localUserIdString = SynicSugarManger.Instance.LocalUserId.ToString();
            if(localUserIdString != OFFLINE_USERID){
                idCache.Add(localUserIdString, SynicSugarManger.Instance.LocalUserId);
            }
        }
    #endregion
        const string OFFLINE_USERID = "OFFLINEUSER";
        readonly ProductUserId value;
        readonly string value_s;
        private UserId(ProductUserId id){
            if(!id.IsValid()){
                return;
            }
            value = id;
            value_s = id.ToString();
        }
        /// <summary>
        /// *Experimental. <br />
        /// For offline mode.
        /// </summary>
        /// <returns></returns>
        internal static UserId GenerateOfflineUserId(){
            return new UserId();
        }
        /// <summary>
        /// Invalid UserId for offline.
        /// </summary>
        private UserId(){
            value = null;
            value_s = OFFLINE_USERID;
        }
        /// <summary>
        /// Reconencter creates userid list 
        /// </summary>
        /// <param name="id">String to be got from Host.</param>
        /// <returns></returns>
        internal static UserId GenerateFromStringForReconnecter(string id){
            if(idCache.ContainsKey(id)){
                return idCache[id];
            }
            UserId obj = new UserId(ProductUserId.FromString(id));
            idCache.Add(id, obj);
            return obj;
        }
        /// <summary>
        /// We can only create a new UserID Instance from Epic's product UserID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> <summary>
        internal static UserId GetUserId(ProductUserId id){
            string s = id.ToString();
            if(idCache.ContainsKey(s)){
                return idCache[s];
            }
            UserId obj = new UserId(id);
            idCache.Add(s, obj);
            return obj;
        }
        public static UserId GetUserId(UserId id){
            string s = id.ToString();
            if(idCache.ContainsKey(s)){
                return idCache[s];
            }
            if(s is OFFLINE_USERID){
                return SynicSugarManger.Instance.LocalUserId;
            }
            return null;
        }
        public static UserId GetUserId(string id){
            if(idCache.ContainsKey(id)){
                return idCache[id];
            }
            return null;
        }
        /// <summary>
        /// For library user to use internal static UserId GetUserId(ProductUserId id). <br />
        /// GetUserId(ProductUserId id) creates catch, so doesn't open as public.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        public static UserId ToUserId(ProductUserId id){
            string key = id.ToString();
            if(idCache.ContainsKey(key)){
                return idCache[key];
            }
            return null;
        }
        public ProductUserId AsEpic => value;

        public static explicit operator ProductUserId(UserId id) => GetUserId(id).value;
        public static explicit operator UserId(ProductUserId value) => ToUserId(value);
        #nullable enable
        public bool Equals(UserId? other){
            if(other is null) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            if(value is not null && other.value is not null ) { return value.Equals(other.value); }

            return value_s == other?.value_s;
        }
        public override bool Equals(object? obj){
            if(obj is null){ return false; }
            if(ReferenceEquals(this, obj)) { return true; }
            if(obj.GetType() != typeof(UserId)){ return false; }

            return Equals((UserId)obj);
        }
        public override int GetHashCode() => value.GetHashCode();
        /// <summary>
        /// Return UserId value as string from catch.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => value_s;

        /// <summary>
        /// Check whether UserId values are the same or not.
        /// Basically, this is compared by the original ProductUserId, then for offline, the comparison is done with strings.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Return true when UserId is the same with other.</returns>
        public static bool operator ==(in UserId? x, in UserId? y){
            if (ReferenceEquals(x, y)) { return true; }
            if (x is null && y is null) { return true; }
            if (x is null || y is null) { return false; }

            if (x.value is not null && y.value is not null){ return x.value.Equals(y.value); }

            return x.value_s == y.value_s;
        }

        /// <summary>
        /// Check whether UserId values are the same or not.
        /// Basically, this is compared by the original ProductUserId, then for offline, the comparison is done with strings.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Return true when UserId is NOT the same with other.</returns>
        public static bool operator !=(in UserId? x, in UserId? y){
            if (ReferenceEquals(x, y)) { return false; }
            if (x is null && y is null) { return false; }
            if (x is null || y is null) { return true; }

            if (x.value is not null && y.value is not null){ return !x.value.Equals(y.value); }

            return x.value_s != y.value_s;
        }
        #nullable disable
    }
}
