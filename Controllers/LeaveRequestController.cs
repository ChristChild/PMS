using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using PMS.Settings;

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
        private readonly IMailService mailService;
        private readonly MailSettings _mailSettings;

        public LeaveRequestController(
            ILeaveTypeRepository leaverepo,
            ILeaveAllocationRepository leaveallocationrepo,
            ILeaveRequestRepository leaverequestrepo,
            IMapper mapper,
            IMailService mailService,
            UserManager<Employee> userManager,
            IOptions<MailSettings> mailSettings
            )
        {
            _leaverepo = leaverepo;
            _leaveallocationrepo = leaveallocationrepo;
            _leaverequestrepo = leaverequestrepo;
            _mapper = mapper;
            _userManager = userManager;
            //mailService = mailService;
            this.mailService = mailService;
            _mailSettings = mailSettings.Value;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> DownloadFile()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeid = employee.Id;
            var allocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployee(employeeid.ToString());
            var employeeRequests = await _leaverequestrepo.GetLeaveRequestByEmployee(employeeid.ToString());

            var allocationModel = _mapper.Map<List<LeaveAllocationViewModel>>(allocation);
           var employeeRequestModel = _mapper.Map<List<LeaveRequestViewModel>>(employeeRequests);

            var model = new DownloadEmployeeLeaveRequestViewModel
            {
                LeaveAllocations = allocationModel,
                LeaveRequests = employeeRequestModel

            };

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Leave Request");
                var currentRow = 1;
                
                #region Headering
                worksheet.Cell(currentRow, 1).Value = "Hello ";
                worksheet.Cell(currentRow+1, 2).Value = "Sweet P";
                worksheet.Cell(currentRow+2, 3).Value = "Smiles Girl";
                #endregion Headering

                #region Header
                worksheet.Cell(currentRow+3, 1).Value = "First Name";
                worksheet.Cell(currentRow+3, 2).Value = "Last Name";
                worksheet.Cell(currentRow+3, 3).Value = "Start Date";
                worksheet.Cell(currentRow+3, 4).Value = "Start Date";
                #endregion Header

                #region Body
                foreach (var leave in model.LeaveRequests)
                {
                    currentRow++;
                    worksheet.Cell(currentRow + 4, 1).Value = leave.Employee.Firstname;
                    worksheet.Cell(currentRow + 4, 2).Value = leave.Employee.Lastname;
                    worksheet.Cell(currentRow + 4, 3).Value = leave.StartDate;
                    worksheet.Cell(currentRow + 4, 4).Value = leave.LeaveType.Name;

                }
                #endregion Body

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "LeaveRequests.xlsx"
                        );

                }
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
                var userLoggedIn = await _userManager.GetUserAsync(User);

                try
                {
                    var mailInfo = new MailRequest
                    {
                        ToEmail = userLoggedIn.Email,
                        Subject = "Request Recieved",
                        Body = $"{userLoggedIn.Firstname}, We have recieved your request for {model.LeaveTypeId} for the period " +
                        $"{model.StartDate} to {model.EndDate}. Thank you. "
                };
                    await mailService.SendEmailAsync(mailInfo);



                  //  MailMessage message = new MailMessage();
                  //  SmtpClient smtp = new SmtpClient();
                  //  message.From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName);
                  //  message.To.Add(new MailAddress(userLoggedIn.Email));
                  //  message.Subject = "Request Recieved";

                  //  message.IsBodyHtml = false;
                  //  message.Body = $"Dear {userLoggedIn.Firstname}, We have recieved your request for {model.LeaveTypeId} for the period " +
                  //      $"{model.StartDate} to {model.EndDate}. Thank you. ";
                  //  smtp.Port = _mailSettings.Port;
                  //  smtp.Host = _mailSettings.Host;
                  //  smtp.EnableSsl = true;
                  //  smtp.UseDefaultCredentials = false;
                  //  smtp.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
                  //  smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                  //  //message.Headers.Add("Disposition-Notification-To", "b@technospine.com");

                  //  message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

                  // // message.ReplyTo = adminAddress;

                  // // smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                  ////  smtpClient.Send(message);
                  //  await smtp.SendMailAsync(message);

                }
                catch (Exception ex)
                {

                    throw;
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
