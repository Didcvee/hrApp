using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HrApp.Models;

public class Department
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? ParentId { get; set; }
    public ObservableCollection<Department> Children { get; set; } = new();
}