using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Token
    {
        public int Id { get; set; }
        public string? TokenValue { get; set; }
        public bool? IsUse { get; set; }
        public DateTime? ExpiredTime { get; set; }
        public string? Type { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual User? User { get; set; }
    }
}
