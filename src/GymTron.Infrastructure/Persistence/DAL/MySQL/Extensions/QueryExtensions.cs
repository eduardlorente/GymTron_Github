namespace GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;

internal static class QueryExtensions
{


    public static string ToReadUncommited(this string query)
        => $@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
              {query}
              SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;";
}
