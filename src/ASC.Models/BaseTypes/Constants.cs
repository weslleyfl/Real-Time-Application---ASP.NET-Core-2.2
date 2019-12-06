namespace ASC.Models.BaseTypes
{
    /// <summary>
    ///  A developer should be aware of the practice that all the environment-related settings    
    ///  should go into appsettings.json, and any setting related to C# code should go into the Constants class.
    ///  Constant values are useful in creating where clause ocnditions for Azure Table Storage.
    /// </summary>
    public static class Constants
    {
        public const string Equal = "eq";
        public const string NotEqual = "ne";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "ge";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "le";
    }

    public enum Roles
    {
        Admin,
        Engineer,
        User
    }

    public enum MasterKeys
    {
        VehicleName,
        VehicleType,
        PromotionType
    }

    public enum Status
    {
        New,
        Denied,
        Pending,
        Initiated,
        InProgress,
        PendingCustomerApproval,
        RequestForInformation,
        Completed
    }
}
