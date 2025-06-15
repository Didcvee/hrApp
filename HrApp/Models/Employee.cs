namespace HrApp.Models;

public class Employee
{
    public long Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Patronymic { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }

    public string FullName => $"{Lastname} {Firstname} {Patronymic}";
}