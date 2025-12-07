using System;
using System.Collections.Generic;

namespace TaskScheduler.Models;

public partial class Schedule
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int RecurrenceTypeid { get; set; }

    public DateTime DueDate { get; set; }

    public int? StatusId { get; set; }

    public virtual Recurrence RecurrenceType { get; set; } = null!;

    public virtual Status? Status { get; set; }

    public virtual Task Task { get; set; } = null!;
}
