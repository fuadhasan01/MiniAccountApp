using Microsoft.Data.SqlClient;
using System.Data;

namespace MiniAccountApp.Data
{
    public class RolePageAccessService
    {
        private readonly string _connectionString;

        public RolePageAccessService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<string>> GetAllPagesAsync()
        {
            var pages = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetAllPages", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pages.Add(reader.GetString(0));
            }
            return pages;
        }

        public async Task<Dictionary<string, bool>> GetAccessForRoleAsync(string roleName)
        {
            var dict = new Dictionary<string, bool>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetPageAccessByRole", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@RoleName", roleName);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dict[reader.GetString(0)] = reader.GetBoolean(1);
            }
            return dict;
        }

        public async Task UpdateAccessAsync(string roleName, string pageUrl, bool hasAccess)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdatePageAccess", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@RoleName", roleName);
            cmd.Parameters.AddWithValue("@PageUrl", pageUrl);
            cmd.Parameters.AddWithValue("@HasAccess", hasAccess);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddNewPageAsync(string pageUrl)
        {
            using var conn = new SqlConnection(_connectionString);
            var cmdText = @"
                IF NOT EXISTS (SELECT 1 FROM ApplicationPages WHERE PageUrl = @PageUrl)
                BEGIN
                    INSERT INTO ApplicationPages (PageUrl) VALUES (@PageUrl)
                END";
            using var cmd = new SqlCommand(cmdText, conn);
            cmd.Parameters.AddWithValue("@PageUrl", pageUrl);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
