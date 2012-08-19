using ConnorsNetworkingSuite;

namespace BukkitServiceAPI.Authentication {
    public delegate NewClientCredentials AuthenticationMethod(NetStream clientStream);

    public class CustomAuthentication {
        private static AuthenticationMethod authMethod;

        public static void OverrideAuthenticationMethod(AuthenticationMethod auth) {
            authMethod = auth;
        }

        internal static NewClientCredentials CustomAuth(NetStream client) {
            return authMethod == null ? null : authMethod(client);
        }
    }
    public class NewClientCredentials {
        public bool Successful { get; set; }
        public string Username { get; set; }
        public int SecurityLevel { get; set; }
    }
}
