using Microsoft.EntityFrameworkCore;
using PMS.Contracts;
using PMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Repository
{
    public class FileModelRepository : IFileModelRepository
    {
        private readonly ApplicationDbContext _db;
        public FileModelRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(FileModel entity)
        {
            await _db.FileModels.AddAsync(entity);
            return await Save();
        }

        public Task<bool> Delete(FileModel entity)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<FileModel>> FindAll()
        {
            return await _db.FileModels
                        .Include(q => q.LeaveType)
                        //.Include(q => q.)
                        //.Include(q => q.Employee)
                         .ToListAsync();

        }

        public Task<FileModel> FindByID(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isExists(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public Task<bool> Update(FileModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
