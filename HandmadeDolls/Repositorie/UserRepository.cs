using HandmadeDolls.DAL;
using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Identity.Model;
using System.Data;
using System.Data.Common;
using static HandmadeDolls.Data.MinimalContextDb;

namespace HandmadeDolls.Repositorie;

public class UserRepository
{
    public async Task<int> UserClaim(IdentityUser? currentUser, string claim)
    {
        int result;
        
        try
        {
            result = await VerifyUserClaim(currentUser, claim);

            if (result != 1)
                result = await SaveUserClaim(currentUser, claim);
        }
        catch (Exception)
        {
            throw;
        }
        
        return result;
    }

    public async Task<int> DeleteUserClaim(IdentityUser? currentUser, string claim)
    {
        int result;

        try
        {
            result = await VerifyUserClaim(currentUser, claim);

            if (result == 1)
            {
                await DeleteClaim(currentUser, claim);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return result;
    }

    public async Task<int> VerifyUserClaim(IdentityUser? currentUser, string claim)
    {
        var result = 0;
        LoginUser user = new();

        await using (Context ctx = new())
        {
            ctx.Connection.Open();
            DbCommand cmd = ctx.Connection.CreateCommand();
            cmd.CommandText = $"SELECT ANU.UserName FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";
            cmd.CommandType = CommandType.Text;

            using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    user.Email = reader.GetString(0);
                }
            }
        }

        if (!String.IsNullOrEmpty(user.Email))
            result = 1;
        
        return result;
    }
    
    public async Task<int> SaveUserClaim(IdentityUser? currentUser, string claim)
    {
        var result = 0;
        LoginUser user = new();

        await using (Context ctx = new())
        {
            ctx.Connection.Open();
            DbCommand cmd = ctx.Connection.CreateCommand();
            cmd.CommandText = $"INSERT AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ('{currentUser?.Id}', '{claim}', '{claim}')";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }

        await using (Context ctx = new())
        {
            ctx.Connection.Open();
            DbCommand cmd = ctx.Connection.CreateCommand();
            cmd.CommandText = $"SELECT ANU.UserName FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";
            cmd.CommandType = CommandType.Text;

            using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    user.Email = reader.GetString(0);
                }
            }
        }

        if (!String.IsNullOrEmpty(user.Email))
            result = 2;

        return result;
    }

    public async Task DeleteClaim(IdentityUser? currentUser, string claim)
    {    
        await using (Context ctx = new())
        {
            ctx.Connection.Open();
            DbCommand cmd = ctx.Connection.CreateCommand();
            cmd.CommandText = $"DELETE AspNetUserClaims WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }
    }

    public async Task<int> VerifyUser(Object? customer)
    {
        LoginUser user = new();

        if (customer != null)
        {
            await using (Context ctx = new())
            {
                ctx.Connection.Open();
                DbCommand cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = $"SELECT UserName FROM AspNetUsers WHERE Email = @email";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(SQLUteis.GetSqlParameter("@email", SqlDbType.VarChar, customer));

                using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.Email = reader.GetValue<string>("UserName");
                    }
                }
            }
        }     

        if (!String.IsNullOrEmpty(user.Email))
            return 1;
        
        return 0;
    }

    public async Task<int> VerifyCustomerClain(Object? customer)
    {
        LoginUser user = new();

        if (customer != null)
        {
            await using (Context ctx = new())
            {
                ctx.Connection.Open();
                DbCommand cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = $"SELECT ANU.UserName FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE ANU.UserName = @email AND ClaimType = 'Customer'";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(SQLUteis.GetSqlParameter("@email", SqlDbType.VarChar, customer));

                using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user.Email = reader.GetValue<string>("UserName");
                    }
                }
            }
        }

        if (!String.IsNullOrEmpty(user.Email))
            return 1;

        return 0;
    }

    public async Task<Guid> GetUserId(Object? customer)
    {
        Order order = new();

        if (customer != null)
        {
            await using (Context ctx = new())
            {
                ctx.Connection.Open();
                DbCommand cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = $"SELECT AUC.UserId FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE ANU.UserName = @email AND ClaimType = 'Customer'";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(SQLUteis.GetSqlParameter("@email", SqlDbType.VarChar, customer));

                using (SqlDataReader reader = (SqlDataReader)cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        order.CustomerId = Guid.Parse(reader.GetString(0));
                    }
                }
            }
        }

        if (order.CustomerId == Guid.Empty)
            return order.CustomerId;

        return order.CustomerId;
    }
}
