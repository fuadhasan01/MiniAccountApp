using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MiniAccountApp.Models;

namespace MiniAccountApp.Data
{
    public class VoucherTypeService
    {
        private readonly IConfiguration _configuration;

        public VoucherTypeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        public List<VoucherType> GetAllVoucherTypes()
        {
            var list = new List<VoucherType>();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVoucherTypes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GETALL");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new VoucherType
                    {
                        VoucherTypeId = reader.GetGuid(reader.GetOrdinal("VoucherTypeId")),
                        TypeCode = reader["TypeCode"].ToString(),
                        TypeName = reader["TypeName"].ToString(),
                        NumberPrefix = reader["NumberPrefix"].ToString(),
                        IsActive = reader["IsActive"].ToString(),
                        CreatedBy = reader["CreatedBy"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedAt"]),
                        UpdatedBy = reader["UpdatedBy"].ToString(),
                        Description = reader["Description"].ToString()
                    });
                }
            }
            return list;
        }

        public VoucherType GetVoucherTypeById(Guid id)
        {
            VoucherType vt = null;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVoucherTypes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "GETBYID");
                cmd.Parameters.AddWithValue("@VoucherTypeId", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    vt = new VoucherType
                    {
                        VoucherTypeId = reader.GetGuid(reader.GetOrdinal("VoucherTypeId")),
                        TypeCode = reader["TypeCode"].ToString(),
                        TypeName = reader["TypeName"].ToString(),
                        NumberPrefix = reader["NumberPrefix"].ToString(),
                        IsActive = reader["IsActive"].ToString(),
                        CreatedBy = reader["CreatedBy"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedAt"]),
                        UpdatedBy = reader["UpdatedBy"].ToString(),
                        Description = reader["Description"].ToString()
                    };
                }
            }
            return vt;
        }

        public void CreateVoucherType(VoucherType model)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVoucherTypes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "CREATE");
                cmd.Parameters.AddWithValue("@TypeCode", model.TypeCode);
                cmd.Parameters.AddWithValue("@TypeName", model.TypeName);
                cmd.Parameters.AddWithValue("@NumberPrefix", (object)model.NumberPrefix ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? "system");
                cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateVoucherType(VoucherType model)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVoucherTypes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "UPDATE");
                cmd.Parameters.AddWithValue("@VoucherTypeId", model.VoucherTypeId);
                cmd.Parameters.AddWithValue("@TypeCode", model.TypeCode);
                cmd.Parameters.AddWithValue("@TypeName", model.TypeName);
                cmd.Parameters.AddWithValue("@NumberPrefix", (object)model.NumberPrefix ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedBy", model.UpdatedBy ?? "system");
                cmd.Parameters.AddWithValue("@Description", (object)model.Description ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteVoucherType(Guid id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("sp_ManageVoucherTypes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@VoucherTypeId", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
