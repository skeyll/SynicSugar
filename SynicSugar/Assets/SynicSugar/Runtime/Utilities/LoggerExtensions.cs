namespace SynicSugar
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Partially masks UserId for secure logging.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>[***...***]</returns>
        public static string MaskUserId(this UserId userId) 
        {
            string idStr = userId.ToString();
            int length = idStr.Length;

            if (length <= 6) return idStr;

            return $"[{idStr.Substring(0, 3)}...{idStr.Substring(length - 3)}]";
        }
    }
}