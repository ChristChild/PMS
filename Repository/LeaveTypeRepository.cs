using PMS.Contracts;
using PMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PMS.Repository
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly ApplicationDbContext _db;
        public LeaveTypeRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public bool Create(LeaveType entity)
        {
            _db.LeaveTypes.Add(entity);
            //Save
            return Save();
        }

        public bool Delete(LeaveType entity)
        {
            _db.LeaveTypes.Remove(entity);
            return Save();
        }

        public ICollection<LeaveType> FindAll()
        {
            var leaveType = _db.LeaveTypes.ToList();
            return leaveType;
        }

        public LeaveType FindByID(int id)
        {
            var leaveType = _db.LeaveTypes.Find(id);
            return leaveType;
        }

        public ICollection<LeaveType> GetEmployeeByLeaveType(int id)
        {
            throw new NotImplementedException();
        }

        public bool isExists(int id)
        {
            //cool way to check if a table is empty
            //var exists = _db.LeaveHistories.Any();
            //returns true if table has value

            var exists = _db.LeaveTypes.Any(q =>q.Id == id);
            return exists;
        }

        public bool Save()
        {
           var changes= _db.SaveChanges() ;
            return changes > 0;
        }

        public bool Update(LeaveType entity)
        {
            _db.LeaveTypes.Update(entity);
            //Save
            return Save();
        }
    }
}
