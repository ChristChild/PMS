using AutoMapper;
using PMS.Data;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Mappings
{
    public class AutomapperMaps: Profile
    {
        public AutomapperMaps()
        {
            CreateMap<LeaveType, DetailsLeaveTypeViewModel>().ReverseMap();
            CreateMap<LeaveType, CreateLeaveTypeViewModel>().ReverseMap();
            
            CreateMap<LeaveAllocation, LeaveAllocationViewModel>().ReverseMap();
            CreateMap<LeaveHistory, LeaveHistoryViewModel>().ReverseMap();
            CreateMap<Employee, EmployeeViewModel>().ReverseMap();

        }
    }
}
