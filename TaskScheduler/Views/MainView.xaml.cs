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
using TaskScheduler.Pages;
using TaskScheduler.Services;

namespace TaskScheduler.UserControls
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private readonly MyTasks _myTasks;
        private readonly CreateTask _createTask;
        public MainView(MyTasks myTasks, CreateTask createTask)
        {
            InitializeComponent();
            ArgumentNullException.ThrowIfNull(myTasks);
            ArgumentNullException.ThrowIfNull(createTask);
            _myTasks = myTasks;
            _createTask = createTask;
            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                SessionManager.StopSession();
                MainContentFrame.Content = null;
                mainWindow.ShowAuthAndRegistrView();
            }
        }

        private void btnCreateTask_Click(object sender, RoutedEventArgs e)
        {
            _createTask.ResetForm();
            MainContentFrame.Content = _createTask;
        }

        private void btnMyTasks_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string taskTypeTag = button.Tag.ToString();
                _myTasks.RefreshData(taskTypeTag);
            }
            MainContentFrame.Content = _myTasks;
        }
    }
}
