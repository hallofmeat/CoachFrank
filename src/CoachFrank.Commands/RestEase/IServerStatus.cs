using System;
using System.Threading.Tasks;
using RestEase;

namespace CoachFrank.Commands.RestEase
{
    public interface IServerStatus
    {
        [Get("/debug/status")]
        Task<StatusUpdate> GetStatus();
    }

    public class StatusUpdate
    {
        public DateTime StartTime { get; set; }
    }

}
