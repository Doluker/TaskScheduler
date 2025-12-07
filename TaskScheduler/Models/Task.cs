using System;
using System.Collections.Generic;

namespace TaskScheduler.Models;

public partial class Task
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int TaskTypeId { get; set; }

    public int PriorityId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Priority Priority { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual TaskType TaskType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
