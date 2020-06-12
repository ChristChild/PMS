using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PMS.Data;
using PMS.Models;

namespace PMS.Contracts
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveTypeRepository _leaverepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly ILeaveRequestRepository _leaverequestrepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(
            ILeaveTypeRepository leaverepo,
            ILeaveAllocationRepository leaveallocationrepo,
            ILeaveRequestRepository leaverequestrepo,
            IMapper mapper,
            UserManager<Employee> userManager)
        {
            _leaverepo = leaverepo;
            _leaveallocationrepo = leaveallocationrepo;
            _leaverequestrepo = leaverequestrepo;
            _mapper = mapper;
            _userManager = userManager;
        }
        [Authorize(Roles ="Administrator")]
        // GET: LeaveRequestController
        public async Task<ActionResult> Index()
        {
            var leaverequest = await _leaverequestrepo.FindAll();
            var leaveRequestModel = _mapper.Map<List<LeaveRequestViewModel>>(leaverequest);
            var model = new AdminLeaveRequestViewModel
            {
                TotalRequest= leaveRequestModel.Count,
                ApprovedRequest=leaveRequestModel.Count(q => q.Approved==true),
                PendingRequest = leaveRequestModel.Count(q => q.Approved==null),
                RejectedRequest = leaveRequestModel.Count(q => q.Approved==false),
                LeaveRequests = leaveRequestModel

            };
            return View(model);
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeid = employee.Id;
            var allocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployee(employeeid);
            var employeeRequests = await _leaverequestrepo.GetLeaveRequestByEmployee(employeeid);

            var allocationModel = _mapper.Map<List<LeaveAllocationViewModel>>(allocation);
            var employeeRequestModel = _mapper.Map<List<LeaveRequestViewModel>>(employeeRequests);

            var model = new EmployeeLeaveRequestViewModel
            {
                LeaveAllocations = allocationModel,
                LeaveRequests = employeeRequestModel

            };
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaverequest = await _leaverequestrepo.FindByID(id);
            var model = _mapper.Map<LeaveRequestViewModel>(leaverequest);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                var leaverequest = await _leaverequestrepo.FindByID(id);
                leaverequest.Approved = true;
                leaverequest.ApprovedById = userLoggedIn.Id;
                leaverequest.DateActioned = DateTime.Now;
                int DaysRequested = (int)(leaverequest.EndDate - leaverequest.StartDate).TotalDays;

                var employeeid = leaverequest.RequestingEmployeeId;
                var leavetypeid = leaverequest.LeaveTypeId;
                var allocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployeeAndType(employeeid, leavetypeid);
                allocation.NumberOfDays = allocation.NumberOfDays - DaysRequested;


                await _leaverequestrepo.Update(leaverequest);
                await _leaveallocationrepo.Update(allocation);

                    return RedirectToAction(nameof(Index));
               
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }       
        }


        public async Task<ActionResult> RejectRequest(int id)
        {

            try
            {
                var userLoggedIn = await _userManager.GetUserAsync(User);
                var leaverequest = await _leaverequestrepo.FindByID(id);
                leaverequest.Approved = false;
                leaverequest.ApprovedById = userLoggedIn.Id;
                leaverequest.DateActioned = DateTime.Now;

                await _leaverequestrepo.Update(leaverequest);
              
                return RedirectToAction(nameof(Index));
               
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leavetypes = await _leaverepo.FindAll();
            var LeaveTypeItems = leavetypes.Select(q => new SelectListItem
            {
                Text=q.Name,
                Value= q.Id.ToString()

            });
            var model = new CreateLeaveRequestViewModel
            {
                LeaveTypes = LeaveTypeItems
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestViewModel model)
        {
            
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);

                //load dropdown 
                var leavetypes = await _leaverepo.FindAll();
                var LeaveTypeItems = leavetypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()

                });
                model.LeaveTypes = LeaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                //compare dates
                if(DateTime.Compare(startDate, endDate) > 1 )
                {
                    ModelState.AddModelError("", "Start date cannot be further in the future than End date");
                    return View(model);
                }

                //current user
                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);

                int DaysRequested = (int)(endDate - startDate).TotalDays;
                if (DaysRequested> allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficent days for this request");
                    return View(model);
                }

                var leaverequestmodel = new LeaveRequestViewModel
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved=null,
                    DateRequested=DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId=model.LeaveTypeId,
                    RequestComment=model.RequestComment
                };
                var leaverequest = _mapper.Map<LeaveRequest>(leaverequestmodel);
                var isSuccess = await _leaverequestrepo.Create(leaverequest);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(model);
                }
            

                return RedirectToAction("MyLeave");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View();
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
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
