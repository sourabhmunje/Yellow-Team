using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public int Length { get; set; }
        [Display(Name ="Parent Present")]
        public bool Parent { get; set; }
        public string ScheduledCheckIn { get; set; }
        public string ScheduledCheckout { get; set; }
        public string EstimatedCheckin { get; set; }
        public string EstimatedCheckout { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public int Priority { get; set; }
        public string created { get; set; }
        public int completed { get; set; }
        public int late { get; set; }
        public virtual int RoomId { get; set; }
        public virtual Rooms room { get; set; }

    }
}