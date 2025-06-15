using System.Collections.Generic;
using System.Threading.Tasks;
using HrApp.Models;
using Npgsql;

namespace HrApp.service;

public class EmployeeService
{
    private readonly string _connectionString;

    public EmployeeService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        var list = new List<Employee>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, firstname, lastname, patronymic, departmentid FROM employee", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Employee
            {
                Id = reader.GetInt64(0),
                Firstname = reader.GetString(1),
                Lastname = reader.GetString(2),
                Patronymic = reader.GetString(3),
                DepartmentId = reader.GetInt32(4)
            });
        }

        return list;
    }
}