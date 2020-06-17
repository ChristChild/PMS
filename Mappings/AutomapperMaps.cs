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
            CreateMap<LeaveType, LeaveTypeViewModel>().ReverseMap();            
            CreateMap<LeaveAllocation, LeaveAllocationViewModel>().ReverseMap();
            CreateMap<LeaveAllocation, EditLeaveAllocationViewModel>().ReverseMap();
            CreateMap<LeaveRequest, LeaveRequestViewModel>().ReverseMap();
            CreateMap<Employee, EmployeeViewModel>().ReverseMap();


            CreateMap<LeaveAllocation, EmployeeLeaveRequestViewModel>().ReverseMap();
            CreateMap<LeaveType, EmployeeLeaveRequestViewModel>().ReverseMap();

            CreateMap<LeaveType, DownloadEmployeeLeaveRequestViewModel>().ReverseMap();
            CreateMap<LeaveType, DownloadEmployeeLeaveRequestViewModel>().ReverseMap();

            CreateMap<FileModel, FileModelViewModel>().ReverseMap();
            CreateMap<FileModel, FileUploadViewModel>().ReverseMap();
            CreateMap<LeaveType, FileUploadViewModel>().ReverseMap();

        }
    }
}
