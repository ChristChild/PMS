using Microsoft.AspNetCore.Mvc.Rendering;
using PMS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class FileModelViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        [Required]
        public string Description { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string FilePath { get; set; }
        public LeaveType LeaveType { get; set; }
        public int LeaveTypeId { get; set; }


    }

    public class FileUploadViewModel
    {
        public List<FileModelViewModel> FilesOnFileSystem { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; }

        [Display(Name = "Leave Type")]
        public int LeaveTypeId { get; set; }
    }
}
