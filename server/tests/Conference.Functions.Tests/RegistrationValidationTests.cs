using Conference.Functions.Models;
using Conference.Functions.Services;
using Xunit;

namespace Conference.Functions.Tests
{
    public class RegistrationValidationTests
    {
        [Fact]
        public void ValidRegistration_PassesValidation()
        {
            var reg = new Registration { Name = "Alice", Email = "a@b.com", Affiliation = "Uni" };
            var ok = ValidationService.TryValidateRegistration(reg, out var err);
            Assert.True(ok);
            Assert.Null(err);
        }

        [Fact]
        public void MissingName_FailsValidation()
        {
            var reg = new Registration { Name = "", Email = "a@b.com", Affiliation = "Uni" };
            var ok = ValidationService.TryValidateRegistration(reg, out var err);
            Assert.False(ok);
            Assert.Equal("name is required", err);
        }

        [Fact]
        public void MissingAffiliation_FailsValidation()
        {
            var reg = new Registration { Name = "Bob", Email = "b@c.com", Affiliation = "" };
            var ok = ValidationService.TryValidateRegistration(reg, out var err);
            Assert.False(ok);
            Assert.Equal("affiliation is required", err);
        }
    }
}