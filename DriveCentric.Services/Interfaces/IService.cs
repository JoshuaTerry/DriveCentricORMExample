using DriveCentric.Shared.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DriveCentric.Services.Interfaces
{
    public interface IService<T>  
    {
        IDataResponse<IEnumerable<ICanTransmogrify>> GetAll();
        IDataResponse<IEnumerable<ICanTransmogrify>> GetAll(string fields, IPageable search = null);
        IDataResponse<T> GetById(Guid id);
        IDataResponse Update(T entity);
        IDataResponse<T> Update(Guid id, JObject changes);
        IDataResponse<T> Update(T entity, JObject changes);
        IDataResponse<T> Add(T entity);
        IDataResponse Delete(T entity);
        IDataResponse<T> GetWhereExpression(Expression<Func<T, bool>> expression);
        IDataResponse<IEnumerable<ICanTransmogrify>> GetAllWhereExpression(Expression<Func<T, bool>> expression, IPageable search = null, string fields = null);
        Expression<Func<T, object>>[] IncludesForSingle { set; }
        Expression<Func<T, object>>[] IncludesForList { set; }

    }
}
