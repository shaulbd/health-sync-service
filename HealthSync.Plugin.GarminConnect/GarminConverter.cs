using Garmin.Connect.Models;
using HealthSync.Core.Models;

namespace HealthSync.Plugin.GarminConnect
{
	internal static class GarminConverter
	{
		internal static HealthData Init(string index, DateTimeOffset startTime, DateTimeOffset endTime)
		{
			var healthData = new HealthData
			{
				StartTime = startTime,
				EndTime = endTime,
				Index = index,
				Metadata = []
			};

			return healthData;
		}

	
		internal static HealthData AddGarminHeartRate(this HealthData healthData, GarminHr garminHr)
		{
			if (garminHr != null && garminHr.HeartRateValues != null)
			{
				foreach (var rate in garminHr.HeartRateValues)
				{
					if (rate.Count() >= 2)
					{
						var time = DateTimeOffset.FromUnixTimeMilliseconds(rate[0]);
						var value = rate[1];

						healthData.HeartRateSamples.Add(new HeartRateSample { HeartRate = value, Timestamp = time });
					}
				}

				healthData.MaximumHeartRate = garminHr.MaxHeartRate;
				healthData.MinimumHeartRate = garminHr.MinHeartRate;
				healthData.RestingHeartRate = garminHr.RestingHeartRate;
				healthData.Metadata.Add("last7DaysAvgRestingHeartRate", garminHr.LastSevenDaysAvgRestingHeartRate.ToString());
			}
			return healthData;
		}

		internal static HealthData AddGarminSteps(this HealthData healthData, GarminStepsData[] garminSteps)
		{
			if (garminSteps != null && garminSteps.Any())
			{
				foreach (var garminStep in garminSteps)
				{
					var sampleData = new StepsSample
					{
						Steps = garminStep.Steps,
						Start = ToDateTimeOffset(garminStep.StartGmt, TimeZoneInfo.Utc),
						End = ToDateTimeOffset(garminStep.EndGmt, TimeZoneInfo.Utc)
					};
					healthData.StepsSamples.Add(sampleData);
				}
			}
			return healthData;
		}

		static DateTimeOffset ToDateTimeOffset(DateTime dateTime, TimeZoneInfo timeZone)
		{
			// Convert DateTime to UTC
			DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);

			// Create a DateTimeOffset using the UTC DateTime and the specified time zone
			return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
		}
	}
}
