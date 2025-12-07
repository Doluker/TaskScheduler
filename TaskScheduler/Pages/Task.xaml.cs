using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskScheduler.Interfaces;
using TaskScheduler.Models;

namespace TaskScheduler.Pages
{
    /// <summary>
    /// Логика взаимодействия для Task.xaml
    /// </summary>
    public partial class Task : Page
    {
        public Schedule currentSchedule { get; set; }
        public List<Status>? AvailableStatuses { get; set; }
        public Status? Status { get; set; }
        private readonly ITaskService _taskService;
        private int originalStatusId;
        public Task(int scheduleId, ITaskService taskService)
        {
            InitializeComponent();
            _taskService = taskService;
            InitializeSchedule(scheduleId);
            this.DataContext = this;
            btnSave.Visibility = Visibility.Collapsed;
        }
        private void InitializeSchedule(int scheduleId)
        {
            AvailableStatuses = _taskService.GetStatusesForEdit();
            var scheduleDetails = _taskService.GetScheduleWithDetails(scheduleId);

            if (scheduleDetails != null)
            {
                this.currentSchedule = scheduleDetails;

                var initialStatus = AvailableStatuses.FirstOrDefault(s => s.Id == scheduleDetails.StatusId);

                if (initialStatus != null)
                {
                    this.Status = initialStatus;
                    this.originalStatusId = initialStatus.Id;
                }
                switch (currentSchedule.Task?.Priority?.Name)
                {
                    case "Низкий": tbPriorityName.Foreground = Brushes.Green; break;
                    case "Средний": tbPriorityName.Foreground = Brushes.Orange; break;
                    case "Высокий": tbPriorityName.Foreground = Brushes.Red; break;
                    default: tbPriorityName.Foreground = Brushes.Gray; break;
                }
            }
            else
            {
                MessageBox.Show("Расписание задачи не найдено или нет доступа.");
                NavigationService.GoBack();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _taskService.DeleteTask(currentSchedule.TaskId);
                MyTasks myTasks = new MyTasks(_taskService);
                NavigationService.Navigate(myTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _taskService.EditStatus(currentSchedule.Id, this.Status.Id);
                btnSave.Visibility = Visibility.Collapsed;
                originalStatusId = this.Status.Id;
            }
            catch(Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }
        }

        private void cbEditStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Status == null)
            {
                btnSave.Visibility = Visibility.Collapsed;
                return;
            }
            if (originalStatusId != this.Status.Id)
            {
                btnSave.Visibility = Visibility.Visible;
            }
            else
            {
                btnSave.Visibility = Visibility.Collapsed;
            }
        }
    }
}
