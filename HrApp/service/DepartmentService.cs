using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data;
using HrApp.Models;

namespace HrApp.service;

using Npgsql;

public class DepartmentService
{
    private readonly string _connectionString;

    public DepartmentService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        var list = new List<Department>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name, parentid, description FROM department", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Department
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                ParentId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return BuildTree(list);
    }

    private List<Department> BuildTree(List<Department> flatList)
    {
        var lookup = flatList.ToDictionary(d => d.Id);
        var roots = new List<Department>();

        foreach (var dep in flatList)
        {
            // ✅ если он сам себе родитель — добавляем в корни и больше ничего не делаем
            if (dep.ParentId == dep.Id)
            {
                roots.Add(dep);
                continue; // <- важно! чтобы не вставлять в самого себя
            }

            if (dep.ParentId.HasValue)
            {
                if (lookup.TryGetValue((long)dep.ParentId.Value, out var parent))
                {
                    parent.Children.Add(dep);
                }
                else
                {
                    roots.Add(dep); // нет родителя
                }
            }
            else
            {
                roots.Add(dep); // root без parentid
            }
        }
        
        Console.WriteLine($"Всего записей: {flatList.Count}");
        Console.WriteLine($"Корней найдено: {roots.Count}");
        foreach (var root in roots)
        {
            PrintTree(root, 0);
        }

        return roots;
    }
    
    private void PrintTree(Department dept, int indent)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}- {dept.Name} ({dept.Id})");

        foreach (var child in dept.Children)
        {
            PrintTree(child, indent + 1);
        }
    }
}
