namespace ASC.Models.BaseTypes
{
    /// <summary>
    ///  A developer should be aware of the practice that all the environment-related settings    
    ///  should go into appsettings.json, and any setting related to C# code should go into the Constants class.
    /// </summary>
    public static class Constants
    {
    }

    public enum Roles
    {
        Admin,
        Engineer,
        User
    }
}
