namespace CedarDelivery.Constants
{
    // Role names used across the app. Keep them in one place to avoid typos.
    // Everyone signs up as a Customer; Merchant and Driver are granted by an Admin
    // after a RoleApplication is approved.
    public class Roles
    {
        public const string Admin = "Admin";
        public const string Merchant = "Merchant";
        public const string Driver = "Driver";
        public const string Customer = "Customer";

        public static readonly string[] All = { Admin, Merchant, Driver, Customer };
    }
}
