using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using HrApp.ViewModels;

namespace HrApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Console.WriteLine("LoadDepd");
        var vm = new MainWindowViewModel();
        DataContext = vm;
        // временно вызываем напрямую
        Task.Run(async () =>
        {
            await vm.LoadDepartmentsAsync();
            await vm.LoadEmployeesAsync();
        });
    }
}