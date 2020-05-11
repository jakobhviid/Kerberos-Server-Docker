namespace app_api.V1.Contracts
{
    public static class ErrorMessages
    {
        public const string UserAlreadyExists = "User Already exists with a keytab";
        public const string NewServiceNameRequired = "'newServiceName' required!";
        public const string ServiceAlreadyExists = "Service already exists with a keytab";
        public const string AdminPasswordInCorrect = "Admin Password incorrect";
        public const string UserDoesNotExist = "User does not exist, contact an administrator to get a user and a kerberos keytab";
        public const string UserNameValidationError = "Username cannot contain special characters or spaces";
        public const string ServiceNameValidationError = "Service name cannot contain special characters or spaces, except . and /";
    }
}