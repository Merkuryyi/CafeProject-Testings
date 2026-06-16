namespace CafeApp.Models
{
    public class ShiftAssignment
    {
        public int AssignmentId { get; set; }
        public int ShiftId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public Shift? Shift { get; set; }
    }
}