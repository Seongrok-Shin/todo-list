using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TodoBlazorSupabaseApp.Models
{
    [Table("todos")]
    public class TodoItem : BaseModel
    {
        [PrimaryKey("id", false)] public Guid Id { get; set; }
        [Column("user_id")] public Guid UserId { get; set; }

        [Column("status")]
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "WIP";

        [Column("category")] public string? Category { get; set; }

        [Column("description")]
        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}