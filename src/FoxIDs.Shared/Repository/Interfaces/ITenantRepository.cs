﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FoxIDs.Models;

namespace FoxIDs.Repository
{
    public interface ITenantRepository
    {
        Task<bool> ExistsAsync<T>(string id) where T : IDataDocument;
        Task<T> GetAsync<T>(string id, bool required = true, bool delete = false) where T : IDataDocument;
        Task<Tenant> GetTenantByNameAsync(string tenantName, bool required = true);
        Task<Track> GetTrackByNameAsync(Track.IdKey idKey, bool required = true);
        Task<UpParty> GetUpPartyByNameAsync(Party.IdKey idKey, bool required = true);
        Task<DownParty> GetDownPartyByNameAsync(Party.IdKey idKey, bool required = true);

        Task<HashSet<T>> GetListAsync<T>(Track.IdKey idKey = null, Expression<Func<T, bool>> whereQuery = null, int maxItemCount = 50) where T : IDataDocument;

        /// <summary>
        /// Create document. Throws exception if already exists.
        /// </summary>
        Task CreateAsync<T>(T item) where T : IDataDocument;
        /// <summary>
        /// Update document. Throws exception if not exists.
        /// </summary>
        Task UpdateAsync<T>(T item) where T : IDataDocument;
        /// <summary>
        /// Create or update document.
        /// </summary>
        Task SaveAsync<T>(T item) where T : IDataDocument;
        /// <summary>
        /// Delete document. Throws exception if not already exists.
        /// </summary>
        Task<T> DeleteAsync<T>(string id) where T : IDataDocument;
        Task<T> DeleteAsync<T>(Track.IdKey idKey, Expression<Func<T, bool>> whereQuery = null) where T : IDataDocument;
        Task<int> DeleteListAsync<T>(Track.IdKey idKey, Expression<Func<T, bool>> whereQuery = null) where T : IDataDocument;
    }
}