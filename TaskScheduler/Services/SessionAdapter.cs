using TaskScheduler.Interfaces;

namespace TaskScheduler.Services
{
    public class SessionAdapter: ISessionService
    {
        public int CurrentUserId => SessionManager.CurrentUserId;
    }
}
