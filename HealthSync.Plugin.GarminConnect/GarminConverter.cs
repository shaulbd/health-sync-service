using HealthSync.Core.Extensions;
using HealthSync.Core.Models;
using HealthSync.Core.Shared;
using HealthSync.Plugin.GarminConnect.Client.Models;

namespace HealthSync.Plugin.GarminConnect
{
    /// <summary>
    /// Garmin converter constants
    /// </summary>
    public class GConsts
    {
        public const string DateUtc = "DateUTC";
        public const string StartTimeLocal = "StartTimeLocal";
        public const string EndTimeLocal = "EndTimeLocal";
    }

    public static class GUtil
    {
        public static bool IsValidGarminMetric<T>(this T?[]? s)
        {
            // Check if the array is null or has less than 2 elements
            if (s == null || s.Length < 2)
            {
                return false;
            }

            // Check if the first two elements are not null
            if (s[0] == null || s[1] == null)
            {
                return false;
            }

            // Use dynamic to handle the comparison at runtime
            dynamic value = s[1];
            return value > 0;
        }
    }

    public class HeartRateConverter : HealthDataConverter<GarminHr>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminHr sourceData)
        {
            if (sourceData == null || sourceData.HeartRateValues == null)
            {
                return;
            }

            healthData.HeartRate ??= new HeartRateData();

            foreach (var hr in sourceData.HeartRateValues.Where(s => s.IsValidGarminMetric()))
            {
                var time = DateTimeOffset.FromUnixTimeMilliseconds(hr[0].Value);
                var value = hr[1].Value;

                healthData.HeartRate.Samples.Add(new HeartRateSample { Value = value, Start = time });
            }

            if (healthData.HeartRate.Samples.Count == 0)
            {
                healthData.HeartRate = null;
                return;
            }

            healthData.HeartRate.Start = sourceData.StartTimestampGmt.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.HeartRate.End = sourceData.EndTimestampGmt.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.HeartRate[GConsts.DateUtc] = sourceData.CalendarDate;
            healthData.HeartRate.SafeSetValue("MaximumHeartRate", sourceData.MaxHeartRate);
            healthData.HeartRate.SafeSetValue("MinimumHeartRate", sourceData.MinHeartRate);
            healthData.HeartRate.SafeSetValue("RestingHeartRate", sourceData.RestingHeartRate);
            healthData.HeartRate.SafeSetValue("Last7DaysAvgRestingHeartRate", sourceData.LastSevenDaysAvgRestingHeartRate);
            healthData.HeartRate[GConsts.StartTimeLocal] = sourceData.StartTimestampLocal;
            healthData.HeartRate[GConsts.EndTimeLocal] = sourceData.EndTimestampLocal;
        }
    }

    public class BodyBatteryConverter : HealthDataConverter<GarminBodyBatteryData[]>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminBodyBatteryData[] sourceData)
        {
            if (sourceData == null || sourceData.Length == 0)
            {
                return;
            }

            healthData.BodyEnergy ??= new BodyEnergyData();

            foreach (var battery in sourceData)
            {
                if (battery.BodyBatteryValuesArray != null)
                {
                    foreach (var item in battery.BodyBatteryValuesArray.Where(s => s.IsValidGarminMetric()))
                    {
                        var sample = new BodyEnergySample
                        {
                            Start = DateTimeOffset.FromUnixTimeMilliseconds(item[0].Value),
                            Value = item[1].Value
                        };
                        healthData.BodyEnergy.Samples.Add(sample);
                    }
                }
            }

            if (healthData.BodyEnergy.Samples.Count == 0)
            {
                healthData.BodyEnergy = null;
                return;
            }

            healthData.BodyEnergy.Start = healthData.BodyEnergy.Samples.Min(s => s.Start);
            healthData.BodyEnergy.End = healthData.BodyEnergy.Samples.Max(s => s.Start);
        }
    }

    public class SleepDataConverter : HealthDataConverter<GarminSleepData>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminSleepData sourceData)
        {
            if (sourceData?.DailySleepDto == null
            || !sourceData.DailySleepDto.SleepStartTimestampGmt.HasValue
            || !sourceData.DailySleepDto.SleepEndTimestampGmt.HasValue)
            {
                return;
            }

            var dailySleep = sourceData.DailySleepDto;
            healthData.Sleep ??= new SleepData();

            healthData.Sleep.TotalSleep = TimeSpan.FromSeconds(dailySleep.SleepTimeSeconds ?? 0);
            healthData.Sleep.DeepSleep = TimeSpan.FromSeconds(dailySleep.DeepSleepSeconds ?? 0);
            healthData.Sleep.LightSleep = TimeSpan.FromSeconds(dailySleep.LightSleepSeconds ?? 0);
            healthData.Sleep.RemSleep = TimeSpan.FromSeconds(dailySleep.RemSleepSeconds ?? 0);
            healthData.Sleep.Awake = TimeSpan.FromSeconds(dailySleep.AwakeSleepSeconds ?? 0);
            healthData.Sleep.Nap = TimeSpan.FromSeconds(dailySleep.NapTimeSeconds ?? 0);
            healthData.Sleep.Start = DateTimeOffset.FromUnixTimeMilliseconds(dailySleep.SleepStartTimestampGmt.Value);
            healthData.Sleep.End = DateTimeOffset.FromUnixTimeMilliseconds(dailySleep.SleepEndTimestampGmt.Value);

            if (dailySleep.SleepScores != null)
            {
                healthData.Sleep.Score = dailySleep.SleepScores.Overall.Value;
            }

            healthData.Sleep[GConsts.DateUtc] = dailySleep.CalendarDate;

            ConvertSleepLevels(healthData.Sleep, sourceData.SleepLevels);
            ConvertSleepMovements(healthData.Sleep, sourceData.SleepMovement);
            ConvertSleepStress(healthData.Sleep, sourceData.SleepStress);
        }

        private void ConvertSleepLevels(SleepData sleepData, IEnumerable<Sleep> sleepLevels)
        {
            if (sleepLevels == null)
            {
                return;
            }

            foreach (var sleepLevel in sleepLevels)
            {
                sleepData.LevelSamples.Add(new SleepLevelSample
                {
                    Start = new DateTimeOffset(sleepLevel.StartGmt.DateTime, TimeSpan.Zero),
                    End = new DateTimeOffset(sleepLevel.EndGmt.DateTime, TimeSpan.Zero),
                    Value = GetSleepStage(sleepLevel.ActivityLevel)
                });
            }
        }

        private void ConvertSleepMovements(SleepData sleepData, IEnumerable<Sleep> sleepMovements)
        {
            if (sleepMovements == null)
            {
                return;
            }

            foreach (var sleepMovement in sleepMovements)
            {
                sleepData.MovementSamples.Add(new SleepMovementSample
                {
                    Start = new DateTimeOffset(sleepMovement.StartGmt.DateTime, TimeSpan.Zero),
                    End = new DateTimeOffset(sleepMovement.EndGmt.DateTime, TimeSpan.Zero),
                    Value = sleepMovement.ActivityLevel
                });
            }
        }

        private void ConvertSleepStress(SleepData sleepData, IEnumerable<SleepStress> sleepStress)
        {
            if (sleepStress == null)
            {
                return;
            }

            foreach (var stress in sleepStress)
            {
                sleepData.StressSamples.Add(new SleepStressSample
                {
                    Start = DateTimeOffset.FromUnixTimeMilliseconds(stress.StartGmt),
                    Value = stress.Value
                });
            }
        }

        public static SleepStage GetSleepStage(double activityLevel)
        {
            if (activityLevel == 0)
                return SleepStage.Deep;
            else if (activityLevel == 1)
                return SleepStage.Light;
            else if (activityLevel == 2)
                return SleepStage.REM;
            else if (activityLevel == 3)
                return SleepStage.Awake;
            else
                return SleepStage.Unknown;
        }
    }

    public class StepsConverter : HealthDataConverter<GarminStepsData[]>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminStepsData[] sourceData)
        {
            if (sourceData == null || sourceData.Length == 0)
            {
                return;
            }

            healthData.Steps ??= new StepsData();

            foreach (var step in sourceData)
            {
                var sampleData = new StepsSample
                {
                    Value = step.Steps,
                    Start = step.StartGmt.ToDateTimeOffset(TimeZoneInfo.Utc),
                    End = step.EndGmt.ToDateTimeOffset(TimeZoneInfo.Utc)
                };

                sampleData["PrimaryActivityLevel"] = step.PrimaryActivityLevel;
                sampleData["ActivityLevelConstant"] = step.ActivityLevelConstant;

                healthData.Steps.Samples.Add(sampleData);
            }
        }
    }

    public class HrvConverter : HealthDataConverter<GarminReportHrvStatus>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminReportHrvStatus sourceData)
        {
            if (sourceData == null || sourceData.HrvSummaries == null)
            {
                return;
            }

            healthData.HrvData ??= new HrvData();

            foreach (var hrv in sourceData.HrvSummaries)
            {
                var hrvSample = new HrvSample
                {
                    Start = hrv.CreateTimeStamp.ToDateTimeOffset(TimeZoneInfo.Utc),
                    Value = hrv.LastNightAvg
                };
                hrvSample.ExtraData["DateUTC"] = hrv.CalendarDate;
                hrvSample.ExtraData["LastNight5MinHigh"] = hrv.LastNight5MinHigh;
                hrvSample.ExtraData["WeeklyAvg"] = hrv.WeeklyAvg;

                healthData.HrvData.Samples.Add(hrvSample);
            }
        }
    }

    public class StressConverter : HealthDataConverter<GarminStress>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminStress sourceData)
        {
            if (sourceData == null || sourceData.StressValuesArray == null)
            {
                return;
            }

            healthData.Stress ??= new StressData();

            foreach (var stress in sourceData.StressValuesArray.Where(s => s.IsValidGarminMetric()))
            {
                var sample = new StressSample
                {
                    Start = DateTimeOffset.FromUnixTimeMilliseconds(stress[0].Value),
                    Value = stress[1].Value
                };
                healthData.Stress.Samples.Add(sample);
            }

            if (healthData.Stress.Samples.Count == 0)
            {
                healthData.Stress = null;
                return;
            }

            healthData.Stress.Start = sourceData.StartTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Stress.End = sourceData.EndTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Stress.SafeSetValue(GConsts.DateUtc, sourceData.CalendarDate);
            healthData.Stress.SafeSetValue(GConsts.StartTimeLocal, sourceData.StartTimestampLocal);
            healthData.Stress.SafeSetValue(GConsts.EndTimeLocal, sourceData.EndTimestampLocal);
            healthData.Stress.SafeSetValue("MaxStressLevel", sourceData.MaxStressLevel);
            healthData.Stress.SafeSetValue("AvgStressLevel", sourceData.AvgStressLevel);
        }
    }

    public class RespirationConverter : HealthDataConverter<GarminRespiration>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminRespiration sourceData)
        {
            if (sourceData == null || sourceData.RespirationValuesArray == null)
            {
                return;
            }

            healthData.Respiration ??= new RespirationData();

            foreach (var respiration in sourceData.RespirationValuesArray.Where(s => s.IsValidGarminMetric()))
            {
                var sample = new RespirationSample
                {
                    Start = DateTimeOffset.FromUnixTimeMilliseconds((long)respiration[0].Value),
                    Value = (long)respiration[1].Value
                };
                healthData.Respiration.Samples.Add(sample);
            }

            if (healthData.Respiration.Samples.Count == 0)
            {
                healthData.Respiration = null;
                return;
            }

            healthData.Respiration.Start = sourceData.StartTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Respiration.End = sourceData.EndTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Respiration.SafeSetValue(GConsts.StartTimeLocal, sourceData.StartTimestampLocal);
            healthData.Respiration.SafeSetValue(GConsts.EndTimeLocal, sourceData.EndTimestampLocal);
            healthData.Respiration.SafeSetValue(GConsts.DateUtc, sourceData.CalendarDate);
            healthData.Respiration.SafeSetValue("HighestRespirationValue", sourceData.HighestRespirationValue);
            healthData.Respiration.SafeSetValue("LowestRespirationValue", sourceData.LowestRespirationValue);
            healthData.Respiration.SafeSetValue("AvgSleepRespirationValue", sourceData.AvgSleepRespirationValue);
            healthData.Respiration.SafeSetValue("AvgTomorrowSleepRespirationValue", sourceData.AvgTomorrowSleepRespirationValue);
            healthData.Respiration.SafeSetValue("AvgWakingRespirationValue", sourceData.AvgWakingRespirationValue);
        }
    }

    public class Spo2Converter : HealthDataConverter<GarminSpo2>
    {
        public override void ConvertAndAdd(HealthData healthData, GarminSpo2 sourceData)
        {
            if (sourceData == null || sourceData.SpO2HourlyAverages == null)
            {
                return;
            }

            healthData.Spo2 ??= new Spo2Data();

            foreach (var hourly in sourceData.SpO2HourlyAverages.Where(s => s.IsValidGarminMetric()))
            {
                var sample = new Spo2Sample
                {
                    Start = DateTimeOffset.FromUnixTimeMilliseconds((long)hourly[0].Value),
                    Value = (long)hourly[1].Value
                };
                healthData.Spo2.Samples.Add(sample);
            }

            if (healthData.Spo2.Samples.Count == 0)
            {
                healthData.Spo2 = null;
                return;
            }

            healthData.Spo2.Start = sourceData.StartTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Spo2.End = sourceData.EndTimestampGmt.Value.ToDateTimeOffset(TimeZoneInfo.Utc);
            healthData.Spo2.SafeSetValue(GConsts.StartTimeLocal, sourceData.StartTimestampLocal);
            healthData.Spo2.SafeSetValue(GConsts.EndTimeLocal, sourceData.EndTimestampLocal);
            healthData.Spo2.SafeSetValue(GConsts.DateUtc, sourceData.CalendarDate);
            healthData.Spo2.SafeSetValue("AverageSpO2", sourceData.AverageSpO2);
            healthData.Spo2.SafeSetValue("LowestSpO2", sourceData.LowestSpO2);
            healthData.Spo2.SafeSetValue("LatestSpO2", sourceData.LatestSpO2);
            healthData.Spo2.SafeSetValue("AvgSleepSpO2", sourceData.AvgSleepSpO2);
            healthData.Spo2.SafeSetValue("AvgTomorrowSleepSpO2", sourceData.AvgTomorrowSleepSpO2);
            healthData.Spo2.SafeSetValue("LastSevenDaysAvgSpO2", sourceData.LastSevenDaysAvgSpO2);
        }
    }
}
