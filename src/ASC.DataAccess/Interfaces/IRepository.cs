﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess.Interfaces
{
    public interface IRepository<T> where T : TableEntity
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> FindAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> FindAllAsync(string partitionkey);
        Task CreateTableAsync();
    }
}
