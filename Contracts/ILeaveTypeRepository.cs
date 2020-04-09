using PMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Contracts
{
    public interface ILeaveTypeRepository: IRepositoryBase<LeaveType>
    {
        ICollection<LeaveType> GetEmployeeByLeaveType(int id);
    }
}
