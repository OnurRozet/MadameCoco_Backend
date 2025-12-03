using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadameCoco.Audit.Worker.Interfaces
{
    public interface ILogReportingService
    {
        Task SendDailyReportAsync();
    }
}
