using Conference.Functions.Models;

namespace Conference.Functions.Services
{
    public static class ValidationService
    {
        public static bool TryValidateRegistration(Registration reg, out string error)
        {
            error = null;
            if (reg == null)
            {
                error = "registration is null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(reg.Name))
            {
                error = "name is required";
                return false;
            }
            if (string.IsNullOrWhiteSpace(reg.Email))
            {
                error = "email is required";
                return false;
            }
            if (string.IsNullOrWhiteSpace(reg.Affiliation))
            {
                error = "affiliation is required";
                return false;
            }
            return true;
        }
    }
}