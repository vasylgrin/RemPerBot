using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySuperUniversalBot_BL.Controller.ControllerBase
{
    public class DataBaseControllerBase<TEntity> where TEntity : class
    {
        DbContext _context;
        DbSet<TEntity> _dbSet;

        public DataBaseControllerBase(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public bool SaveDB(TEntity entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        public List<TEntity> LoadDB(out List<TEntity> entities)
        {
            entities = new List<TEntity>();
            return entities = _dbSet.ToList();
        }

        public List<TEntity> LoadDB()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public bool RemoveDB(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        public bool UpdateDB(TEntity entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        private bool CheckForSaveDB(TEntity entity)
        {
            // We receive reminders from the database and check with the incoming reminder.
            List<TEntity> Entityes = _dbSet.Where(x => x == entity).ToList();

            // Checking for null.
            if (Entityes != null || Entityes.Count != 0)
                return true;
            else
                return false;
        }
    }
}
