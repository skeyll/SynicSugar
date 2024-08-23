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
            Result result = await SynicSugarManger.Instance.GetCoreFactory().GetAuthenticationCore().Login("Guest", token);
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
            Result result = await SynicSugarManger.Instance.GetCoreFactory().GetAuthenticationCore().Login(displayName, token);
            SynicSugarManger.Instance.State.IsLoggedIn = result == Result.Success;
            return result;
        }
        /// <summary>
        /// Delete any existing Device ID access credentials for the current user profile on the local device. <br />
        /// On Android and iOS devices, uninstalling the application will automatically delete any local Device ID credentials.<br />
        /// This doesn't means delete User on EOS. We can't delete users from EOS.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTask<Result> DeleteAccount(CancellationToken token = default(CancellationToken)){
            return await SynicSugarManger.Instance.GetCoreFactory().GetAuthenticationCore().DeleteAccount(token);
        }
    }
}