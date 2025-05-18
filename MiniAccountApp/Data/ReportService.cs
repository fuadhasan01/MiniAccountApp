using Microsoft.Data.SqlClient;
using System.Data;
using MiniAccountApp.Models;

public class ReportService
{
    private readonly IConfiguration _configuration;

    public ReportService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<TrialBalanceEntry> GetTrialBalance(DateTime startDate, DateTime endDate)
    {
        var list = new List<TrialBalanceEntry>();

        using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("dbo.sp_TrialBalanceReport", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@StartDate", startDate);
        cmd.Parameters.AddWithValue("@EndDate", endDate);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new TrialBalanceEntry
            {
                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
                TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit"))
            });
        }

        return list;
    }
}
