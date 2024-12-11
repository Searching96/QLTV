using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class THAMSO
{
    public int ID { get; set; }

    public DateTime ThoiGian { get; set; }

    public int TuoiToiThieu { get; set; }

    public decimal TienPhatTraTreMotNgay { get; set; }
}
