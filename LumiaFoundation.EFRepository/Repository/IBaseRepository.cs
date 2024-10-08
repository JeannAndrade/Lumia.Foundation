﻿using System.Linq.Expressions;
using LumiaFoundation.EFRepository.Domain;

namespace LumiaFoundation.EFRepository.Repository;

public interface IIdentityBaseRepository<T> where T : Entity
{
    IQueryable<T> FindAll(bool trackChanges);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
