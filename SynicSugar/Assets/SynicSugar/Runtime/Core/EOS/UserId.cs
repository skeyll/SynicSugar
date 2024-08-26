using System.Collections.Generic;
using Epic.OnlineServices;

namespace SynicSugar {
    //MEMO: If we pass this with null in an argument, this will not be null. So, when we use AsEpic to that instance, this returns error.
    public class UserId {
    #region Cache
        static Dictionary<string, UserId> idCache = new();
        internal static void CacheClear(){
            idCache.Clear();
        }
    #endregion

        readonly ProductUserId value;
        readonly string value_s;
        private UserId(ProductUserId id){
            if(id is null){
                return;
            }
            value = id;
            value_s = id.ToString();
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
        public bool Equals(UserId? other) => value is not null && other is not null && value.Equals(other.value);
        public override bool Equals(object? obj){
            return value is not null && obj is not null && obj.GetType() == typeof(UserId) && Equals((UserId)obj);
        }
        public override int GetHashCode() => value.GetHashCode();
        public override string ToString() => value_s;
        public static bool operator ==(in UserId? x, in UserId? y){
            if (x is null && y is null){ return true; }
            if (x is null || y is null) { return false; }
            return x.value.Equals(y.value);
        } 
        public static bool operator !=(in UserId? x, in UserId? y){
            if (x is null && y is null) { return false; }
            if (x is null || y is null) { return true; }
            return !x.value.Equals(y.value);
        }
        #nullable disable
    }
}
