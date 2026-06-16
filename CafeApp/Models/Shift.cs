using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class Shift
    {
        public int ShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<User> AssignedUsers { get; set; } = new();
    }
}