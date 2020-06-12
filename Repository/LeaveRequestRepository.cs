using Microsoft.EntityFrameworkCore;
using PMS.Contracts;
using PMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;
        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public bool Create(LeaveRequest entity)
        {
            _db.LeaveRequests.Add(entity);
            return Save();
        }

        public bool Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return Save();
        }

        public ICollection<LeaveRequest> FindAll()
        {
            return _db.LeaveRequests
                .Include(q => q.LeaveType)
                .Include(q => q.ApprovedBy)
                .Include(q => q.Employee)
                .ToList();
        }

        public LeaveRequest FindByID(int id)
        {
            return _db.LeaveRequests
                .Include(q => q.Employee)
                .Include(q => q.LeaveType)
                .Include(q => q.ApprovedBy)
                .FirstOrDefault(q => q.Id==id);
        }

        public ICollection<LeaveRequest> GetLeaveRequestByEmployee(string id)
        {
            //var period = DateTime.Now.Year;
            return FindAll()
                .Where(q => q.RequestingEmployeeId == id)
                .ToList();
        }

        public bool isExists(int id)
        {
            //cool way to check if a table is empty
            //var exists = _db.LeaveRequests.Any();
            //returns true if table has value

            var exists = _db.LeaveRequests.Any(q => q.Id == id);
            return exists;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return Save();
        }
    }
}
