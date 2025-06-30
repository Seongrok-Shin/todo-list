using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ToDoWebApp.Models
{
    [Table("todos")]
    public class ToDoItems : BaseModel
    {
        [PrimaryKey("id", false)] 
        public Guid Id { get; set; }

        [Column("user_id")] 
        public Guid UserId { get; set; }

        [Column("status")] 
        public string Status { get; set; } = string.Empty;

        [Column("category")] 
        public string Category { get; set; } = string.Empty;

        [Column("description")] 
        public string Description { get; set; } = string.Empty;

        [Column("created_at")] 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")] 
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public bool IsCompleted => Status == "DONE";

        [JsonIgnore]
        [IgnoreDataMember]
        public bool IsSelected { get; set; } = false;
    }
}