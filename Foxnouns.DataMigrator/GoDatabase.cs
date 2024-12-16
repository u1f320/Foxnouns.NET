using System.Data;
using Dapper;
using Foxnouns.DataMigrator.Models;
using Newtonsoft.Json;
using Npgsql;

namespace Foxnouns.DataMigrator;

public static class GoDatabase
{
    private static NpgsqlDataSource? _dataSource;

    public static async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_dataSource != null)
            return await _dataSource.OpenConnectionAsync();

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        SqlMapper.RemoveTypeMap(typeof(ulong));
        SqlMapper.AddTypeHandler(new UlongEncodeAsLongHandler());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<GoFieldEntry[]>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<Dictionary<string, GoCustomPreference>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<GoPronounEntry[]>());
        SqlMapper.AddTypeHandler(new UlongListHandler());

        string dsn =
            Environment.GetEnvironmentVariable("GO_DATABASE")
            ?? throw new Exception("$GO_DATABASE is not set");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(dsn);
        dataSourceBuilder.UseJsonNet();

        _dataSource = dataSourceBuilder.Build();

        return await _dataSource.OpenConnectionAsync();
    }

    // dapper why
    // taken from https://codeberg.org/starshine/catalogger/src/branch/main/Catalogger.Backend/Database/DatabasePool.cs
    private class UlongEncodeAsLongHandler : SqlMapper.TypeHandler<ulong>
    {
        public override void SetValue(IDbDataParameter parameter, ulong value) =>
            parameter.Value = (long)value;

        public override ulong Parse(object value) =>
            // Cast to long to unbox, then to ulong (???)
            (ulong)(long)value;
    }

    private class UlongListHandler : SqlMapper.TypeHandler<List<ulong>>
    {
        public override void SetValue(IDbDataParameter parameter, List<ulong>? value) =>
            parameter.Value = value?.Select(i => (long)i).ToArray();

        public override List<ulong>? Parse(object value) =>
            ((long[])value).Select(i => (ulong)i).ToList();
    }

    private class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override void SetValue(IDbDataParameter parameter, T? value) =>
            parameter.Value = JsonConvert.SerializeObject(value);

        public override T? Parse(object value)
        {
            var json = (string)value;
            return JsonConvert.DeserializeObject<T>(json) ?? default;
        }
    }
}
