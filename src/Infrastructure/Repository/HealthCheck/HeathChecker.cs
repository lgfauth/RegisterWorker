using Domain.Models;
using Domain.Settings;
using MicroservicesLogger;
using MicroservicesLogger.Interfaces;
using MicroservicesLogger.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository.HealthCheck
{
    public class HeathChecker
    {
        public static async Task CheckMongoDbConnection(EnvirolmentVariables variables)
        {
            IApiLog<WorkerLogModel> logger = new ApiLog<WorkerLogModel>();
            var baselog = await logger.CreateBaseLogAsync();
            var subLog = new SubLog();

            try
            {
                subLog.StartCronometer();

                baselog.Request = "HeathChecker Mongo DB";
                await baselog.AddStepAsync("CHECKING_MONGODB_CONNECTION");

                string mongoUser = variables.MONGODBDATA_USER;
                string mongoPassword = variables.MONGODBDATA_PASSWORD;
                string mongoCluster = variables.MONGODBDATA_CLUSTER;
                string mongoDbConnectionString = variables.MONGODBSETTINGS_CONNECTIONSTRING;

                mongoDbConnectionString = string.Format(mongoDbConnectionString, mongoUser, Uri.EscapeDataString(mongoPassword), mongoCluster);

                if (string.IsNullOrWhiteSpace(mongoDbConnectionString))
                {
                    subLog.StopCronometer();
                    baselog.Response = "MongoDB connection string is invalid.";

                    await baselog.AddStepAsync("MONGODB_CHECKIN_SUCCESS", subLog);

                    return;
                }
                var client = new MongoClient(mongoDbConnectionString);
                var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));

                baselog.Response = "Pinged your deployment. You successfully connected to MongoDB!";
                subLog.StopCronometer();

                await baselog.AddStepAsync("MONGODB_CHECKIN_SUCCESS", subLog);
            }
            catch (Exception ex)
            {
                baselog.Response = "MongoDB connection failed.";
                subLog.Exception = ex;

                await baselog.AddStepAsync("MONGODB_CHECKIN_FAIL", subLog);
            }
            finally
            {
                await logger.WriteLogAsync(baselog);
            }
        }
    }
}