using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HrApp.Models;
using HrApp.service;

namespace HrApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Department> Departments { get; set; } = new();
    public ObservableCollection<Employee> AllEmployees { get; set; } = new();
    public ObservableCollection<Employee> DepartmentEmployees { get; set; } = new();

    private Department _selectedDepartment;
    public Department SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (_selectedDepartment != value)
            {
                _selectedDepartment = value;
                OnPropertyChanged();
                FilterEmployeesByDepartment();
            }
        }
    }

    public async Task LoadDepartmentsAsync()
    {
        var departmentService = new DepartmentService("Host=localhost;Port=5432;Username=postgres;Password=2121;Database=hr");
        var result = await departmentService.GetDepartmentsAsync();

        Departments.Clear();
        foreach (var dep in result)
        {
            Departments.Add(dep);
        }
    }

    public async Task LoadEmployeesAsync()
    {
        var employeeService = new EmployeeService("Host=localhost;Port=5432;Username=postgres;Password=2121;Database=hr");
        var employees = await employeeService.GetEmployeesAsync();

        AllEmployees.Clear();
        foreach (var emp in employees)
        {
            AllEmployees.Add(emp);
        }
    }

    private void FilterEmployeesByDepartment()
    {
        DepartmentEmployees.Clear();

        if (SelectedDepartment == null)
            return;

        foreach (var emp in AllEmployees.Where(e => e.DepartmentId == SelectedDepartment.Id))
        {
            DepartmentEmployees.Add(emp);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}