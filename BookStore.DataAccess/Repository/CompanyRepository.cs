using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, IRepository.ICompanyRepository
    {
        
        public CompanyRepository(DbContextApp context) : base(context)
        {
        }

        public void Update(Company obj)
        {
            base._context.Companies.Update(obj);
        }
    }
}
