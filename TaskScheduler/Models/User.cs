using System;
using System.Collections.Generic;

namespace TaskScheduler.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string HashPassword { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
