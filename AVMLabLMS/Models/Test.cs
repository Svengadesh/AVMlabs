using System.ComponentModel.DataAnnotations;

namespace AVMLabLMS.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TestCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string TestName { get; set; }

        [MaxLength(50)]
        public string SampleType { get; set; }

        public int TATHours { get; set; } // Turn Around Time in hours

        public decimal Rate { get; set; }

        public bool IsActive { get; set; }
    }
}
