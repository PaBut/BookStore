using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
//using BookStore.DataAccess.Repository.IRepository;

namespace BookStore.DataAccess.Repository
{
    public class Repository<T> : IRepository.IRepository<T> where T : class
    {
        protected readonly DbContextApp _context;
        internal DbSet<T> dbSet;

        public Repository(DbContextApp context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        //IncludeProp - "Category,CoverType" 
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            includePropertiesFunc(ref query, includeProperties);
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);

            includePropertiesFunc(ref query, includeProperties);
            
            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        void includePropertiesFunc(ref IQueryable<T> query, string? includeProperties)
        {
            if (includeProperties != null)
            {
                foreach (string propertyName in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(propertyName);
                }
            }
        }
    }
}
