namespace NPMAPI.App_Start
{
    public static class GlobalVariables
    {
        public static string auth_id
        {
            get
            {
                return "37b091d7-5237-5999-1ad5-3aef4dcc9a8d";
            }
        }
        public static string auth_token
        {
            get
            {
                return "YijVqD9NCHVYv4HT8qGx";
            }
        }
        public static string candidates
        {
            get
            {
                return "10";
            }
        }
        public static string ValidateAddressBaseUrl
        {
            get
            {
                return "https://us-street.api.smartystreets.com/street-address";
            }
        }
        public static double JWTTokenDuration
        {
            get
            {
                return 30;
            }
        }
        public static double RefreshTokenDuration
        {
            get
            {
                return 1;
            }
        }
        public static string JWTSecret
        {
            get
            {
                return "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
            }
        }
        public static long MaximumPatientPictureSize
        {
            get
            {
                return 2097152;
            }
        }
        /// <summary>
        /// 20 mb maximum file size
        /// </summary>
        public static long MaximumPatientAttachmentSize
        {
            get
            {
                return 20971520;
            }
        }
    }
}