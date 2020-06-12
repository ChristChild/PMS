using PMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Contracts
{
    public interface ILeaveRequestRepository: IRepositoryBase<LeaveRequest>
    {
         Task<ICollection<LeaveRequest>> GetLeaveRequestByEmployee(string id);

    }
}
