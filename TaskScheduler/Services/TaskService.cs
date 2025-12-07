using Microsoft.EntityFrameworkCore;
using TaskScheduler.Interfaces;
using TaskScheduler.Models;

namespace TaskScheduler.Services
{
    public class TaskService : ITaskService
    {
        private readonly DataBaseContext db;
        private readonly ISessionService sessionService;

        public TaskService(DataBaseContext db, ISessionService sessionService)
        {
            ArgumentNullException.ThrowIfNull(db);
            this.db = db;
            this.sessionService = sessionService;
        }

        public void CreateTask(string title, string description, int taskTypeId, int priorityId, DateTime dueDate, int recurrenceId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                Models.Task task = new Models.Task
                {
                    Name = title,
                    Description = description,
                    TaskTypeId = taskTypeId,
                    PriorityId = priorityId,
                    UpdatedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UserId = sessionService.CurrentUserId
                };
                db.Tasks.Add(task);

                var activeStatus = db.Statuses.FirstOrDefault(s => s.Name == "Активна") ?? db.Statuses.FirstOrDefault(s => s.Id == 1);

                Schedule schedule = new Schedule
                {
                    Task = task,
                    RecurrenceTypeid = recurrenceId,
                    DueDate = dueDate.Date,
                    StatusId = activeStatus?.Id ?? 1
                };
                try
                {
                    db.Schedules.Add(schedule);
                    db.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Ошибка при сохранении задачи.", ex);
                }
            }
        }

        public List<Schedule> LoadAllTasks() => FilterChange(0, 0, "0");
        public List<Schedule> LoadBaseTasks() => LoadTasksByTaskTypes("Обычная");
        public List<Schedule> LoadProjectTasks() => LoadTasksByTaskTypes("Проект");
        public List<Schedule> LoadPersonalTasks() => LoadTasksByTaskTypes("Личная");
        public List<Schedule> LoadMeetingsTasks() => LoadTasksByTaskTypes("Встреча");

        private List<Schedule> LoadTasksByTaskTypes(string TaskTypeName)
        {
            var TaskType = db.TaskTypes.FirstOrDefault(tt => tt.Name == TaskTypeName);
            if (TaskType != null)
            {
                return FilterChange(0, 0, TaskType.Id.ToString());
            }
            return new List<Schedule>();
        }

        public List<Status> GetStatusesForFilter()
        {
            var StatusList = db.Statuses.ToList();
            StatusList.Insert(0, new Status { Id = 0, Name = "Все статусы без просроченных и выполненных" });
            StatusList.RemoveRange(StatusList.Count-2,2);
            return StatusList;
        }

        public List<Status> GetStatusesForEdit()
        {
            return db.Statuses.Where(s => s.Name != "Просрочена" && s.Name != "Выполнена и обработана" && s.Name != "Просрочена и обработана").ToList();
        }

        public Models.Schedule GetScheduleWithDetails(int scheduleId)
        {
            return db.Schedules
                .Include(s => s.Status)
                .Include(s => s.Task)
                    .ThenInclude(t => t.Priority)
                .Include(s => s.Task)
                    .ThenInclude(t => t.TaskType)
                .FirstOrDefault(s => s.Id == scheduleId && s.Task.UserId == sessionService.CurrentUserId);
        }

        public List<Schedule> FilterChange(int statusIndex, int sortIndex, string categoryTag)
        {
            IQueryable<Schedule> query = db.Schedules
                .Include(s => s.Task)
                    .ThenInclude(t => t.Priority)
                .Include(s => s.Task)
                    .ThenInclude(t => t.TaskType)
                .Include(s => s.Status)
                .Where(s => s.Task.UserId == sessionService.CurrentUserId);

            if (categoryTag != "0")
            {
                if (int.TryParse(categoryTag, out int categoryId))
                {
                    query = query.Where(s => s.Task.TaskTypeId == categoryId);
                }
            }

            if (statusIndex != 0)
            {
                query = query.Where(s => s.StatusId == statusIndex);
            }
            else
            {
                query = query.Where(s => s.Status.Name != "Просрочена" && s.Status.Name != "Выполнена" && s.Status.Name != "Выполнена и обработана" && s.Status.Name != "Просрочена и обработана");
            }

            switch (sortIndex)
            {
                case 0:query = query.OrderBy(s => s.DueDate);break;
                case 1:query = query.OrderByDescending(s => s.DueDate);break;
                case 2:query = query.OrderByDescending(s => s.Task.PriorityId);break;
                case 3:query = query.OrderBy(s => s.Task.PriorityId);break;
            }

            return query.ToList();
        }

        public int EditStatus(int currentScheduleId, int newStatusId)
        {
            var editedSchedule = db.Schedules
                .Include(s => s.Task)
                .FirstOrDefault(s => s.Id == currentScheduleId && s.Task.UserId == sessionService.CurrentUserId);

            if (editedSchedule != null)
            {
                editedSchedule.StatusId = newStatusId;
                editedSchedule.Task.UpdatedAt = DateTime.Now;
                try
                {
                    db.SaveChanges();
                    return newStatusId;
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при изменении статуса расписания", ex);
                }
            }
            else
            {
                throw new Exception($"Расписание с ID {currentScheduleId} не найдено.");
            }
        }

        public void DeleteTask(int currentTaskId)
        {
            using (var transition = db.Database.BeginTransaction())
            {
                var taskToDelete = db.Tasks.Include(t => t.Schedules).FirstOrDefault(t => t.Id == currentTaskId);
                if (taskToDelete != null)
                {
                    db.Schedules.RemoveRange(taskToDelete.Schedules);
                    db.Tasks.Remove(taskToDelete);
                    try
                    {
                        db.SaveChanges();
                        transition.Commit();
                    }
                    catch (Exception ex)
                    {
                        transition.Rollback();
                        throw new Exception("Ошибка в удалении задачи", ex);
                    }
                }
            }
        }

        public void UpdateExpiredTasks()
        {
            try
            {
                var expiredStatus = db.Statuses.FirstOrDefault(s => s.Name == "Просрочена");
                var completedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Выполнена");
                var archiveCompletedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Выполнена и обработана");
                var archiveExpiredStatus = db.Statuses.FirstOrDefault(s => s.Name == "Просрочена и обработана");
                if (expiredStatus != null && completedStatus != null && archiveCompletedStatus != null && archiveExpiredStatus != null)
                {
                    DateTime timeNow = DateTime.Now;
                    var newSchedules = new List<Schedule>();
                    var expiredSchedules = db.Schedules.Include(s => s.Task).ThenInclude(t => t.Schedules).Where(s => s.DueDate < timeNow).Where(s => s.StatusId != expiredStatus.Id && s.StatusId != completedStatus.Id && s.StatusId != archiveCompletedStatus.Id && s.StatusId != archiveExpiredStatus.Id).ToList();
                    var tasksToGenerateNewSchedule = new Dictionary<int, Models.Task>();
                    foreach (var schedule in expiredSchedules)
                    {
                        if (schedule.RecurrenceTypeid != 1)
                        {
                            schedule.StatusId = archiveExpiredStatus.Id;
                            tasksToGenerateNewSchedule[schedule.TaskId] = schedule.Task;
                        }
                        else
                        {
                            schedule.StatusId = expiredStatus.Id;
                        }
                    }
                    foreach (var task in tasksToGenerateNewSchedule.Values)
                    {
                        var newSchedule = GenerateNextSchedule(task);
                        if (newSchedule != null)
                        {
                            newSchedules.Add(newSchedule);
                        }
                    }
                    if (newSchedules.Any())
                    {
                        db.AddRange(newSchedules);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось обновить просроченные задачи", ex);
            }
        }
        public void UpdateCompletedTasks()
        {
            try
            {
                var completedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Выполнена");
                var archiveCompletedStatus = db.Statuses.FirstOrDefault(s => s.Name == "Выполнена и обработана");
                if (completedStatus != null && archiveCompletedStatus != null)
                {   
                    DateTime timeNow = DateTime.Now;
                    var newSchedules = new List<Schedule>();
                    var completedSchedules = db.Schedules.Include(s => s.Task).ThenInclude(t => t.Schedules).Where(s => s.DueDate < timeNow).Where(s => s.StatusId == completedStatus.Id && s.StatusId != archiveCompletedStatus.Id).ToList();
                    var tasksToGenerateNewSchedule = new Dictionary<int, Models.Task>();
                    foreach (var schedule in completedSchedules)
                    {
                        if (schedule.RecurrenceTypeid != 1)
                        {
                            tasksToGenerateNewSchedule[schedule.TaskId] = schedule.Task;
                        }
                    }
                    foreach (var task in tasksToGenerateNewSchedule.Values)
                    {
                        var scheduleToArchive = task.Schedules
                        .Where(s => s.StatusId == completedStatus.Id)
                        .Where(s => s.DueDate < timeNow)
                        .OrderByDescending(s => s.DueDate)
                        .FirstOrDefault();
                        var newSchedule = GenerateNextSchedule(task);
                        if (scheduleToArchive != null)
                        {
                            scheduleToArchive.StatusId = archiveCompletedStatus.Id;
                        }
                        if (newSchedule != null)
                        {
                            newSchedules.Add(newSchedule);
                        }
                        
                    }
                    if (newSchedules.Any())
                    {
                        db.AddRange(newSchedules);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось обновить выполненые задачи", ex);
            }
        }
        private Schedule GenerateNextSchedule(Models.Task task)
        {
            var latestSchedule = task.Schedules.OrderByDescending(s => s.DueDate).FirstOrDefault();
            if (latestSchedule == null) 
                return null;
            var activeStatus = db.Statuses.FirstOrDefault(s => s.Name == "Активна") ?? db.Statuses.FirstOrDefault(s => s.Id == 1);
            DateTime nextDueDate;
            switch (latestSchedule.RecurrenceTypeid)
            {
                case 2: nextDueDate = latestSchedule.DueDate.AddDays(1); break;
                case 3: nextDueDate = latestSchedule.DueDate.AddDays(7); break;
                case 4: nextDueDate = latestSchedule.DueDate.AddMonths(1); break;
                case 5: nextDueDate = latestSchedule.DueDate.AddYears(1); break;
                default: return null;
            }

            return new Schedule
            {
                RecurrenceTypeid = latestSchedule.RecurrenceTypeid,
                TaskId = task.Id,
                DueDate = nextDueDate,
                StatusId = activeStatus?.Id ?? 1
            };
        }
    }
}