using Microsoft.EntityFrameworkCore;
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
using TaskScheduler.Services;
using TaskScheduler.UserControls;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaskScheduler.Pages
{
    /// <summary>
    /// Логика взаимодействия для MyTasks.xaml
    /// </summary>
    public partial class MyTasks : Page
    {
        public List<Schedule>? TaskList { get; set; }
        public List<Status>? StatusList { get; set; }
        private readonly ITaskService _taskService;
        private string _taskTypeTag;
        public MyTasks(ITaskService taskService)
        {
            InitializeComponent();
            ArgumentNullException.ThrowIfNull(taskService);
            _taskService = taskService;
            StatusList = _taskService.GetStatusesForFilter();
            TasksListBox.ItemsSource = TaskList;
            cbStatusFilter.SelectedIndex = 0;
            cbSortBy.SelectedIndex = 0;
            this.DataContext = this;
        }
        private void TasksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksListBox.SelectedItem is Schedule selectedSchedule)
            {
                NavigationService.Navigate(new Task(selectedSchedule.Id, _taskService));
                TasksListBox.SelectedItem = null;
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_taskService == null || cbStatusFilter == null || cbSortBy == null)
            {
                return;
            }
            TaskList = _taskService.FilterChange(cbStatusFilter.SelectedIndex, cbSortBy.SelectedIndex, _taskTypeTag);
            TasksListBox.ItemsSource = TaskList;
        }
        public void RefreshData(string taskTypeTag)
        {
            _taskService.UpdateExpiredTasks();
            _taskService.UpdateCompletedTasks();
            _taskTypeTag = taskTypeTag;
            FilterChanged(null, null);
        }
    }
}
