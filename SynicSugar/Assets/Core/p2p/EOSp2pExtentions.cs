using System;

namespace SynicSugar.P2P {
    public static class EOSp2pExtenstions {
        /// <summary>
        /// Generate SocketName for p2p safety connect.
        /// </summary>
        /// <returns></returns>
        internal static string GenerateRandomSocketName(){
            string charSets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] resultChar = new char[28];
            var random = new System.Random();

            for (int i = 0; i < resultChar.Length; i++){
                resultChar[i] = charSets[random.Next(charSets.Length)];
            }
            string result = new String(resultChar) + "null";
            return result;
        }
    }
}
