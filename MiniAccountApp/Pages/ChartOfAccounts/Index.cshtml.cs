using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiniAccountApp.Data;
using MiniAccountApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MiniAccountApp.Pages.ChartOfAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ChartOfAccount> Accounts { get; set; } = new List<ChartOfAccount>();

        public async Task OnGetAsync()
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_GetAllChartOfAccounts";
                command.CommandType = CommandType.StoredProcedure;

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Accounts.Add(new ChartOfAccount
                        {
                            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                            AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                            AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                            AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                            ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId"))
                                ? (int?)null
                                : reader.GetInt32(reader.GetOrdinal("ParentAccountId"))
                        });
                    }
                }
            }
        }
    }
}
