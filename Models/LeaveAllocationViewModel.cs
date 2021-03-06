﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class LeaveAllocationViewModel
    {
        public int Id { get; set; }
        public int NumberOfDays { get; set; }
        public DateTime DateCreated { get; set; }

        public int Period { get; set; }

        public EmployeeViewModel Employee { get; set; }
        public string EmployeeId { get; set; }

        public LeaveTypeViewModel LeaveType { get; set; }
        public int LeaveTypeId { get; set; }

        //public IEnumerable<SelectListItem> Employees { get; set; }
        //public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }

    public class CreateAllocationViewModel
    {
        public int NumberUpdated { get; set; }
        public List<LeaveTypeViewModel> LeaveTypes { get; set; }

    }

    public class EditLeaveAllocationViewModel
    {
        public int Id { get; set; }
        public EmployeeViewModel Employee { get; set; }
        public string EmployeeId { get; set; }
        public int NumberOfDays { get; set; }
        public LeaveTypeViewModel LeaveType { get; set; }

    }
    public class ViewAllocationViewModel
    {
        public EmployeeViewModel Employee { get; set; }
        public string EmployeeId { get; set; }

        public List<LeaveAllocationViewModel> LeaveAllocation { get; set; }

    }
}
