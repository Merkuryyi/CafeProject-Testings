using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class ShiftInfo
    {
        public int ShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<ShiftEmployeeInfo> Employees { get; set; } = new List<ShiftEmployeeInfo>();
    }

    public class ShiftEmployeeInfo
    {
        public int UserId { get; set; }
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Patronymic { get; set; }
        public string Role { get; set; } = "";
        public string? TableNumber { get; set; }
    }

// Модель для создания смены
    public class CreateShiftData
    {
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<ShiftEmployeeData> Employees { get; set; } = new List<ShiftEmployeeData>();
    }

    public class ShiftEmployeeData
    {
        public int UserId { get; set; }
        public string? TableNumber { get; set; }
    }
}