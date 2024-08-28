using System.Threading;
using Cysharp.Threading.Tasks;

namespace SynicSugar.Auth {
    public static class SynicSugarAuthentication {
        /// <summary>
        /// Login with DeviceID. If success, return true. <br />
        /// We can't use DeviceId directly for security. This id is saved secure pos like as Keystore.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<Result> Login(CancellationToken token = default(CancellationToken)){
            Result result = await SynicSugarManger.Instance.CoreFactory.GetAuthenticationCore().Login("Guest", token);
            SynicSugarManger.Instance.State.IsLoggedIn = result == Result.Success;
            return result;
        }

        /// <summary>
        /// Login with DeviceID. If success, return true. <br />
        /// We can't use DeviceId directly for security. This id is saved secure pos like as Keystore.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<Result> Login(string displayName, CancellationToken token = default(CancellationToken)){
            Result result = await SynicSugarManger.Instance.CoreFactory.GetAuthenticationCore().Login(displayName, token);
            SynicSugarManger.Instance.State.IsLoggedIn = result == Result.Success;
            return result;
        }
    }
}