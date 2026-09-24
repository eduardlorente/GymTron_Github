using System.Data.Common;

namespace GymTron.Infrastructure.Persistence;

public static class MySqlConnectionStringHelper
{
    private static readonly HashSet<string> UnsupportedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "AllowBatch", "Allow Batch",
        "CacheServerProperties", "Cache Server Properties",
        "CheckParameters", "Check Parameters",
        "CommandInterceptors", "Command Interceptors",
        "DnsSrv", "Dns-Srv",
        "ExceptionInterceptors", "Exception Interceptors",
        "FunctionsReturnString", "Functions Return String",
        "IncludeSecurityAsserts", "Include Security Asserts",
        "IntegratedSecurity", "Integrated Security",
        "Logging",
        "OldGetStringBehavior",
        "OldSyntax", "Old Syntax", "UseOldSyntax", "Use Old Syntax",
        "ProcedureCacheSize", "Procedure Cache Size", "ProcedureCache", "Procedure Cache",
        "Replication",
        "RespectBinaryFlags", "Respect Binary Flags",
        "SharedMemoryName", "Shared Memory Name",
        "SshHostName", "SshPort", "SshUserName", "SshPassword", "SshKeyFile", "SshPassPhrase",
        "SqlServerMode", "Sql Server Mode",
        "TreatBlobsAsUtf8", "Treat BLOBs as UTF8",
        "UsePerformanceMonitor", "Use Performance Monitor", "UserPerfMon", "PerfMon",
        "UseUsageAdvisor", "Use Usage Advisor", "Usage Advisor"
    };

    public static string Sanitize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        foreach (var key in UnsupportedKeys.Where(builder.ContainsKey))
        {
            builder.Remove(key);
        }

        // MySQL 8 caching_sha2_password authentication requires AllowPublicKeyRetrieval=True
        // when TLS/SSL is not fully verified or not used.
        if (!builder.ContainsKey("AllowPublicKeyRetrieval") && !builder.ContainsKey("allowpublickeyretrieval"))
        {
            builder["AllowPublicKeyRetrieval"] = "True";
        }

        return builder.ConnectionString;
    }
}
