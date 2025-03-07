using MicroservicesLogger.Models;

namespace Domain.Models
{
    public class WorkerLogModel : LogObject
    {
        public bool IsSuccess { get; set; }
    }
}