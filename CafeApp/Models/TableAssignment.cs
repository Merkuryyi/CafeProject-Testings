using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class TableAssignment
    {
        public int AssignmentId { get; set; }
        public int TableId { get; set; }
        public int UserId { get; set; }
        public int ShiftId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public User? User { get; set; }
        public CafeTable? Table { get; set; }
        public Shift? Shift { get; set; }
    }
}