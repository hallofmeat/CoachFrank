using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoachFrank.Data.Models
{
    public class Warning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public ulong DiscordId { get; set; }

        public ulong IssuerId { get; set; }

        [Required]
        public string Reason { get; set; }

        public DateTime Timestamp { get; set; }

        public bool Removed { get; set; }
    }
}