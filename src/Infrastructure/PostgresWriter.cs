using Microsoft.Extensions.Options;
using tibberservice.Model;
using Npgsql;

namespace tibberservice.Infrastructure;

public class PostgresConfig
{
    public string ConnectionString { get; set; }
}

public class PostgresWriter
{
    private readonly ILogger<PostgresWriter> _logger;
    private readonly string? _connectionString;

    public PostgresWriter(ILogger<PostgresWriter> logger, IOptions<PostgresConfig> configuration)
    {
        _logger = logger;
        if (configuration.Value == null)
        {
            _logger.LogError("Missing \"Postgres\" { \"ConnectionString\": ... } in appsettings.json");
            return;
        }

        _connectionString = configuration.Value.ConnectionString;
    }
    
    public void Write(PowerMeasurement measurement)
    {
        if (_connectionString == null)
        {
            return;
        }

        string insertQuery = "INSERT INTO \"Power\" "
                             + " (time, "
                             + "\"AccumulatedConsumption\","
                             + "\"AccumulatedConsumptionLastHour\","
                             + "\"Power\","
                             + "\"LastMeterConsumption\","
                             + "\"AccumulatedCost\","
                             + "\"MinPower\","
                             + "\"AveragePower\","
                             + "\"MaxPower\""
                             + ") "
                             + "VALUES ("
                             + "@time," 
                             + "@AccumulatedConsumption,"
                             + "@AccumulatedConsumptionLastHour,"
                             + "@Power,"
                             + "@LastMeterConsumption,"
                             + "@AccumulatedCost," 
                             + "@MinPower,"
                             + "@AveragePower,"
                             + "@MaxPower"
                             + ")";
                             
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(insertQuery, connection);
            cmd.Parameters.AddWithValue("time", measurement.TimeStamp);
            cmd.Parameters.AddWithValue("AccumulatedConsumption", measurement.AccumulatedConsumption);
            cmd.Parameters.AddWithValue("AccumulatedConsumptionLastHour", measurement.AccumulatedConsumptionLastHour);
            cmd.Parameters.AddWithValue("Power", measurement.Power);
            cmd.Parameters.AddWithValue("LastMeterConsumption", measurement.LastMeterConsumption);
            cmd.Parameters.AddWithValue("AccumulatedCost", measurement.AccumulatedCost);
            cmd.Parameters.AddWithValue("MinPower", measurement.MinPower);
            cmd.Parameters.AddWithValue("AveragePower", measurement.AveragePower);
            cmd.Parameters.AddWithValue("MaxPower", measurement.MaxPower);
            
            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                _logger.LogWarning("Rows affected was 0");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
