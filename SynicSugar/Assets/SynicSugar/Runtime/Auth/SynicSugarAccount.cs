using System.Threading;
using Cysharp.Threading.Tasks;

namespace SynicSugar.Auth {
    public static class SynicSugarAccount {
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