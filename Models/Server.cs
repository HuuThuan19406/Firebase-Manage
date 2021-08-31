using FirebaseAdmin;
using FirebaseAdmin.Auth;
using System.Collections.Generic;
using System.Linq;

namespace FirebaseManage.Models
{
    static class Server
    {
        internal static FirebaseApp FirebaseApp;
        internal static readonly FirebaseAuth FirebaseAuth = FirebaseAuth.DefaultInstance;
        internal static Dictionary<string, string> NextPageToken = new Dictionary<string, string>();
        internal static Dictionary<string, string> CurrentPageToken = new Dictionary<string, string>();

        internal static IEnumerable<dynamic> ToUsers(this IEnumerator<ExportedUserRecord> enumerator)
        {
            for (int i = 1; enumerator.MoveNext(); i++)
            {
                yield return new
                {
                    No = i,
                    User = enumerator.Current,
                    UserRole = enumerator.Current.CustomClaims.Keys.FirstOrDefault()
                };
            }
        }
    }
}
