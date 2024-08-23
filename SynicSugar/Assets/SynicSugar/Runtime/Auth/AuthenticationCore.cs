using System.Threading;
using Cysharp.Threading.Tasks;

namespace SynicSugar.Auth.Base {
    public abstract class AuthenticationCore {
        /// <summary>
        /// Basic login function. Should be an anonymous way that does not require an email address or explicit login of the user.
        /// </summary>
        /// <param name="displayName">name or id</param>
        /// <param name="token">token for this task</param>
        /// <returns>Return true if user can login to the server.</returns>
        public abstract UniTask<Result> Login(string displayName, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// To delete user acount for Basic login function.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <returns>Return true if user can login to the server.</returns>
        public abstract UniTask<Result> DeleteAccount(CancellationToken token = default(CancellationToken));
    }
}