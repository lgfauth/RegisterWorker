using System.Diagnostics.CodeAnalysis;

namespace Domain.Settings
{
    [ExcludeFromCodeCoverage]
    public class EnvirolmentVariables
    {
        public string? JWTSETTINGS_SECRETKEY { get; set; }
        public string? JWTSETTINGS_ISSUER { get; set; }
        public string? JWTSETTINGS_AUDIENCE { get; set; }
        public int JWTSETTINGS_EXPIRATIONMINUTES { get; set; } = 60;
        public string? MONGODBDATA_USER { get; set; }
        public string? MONGODBDATA_PASSWORD { get; set; }
        public string? MONGODBDATA_CLUSTER { get; set; }
        public string? MONGODBSETTINGS_CONNECTIONSTRING { get; set; }
        public string MONGODBSETTINGS_DATABASENAME { get; set; } = "mongodb+srv://{0}:{1}@{2}.mongodb.net/";
        public string? RABBITMQCONFIGURATION_HOSTNAME { get; set; }
        public string? RABBITMQCONFIGURATION_USERNAME { get; set; }
        public string? RABBITMQCONFIGURATION_PASSWORD { get; set; }
        public string? RABBITMQCONFIGURATION_QUEUENAME { get; set; }
        public string? RABBITMQCONFIGURATION_DLQ_QUEUENAME { get; set; }
        public string? RABBITMQCONFIGURATION_RETRY_QUEUENAME { get; set; }
        public string? RABBITMQCONFIGURATION_VIRTUALHOST { get; set; }
        public string? EMAIL_FROM_ADDRESS { get; set; }
        public string? EMAIL_SMTP_CLIENT { get; set; }
        public string? EMAIL_FROM_PASSWORD { get; set; }
    }
}
