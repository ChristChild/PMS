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
        public async Task<bool> Create(LeaveRequest entity)
        {
            await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            return await _db.LeaveRequests
                .Include(q => q.LeaveType)
                .Include(q => q.ApprovedBy)
                .Include(q => q.Employee)
                .ToListAsync();
        }

        public async Task<LeaveRequest> FindByID(int id)
        {
            return await _db.LeaveRequests
                .Include(q => q.Employee)
                .Include(q => q.LeaveType)
                .Include(q => q.ApprovedBy)
                .FirstOrDefaultAsync(q => q.Id==id);
        }

        public async Task<ICollection<LeaveRequest>> GetLeaveRequestByEmployee(string id)
        {
            //var period = DateTime.Now.Year;
            var leaverequests = await FindAll();
             return leaverequests.Where(q => q.RequestingEmployeeId == id)
                .ToList();
        }

        public async Task<bool> isExists(int id)
        {
            //cool way to check if a table is empty
            //var exists = _db.LeaveRequests.Any();
            //returns true if table has value

            var exists = await _db.LeaveRequests.AnyAsync(q => q.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
             _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
