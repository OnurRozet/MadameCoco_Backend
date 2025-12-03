using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Shared.BaseModels
{
    public class DetailListResponse<T>
    {
        public DetailListResponse() { SearchResult = []; }
        public List<T> SearchResult { get; set; }
        public int TotalItemCount { get; set; }
    }
}
