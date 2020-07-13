﻿using NextLevelTrainingApi.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// The repository for the user model.
        /// </summary>
        IGenericRepository<Users> UserRepository { get; }
    }
}