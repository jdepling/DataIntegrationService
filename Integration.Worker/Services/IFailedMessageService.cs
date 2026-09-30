using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integration.Data;

namespace Integration.Worker.Services
{
    public interface IFailedMessageService
    {
        Task SaveAsync(FailedMessage failedMessage, CancellationToken cancellationToken);
    }
}
