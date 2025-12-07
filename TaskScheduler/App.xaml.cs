using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Windows;
using TaskScheduler.Interfaces;
using TaskScheduler.Models;
using TaskScheduler.Pages;
using TaskScheduler.Services;
using TaskScheduler.UserControls;

namespace TaskScheduler
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataBaseContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Port=5432;Database=TaskScheduler;Username=postgres;Password=123");
            }, ServiceLifetime.Singleton);
            services.AddSingleton<ISessionService, SessionAdapter>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ITaskService, TaskService>();
            services.AddTransient<Autho>();
            services.AddTransient<Registr>();
            services.AddTransient<MyTasks>();
            services.AddTransient<CreateTask>();
            services.AddSingleton<AuthAndRegistrView>();
            services.AddSingleton<MainView>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}