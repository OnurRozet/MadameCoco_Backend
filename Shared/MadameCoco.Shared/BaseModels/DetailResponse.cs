using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Shared.BaseModels
{
    public class DetailResponse<T>
    {
        public required T Detail { get; set; }
    }
}
