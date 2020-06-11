using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Contracts;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository _leaverepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(
            ILeaveTypeRepository leaverepo, 
            ILeaveAllocationRepository leaveallocationrepo, 
            IMapper mapper,
            UserManager<Employee> userManager)
        {
            _leaverepo = leaverepo;
            _leaveallocationrepo = leaveallocationrepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveAllocationController
        public ActionResult Index()
        {
            var leavetypes = _leaverepo.FindAll().ToList();
            var mappedLeavetypes = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leavetypes);
            var model = new CreateAllocationViewModel
            {
                LeaveTypes = mappedLeavetypes,
                NumberUpdated = 0
        };
            return View(model);

        }

        public ActionResult SetLeave(int id)
        {
            var leavetype = _leaverepo.FindByID(id);
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            foreach (var emp in employees)
            {
                if (_leaveallocationrepo.CheckAllocation(id, emp.Id))
                    continue;
                var allocation = new LeaveAllocationViewModel
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays= leavetype.DefaultDays,
                    Period = DateTime.Now.Year,
                };
                var leaveallocation = _mapper.Map<LeaveAllocation>(allocation);
                _leaveallocationrepo.Create(leaveallocation);
            }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult ListEmployees()
        {
            var employees = _userManager.GetUsersInRoleAsync("Employee").Result;
            var model = _mapper.Map<List<EmployeeViewModel>>(employees);
            return View(model);
        }

            // GET: LeaveAllocationController/Details/5
        public ActionResult Details(string id)
        {
            var employee = _userManager.FindByIdAsync(id).Result;
            var employeemodel = _mapper.Map<EmployeeViewModel>(employee);
            var allocation = _leaveallocationrepo.GetLeaveAllocationsByEmployee(id);
            var allocationmodel = _mapper.Map<List<LeaveAllocationViewModel>>(allocation);
            var model = new ViewAllocationViewModel
            {
                Employee = employeemodel,
                LeaveAllocation = allocationmodel,
            
            
            };
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public ActionResult Edit(int id)
        {
            var leaveallocation = _leaveallocationrepo.FindByID(id);
            var model = _mapper.Map<EditLeaveAllocationViewModel>(leaveallocation);
            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EditLeaveAllocationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var record = _leaveallocationrepo.FindByID(id);
                record.NumberOfDays = model.NumberOfDays;             
                var isSuccess =_leaveallocationrepo.Update(record);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Error while saving");
                    return View(model);
                }

                return RedirectToAction(nameof(Details), new {id=model.EmployeeId });
            }
            catch
            {
                ModelState.AddModelError("", "Error while saving");
                return View(model);
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
