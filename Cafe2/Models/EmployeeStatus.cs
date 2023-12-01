using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class EmployeeStatus
{
    public int IdemployeeStatus { get; set; }

    public string NameStatus { get; set; } = null!;

    public virtual ICollection<HistoryUser> HistoryUsers { get; set; } = new List<HistoryUser>();
}
