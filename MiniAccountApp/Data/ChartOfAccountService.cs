using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountApp.Models;

namespace MiniAccountApp.Services
{
    public class ChartOfAccountService
    {
        private readonly IConfiguration _configuration;

        public ChartOfAccountService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task CreateAsync(ChartOfAccount account)
        {
            // Generate AccountCode based on hierarchy
            account.AccountCode = await GenerateAccountCodeAsync(account.ParentAccountId);

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "INSERT");
            cmd.Parameters.AddWithValue("@AccountCode", account.AccountCode);
            cmd.Parameters.AddWithValue("@AccountName", account.AccountName);
            cmd.Parameters.AddWithValue("@AccountType", account.AccountType);
            cmd.Parameters.AddWithValue("@ParentAccountId", (object?)account.ParentAccountId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountLevel", account.AccountLevel);
            cmd.Parameters.AddWithValue("@IsActive", account.IsActive);
            cmd.Parameters.AddWithValue("@AccountNature", (object?)account.AccountNature ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)account.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)account.Description ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<string> GenerateAccountCodeAsync(Guid? parentAccountId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            if (parentAccountId == null)
            {
                var cmd = new SqlCommand("SELECT MAX(CAST(AccountCode AS INT)) FROM ChartOfAccounts WHERE ParentAccountId IS NULL", conn);
                var result = await cmd.ExecuteScalarAsync();
                int maxCode = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                return (maxCode + 1).ToString();
            }
            else
            {
                var cmdParent = new SqlCommand("SELECT AccountCode FROM ChartOfAccounts WHERE AccountId = @ParentAccountId", conn);
                cmdParent.Parameters.AddWithValue("@ParentAccountId", parentAccountId);
                var parentCodeObj = await cmdParent.ExecuteScalarAsync();

                if (parentCodeObj == null)
                    throw new Exception("Parent account not found");

                string parentCode = parentCodeObj.ToString();

                var cmdChild = new SqlCommand(
                    "SELECT MAX(CAST(AccountCode AS INT)) FROM ChartOfAccounts WHERE AccountCode LIKE @ParentCodePlus + '%' AND ParentAccountId = @ParentAccountId",
                    conn);
                cmdChild.Parameters.AddWithValue("@ParentCodePlus", parentCode);
                cmdChild.Parameters.AddWithValue("@ParentAccountId", parentAccountId);

                var maxChildCodeObj = await cmdChild.ExecuteScalarAsync();

                if (maxChildCodeObj == DBNull.Value || maxChildCodeObj == null)
                {
                    return parentCode + "01";
                }
                else
                {
                    int maxChildCode = Convert.ToInt32(maxChildCodeObj);
                    return (maxChildCode + 1).ToString();
                }
            }
        }

        public async Task<List<ChartOfAccount>> GetAllAccountsAsync()
        {
            var accounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "SELECT");

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var account = new ChartOfAccount
                {
                    AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                    AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("ParentAccountId")),
                    AccountLevel = reader.GetInt32(reader.GetOrdinal("AccountLevel")),
                    IsActive = reader.GetString(reader.GetOrdinal("IsActive")),
                    AccountNature = reader.IsDBNull(reader.GetOrdinal("AccountNature")) ? null : reader.GetString(reader.GetOrdinal("AccountNature")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Children = new List<ChartOfAccount>()
                };

                accounts.Add(account);
            }

            return accounts;
        }


        public async Task<ChartOfAccount?> GetAccountByIdAsync(Guid accountId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ChartOfAccount
                {
                    AccountId = reader.GetGuid(reader.GetOrdinal("AccountId")),
                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                    AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("ParentAccountId")),
                    AccountLevel = reader.GetInt32(reader.GetOrdinal("AccountLevel")),
                    IsActive = reader.GetString(reader.GetOrdinal("IsActive")),
                    AccountNature = reader.IsDBNull(reader.GetOrdinal("AccountNature")) ? null : reader.GetString(reader.GetOrdinal("AccountNature")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                };
            }
            else
            {
                return null;  
            }
        }
    }
}
