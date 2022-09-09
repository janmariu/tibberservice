using InfluxDB.Client.Core;
using Tibber.Sdk;

namespace tibberservice.Model;

[Measurement("Power")]
public class PowerMeasurement
{
    [Column("Power")] public decimal Power { get; set; }
    [Column("TimeStamp", IsTimestamp = true)] public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    [Column("AccumulatedConsumptionLastHour")] public decimal AccumulatedConsumptionLastHour { get; set; }
    [Column("AccumulatedConsumption")] public decimal AccumulatedConsumption { get; set; }
    [Column("LastMeterConsumption")] public decimal? LastMeterConsumption { get; set; }
    [Column("AccumulatedCost")] public decimal? AccumulatedCost { get; set; }
    [Column("MinPower")]public decimal MinPower { get; set; }
    [Column("AveragePower")]public decimal AveragePower { get; set; }
    [Column("MaxPower")]public decimal MaxPower { get; set; }
    [Column("SignalStrength")] public int? SignalStrength { get; set; }
    [Column("Installation", IsTag = true)] public string HomeName { get; set; } = string.Empty;
    
    public static PowerMeasurement Create(RealTimeMeasurement value, string homeName)
    {
        return new PowerMeasurement()
        {
            HomeName = homeName,
            Power = value.Power,
            TimeStamp = value.Timestamp.UtcDateTime,
            AccumulatedConsumption = value.AccumulatedConsumption,
            AccumulatedConsumptionLastHour = value.AccumulatedConsumptionLastHour,
            LastMeterConsumption = value.LastMeterConsumption,
            AccumulatedCost = value.AccumulatedCost,
            MinPower = value.MinPower,
            AveragePower = value.AveragePower,
            MaxPower = value.MaxPower,
            SignalStrength = value.SignalStrength
        };
    }
}