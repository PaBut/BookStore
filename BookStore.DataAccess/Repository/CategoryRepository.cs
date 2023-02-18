using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, IRepository.ICategoryRepository
    {
        
        public CategoryRepository(DbContextApp context) : base(context)
        {
        }

        public void Update(Category obj)
        {
            base._context.Categories.Update(obj);
        }
    }
}
