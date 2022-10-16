using Microsoft.EntityFrameworkCore;
using RemBerBot_BL.Controller.DataBase;

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

        public void Save(TEntity entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public List<TEntity> Load()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
        }
    }
}
