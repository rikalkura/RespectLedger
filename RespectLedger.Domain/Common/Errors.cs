namespace RespectLedger.Domain.Common;

public static class DomainErrors
{
    public static class User
    {
        public const string NotFound = "User not found";
        public const string EmailAlreadyExists = "Email already exists";
        public const string NicknameAlreadyExists = "Nickname already exists";
        public const string InvalidCredentials = "Invalid email or password";
        public const string InactiveAccount = "Account is not active";
        public const string InsufficientMana = "Insufficient mana to give respect";
        public const string SelfRespectNotAllowed = "Cannot give respect to yourself";
        public const string CooldownActive = "Cannot give respect to the same user within 1 hour";
    }

    public static class Respect
    {
        public const string NotFound = "Respect transaction not found";
        public const string InvalidReceiver = "Invalid receiver";
        public const string InvalidSender = "Invalid sender";
    }

    public static class Season
    {
        public const string NotFound = "Season not found";
        public const string NoActiveSeason = "No active season found";
    }

    public static class Achievement
    {
        public const string NotFound = "Achievement not found";
    }
}
