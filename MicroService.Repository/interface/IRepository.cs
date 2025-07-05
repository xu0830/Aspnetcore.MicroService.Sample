using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Repository.@interface
{
    public interface IRepository<T, TDbContext> where T : class where TDbContext : DbContext
    {
        /// <summary>
        /// 返回一个单独的实体，如果记录多于1个则取第一个
        /// </summary>
        T FirstOrDefault(Expression<Func<T, bool>> exp = null);
        /// <summary>
        /// 判断指定条件的记录是否存在
        /// </summary>
        bool Any(Expression<Func<T, bool>> exp);
        IQueryable<T> Find(Expression<Func<T, bool>> exp = null);
        IQueryable<T> Find(int pageindex = 1, int pagesize = 10, string orderby = "",
            Expression<Func<T, bool>> exp = null);
        int Count(Expression<Func<T, bool>> exp = null);
        void Add(T entity);
        void BatchAdd(T[] entities);
        /// <summary>
        /// 更新一个实体的所有属性
        /// </summary>
        void Update(T entity);
        void Delete(T entity);


        /// <summary>
        /// 实现按需要只更新部分更新
        /// <para>如：Update(u =>u.Id==1,u =>new User{Name="ok"});</para>
        /// </summary>
        /// <param name="where">更新条件</param>
        /// <param name="entity">更新后的实体</param>
        void Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> entity);

        /// <summary>
        /// 批量删除
        /// </summary>
        void Delete(Expression<Func<T, bool>> exp);

        void Save();

        int ExecuteSqlRaw(string sql);

        /// <summary>
        /// 使用SQL脚本查询
        /// </summary>
        /// <returns></returns>
        IQueryable<T> FromSql(string sql, params object[] parameters);

        #region 异步接口

        Task<int> ExecuteSqlRawAsync(string sql);
        Task<int> AddAsync(T entity);
        Task<int> BatchAddAsync(T[] entities);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<int> SaveAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> exp = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> exp);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> exp);

        #endregion
    }
}
