using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
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

namespace TaskScheduler.Pages
{
    /// <summary>
    /// Логика взаимодействия для CreateTask.xaml
    /// </summary>
    public partial class CreateTask : Page
    {
        private readonly ITaskService _taskService ;
        public CreateTask(ITaskService taskService)
        {
            InitializeComponent();
            ArgumentNullException.ThrowIfNull(taskService);
            _taskService = taskService;
        }

        private void btnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(tbTitle.Text) && dpDueDate.SelectedDate.HasValue)
                {
                    _taskService.CreateTask(tbTitle.Text, tbDescription.Text, cbTaskType.SelectedIndex + 1, cbPriority.SelectedIndex + 1, dpDueDate.SelectedDate.Value, cbRecurrence.SelectedIndex + 1);
                    MessageBox.Show("Успешное создание задачи!");
                    ResetForm();
                }
                else
                {
                    MessageBox.Show("Заполните обязательные поля!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void ResetForm()
        {
            tbTitle.Clear();
            tbDescription.Clear();
            dpDueDate.SelectedDate = null;
            cbPriority.SelectedIndex = 0;
            cbRecurrence.SelectedIndex = 0;
            cbTaskType.SelectedIndex = 0;
        }
    }
}
