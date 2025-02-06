using System;
namespace SynicSugar
{
    internal static class LoggerExtensions
    {
        /// <summary>
        /// Partially masks UserId for secure logging.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>[***...***]</returns>
        internal static string ToMaskedString(this UserId userId) 
        {
            string idStr = userId.ToString();
            int length = idStr.Length;

            if (length <= 7) return idStr;

            return $"[{idStr[..3]}...{idStr[^3..]}]";
        }
        
        /// <summary>
        /// Partially masks id like as LobbyID for secure logging.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>[***...***]</returns>
        internal static string ToMaskedString(this string id) 
        {

            if (string.IsNullOrEmpty(id) || id.Length <= 7) return id;

            return $"[{id[..3]}...{id[^3..]}]";
        }

    #if SYNICSUGAR_PACKETINFO
        /// <summary>
        /// Convert byte to String.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        internal static string ToHexString(this byte[] byteArray) {
            if(byteArray == null){
                return string.Empty;
            }
            System.Text.StringBuilder hex = new System.Text.StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray) {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
        /// <summary>
        /// Convert byte to String.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        internal static string ToHexString(this ArraySegment<byte> byteArray){
            if(byteArray.Count == 0){
                return string.Empty;
            }
            System.Text.StringBuilder hex = new System.Text.StringBuilder(byteArray.Count * 2);
            for (int i = byteArray.Offset; i < byteArray.Offset + byteArray.Count; i++){
                hex.AppendFormat("{0:x2}", byteArray.Array[i]);
            }
            return hex.ToString();
        }
    #endif
    }
}