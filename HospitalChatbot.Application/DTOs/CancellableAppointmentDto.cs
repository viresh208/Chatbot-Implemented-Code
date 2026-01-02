using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalChatbot.Application.DTOs
{
    public class CancellableAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } 
    }
}
