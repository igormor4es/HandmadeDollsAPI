using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace HandmadeDolls.DAL;

public static class SQLUteis
{
    public static SqlParameter GetSqlDatatypeParameter(string nome, SqlDbType type, object valor)
    {
        SqlParameter parameter = new SqlParameter(nome, type);
        parameter.Value = valor;

        return parameter;
    }

    public static SqlParameter GetSqlParameter(string nome, SqlDbType type, object valor)
    {
        SqlParameter parameter = new SqlParameter(nome, type);
        if (valor == null)
            parameter.Value = DBNull.Value;
        else
            parameter.Value = valor;


        return parameter;
    }

    public static SqlParameter GetSqlParameter(string nome, SqlDbType type, object valor, string typeName)
    {
        SqlParameter parameter = new SqlParameter(nome, type);
        if (valor == null)
            parameter.Value = DBNull.Value;
        else
            parameter.Value = valor;

        parameter.TypeName = typeName;

        return parameter;
    }

    public static T GetValue<T>(this DbDataReader reader, string field)
    {
        return (reader[field] == null || reader[field] == System.DBNull.Value) ? default(T) : (T)reader[field];
    }
}
