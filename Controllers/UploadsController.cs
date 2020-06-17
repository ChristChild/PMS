using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PMS.Contracts;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    public class UploadsController : Controller
    {
        private readonly ILeaveTypeRepository _leaverepo;
        private readonly IFileModelRepository _fileuploadrepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;
        // GET: UploadsController

        public UploadsController(
            ILeaveTypeRepository leaverepo,
            IFileModelRepository fileuploadrepo,
            IMapper mapper,
            UserManager<Employee> userManager)
        {
            _leaverepo = leaverepo;
            _fileuploadrepo = fileuploadrepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ActionResult> IndexAsync()
        {
            var files = await _fileuploadrepo.FindAll();
            //var types = await _leaverepo.FindAll();
            var filedmodel = _mapper.Map<List<FileModel>, List<FileModelViewModel>>(files.ToList());
           // var typedmodel = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(types.ToList());

            var leavetypes = await _leaverepo.FindAll();
            var LeaveTypeItems = leavetypes.Select(q => new SelectListItem
            {
                Text = q.Name,
                Value = q.Id.ToString()

            });

            var model = new FileUploadViewModel
            {
                FilesOnFileSystem = filedmodel,
                LeaveTypes= LeaveTypeItems

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadToFileSystem(List<IFormFile> files, string description, FileUploadViewModel model)
        //public async Task<IActionResult> UploadToFileSystem(List<IFormFile> files, string description)
        {
            foreach (var file in files)
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\");
                bool basePathExists = System.IO.Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(basePath, file.FileName);
                var extension = Path.GetExtension(file.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileModel = new FileModelViewModel
                    {
                        CreatedOn = DateTime.UtcNow,
                        FileType = file.ContentType,
                        Extension = extension,
                        Name = fileName,
                        Description = description,
                        FilePath = filePath,
                        LeaveTypeId=model.LeaveTypeId
                    };

                    var fileupload = _mapper.Map<FileModel>(fileModel);
                    var isSuccess = await _fileuploadrepo.Create(fileupload);
                    if (!isSuccess)
                    {
                        ModelState.AddModelError("", "Something went wrong");
                        return RedirectToAction(nameof(Index));
                    }

                    // context.FilesOnFileSystem.Add(fileModel);
                    // context.SaveChanges();
                }
            }

            TempData["Message"] = "File successfully uploaded to File System.";
            return RedirectToAction("Index");
        }


        //public async Task<IActionResult> DownloadFileFromFileSystem(int id)
        //{

        //    var file = await context.FilesOnFileSystem.Where(x => x.Id == id).FirstOrDefaultAsync();
        //    if (file == null) return null;
        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(file.FilePath, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
        //    return File(memory, file.FileType, file.Name + file.Extension);
        //}


        // GET: UploadsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UploadsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UploadsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }

        // GET: UploadsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UploadsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }

        // GET: UploadsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UploadsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(IndexAsync));
            }
            catch
            {
                return View();
            }
        }
    }
}
