using System;
using System.Collections.Generic;

namespace Cafe2.Models;

public partial class Order
{
    public int Idorder { get; set; }

    public int IdtableNumber { get; set; }

    //public int IddetailOrderId { get; set; }

    public DateTime DateOrder { get; set; }

    public int IdreadyStatus { get; set; }

    public int IdpaymentStatus { get; set; }

    public int IdpaymentMethod { get; set; }

    public int Ammount { get; set; }

    public virtual ICollection<DishInOrder> DishInOrders { get; set; } = new List<DishInOrder>();

    public virtual PymentMethod IdpaymentMethodNavigation { get; set; } = null!;

    public virtual PymentStatus IdpaymentStatusNavigation { get; set; } = null!;

    public virtual ReadyStatus IdreadyStatusNavigation { get; set; } = null!;

    public virtual Table IdtableNumberNavigation { get; set; } = null!;
}
