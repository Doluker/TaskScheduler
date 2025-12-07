using System;
using System.Collections.Generic;
using TaskScheduler.Models;

namespace TaskScheduler.Interfaces
{
    public interface ITaskService
    {
        void CreateTask(string title, string description, int taskTypeId, int priorityId, DateTime dueDate, int recurrenceId);
        void DeleteTask(int currentTaskId);
        List<Schedule> LoadAllTasks();
        List<Schedule> LoadBaseTasks();
        List<Schedule> LoadProjectTasks();
        List<Schedule> LoadPersonalTasks();
        List<Schedule> LoadMeetingsTasks();
        List<Schedule> FilterChange(int statusIndex, int sortIndex, string categoryTag);
        Schedule GetScheduleWithDetails(int scheduleId);
        int EditStatus(int currentScheduleId, int newStatusId);
        void UpdateExpiredTasks();
        void UpdateCompletedTasks();
        List<Status> GetStatusesForFilter();
        List<Status> GetStatusesForEdit();
    }
}