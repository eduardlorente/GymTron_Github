using GymTron.Infrastructure.Persistence;
using Xunit;

namespace GymTron.UnitTests.Infrastructure;

public class MySqlConnectionStringHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_WhenNullOrWhitespace_ReturnsEmpty(string? input)
    {
        var result = MySqlConnectionStringHelper.Sanitize(input);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sanitize_WhenMissingAllowPublicKeyRetrieval_AppendsItAsTrue()
    {
        var input = "Server=localhost;Database=gymtron;Uid=root;Pwd=secret;";
        var result = MySqlConnectionStringHelper.Sanitize(input);

        Assert.Contains("AllowPublicKeyRetrieval=True", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Server=localhost", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=gymtron", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_WhenAllowPublicKeyRetrievalAlreadyExplicitlySet_PreservesValue()
    {
        var input = "Server=localhost;Database=gymtron;Uid=root;Pwd=secret;AllowPublicKeyRetrieval=False;";
        var result = MySqlConnectionStringHelper.Sanitize(input);

        Assert.Contains("AllowPublicKeyRetrieval=False", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AllowPublicKeyRetrieval=True", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_RemovesLegacyOracleMySqlDataUnsupportedKeys()
    {
        var input = "Server=remote.db.com;Database=gymtron;Uid=u;Pwd=p;AllowBatch=true;CheckParameters=false;ProcedureCacheSize=25;Logging=true;UseUsageAdvisor=true;";
        var result = MySqlConnectionStringHelper.Sanitize(input);

        Assert.DoesNotContain("AllowBatch", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CheckParameters", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProcedureCacheSize", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Logging", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UseUsageAdvisor", result, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Server=remote.db.com", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AllowPublicKeyRetrieval=True", result, StringComparison.OrdinalIgnoreCase);
    }
}
