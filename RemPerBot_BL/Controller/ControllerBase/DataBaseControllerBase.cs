using Microsoft.EntityFrameworkCore;
using RemBerBot_BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public bool Save(TEntity entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        public List<TEntity> Load()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public bool Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        public bool Update(TEntity entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();

            return CheckForSaveDB(entity);
        }

        private bool CheckForSaveDB(TEntity entity)
        {
            List<TEntity> Entityes = _dbSet.Where(x => x == entity).ToList();

            if (Entityes != null || Entityes.Count != 0)
                return true;
            else
                return false;
        }
    }
}
