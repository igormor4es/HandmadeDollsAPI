using HandmadeDolls.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Identity.Model;
using System.Data;
using System.Data.Common;

namespace HandmadeDolls.Repositorie;

public class UserRepository
{
    public async Task<int> UserClaim(IdentityUser? currentUser, MinimalContextDb context, string claim, string connectionString)
    {
        int result;
        
        try
        {
            result = await VerifyUserClaim(currentUser, context, claim);

            if (result != 1)
                result = await SaveUserClaim(currentUser, context, claim, connectionString);
        }
        catch (Exception)
        {
            throw;
        }
        
        return result;
    }

    public async Task<int> DeleteUserClaim(IdentityUser? currentUser, MinimalContextDb context, string claim, string connectionString)
    {
        int result;

        try
        {
            result = await VerifyUserClaim(currentUser, context, claim);

            if (result == 1)
            {
                await DeleteClaim(currentUser, context, claim, connectionString);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return result;
    }

    public async Task<int> VerifyUserClaim(IdentityUser? currentUser, MinimalContextDb context, string claim)
    {
        var result = 0;
        LoginUser user = new();

        using (var connection = context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT ANU.UserName FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";

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
    
    public async Task<int> SaveUserClaim(IdentityUser? currentUser, MinimalContextDb context, string claim, string connectionString)
    {
        var result = 0;
        LoginUser user = new();
        
        using (var connection = context.Database.GetDbConnection())
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();
            }

            DbCommand cmd = connection.CreateCommand();

            cmd.CommandText = $"INSERT AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ('{currentUser?.Id}', '{claim}', '{claim}')";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = $"SELECT ANU.UserName FROM AspNetUserClaims AUC JOIN AspNetUsers ANU ON AUC.UserId = ANU.Id WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";
            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                user.Email = reader.GetString(0);
            }
        }

        if (!String.IsNullOrEmpty(user.Email))
            result = 2;

        return result;
    }

    public async Task DeleteClaim(IdentityUser? currentUser, MinimalContextDb context, string claim, string connectionString)
    {    
        using (var connection = context.Database.GetDbConnection())
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();
            }
                
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DELETE AspNetUserClaims WHERE UserId = '{currentUser?.Id}' AND ClaimType = '{claim}'";
                await command.ExecuteNonQueryAsync();
            }
        }      
    }
}
