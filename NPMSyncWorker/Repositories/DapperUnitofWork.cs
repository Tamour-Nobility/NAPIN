using Microsoft.Extensions.Configuration;
using NPMSyncWorker.Repositories.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace NPMSyncWorker.Repositories
{
    internal class DapperUnitofWork : IUnitofWork
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private IPracticeRepository _practiceRepository;
        private IPracticeLocationsRepository _practiceLocationsRepository;
        private ISynchronizationRepository _synchronizationRepository;
        private ISynchronizationLogRepository _synchronizationLogRepository;
        private IPatientRepository _petientRepository;
        private IProviderRepository _providerRepository;
        private bool _disposed;

        public DapperUnitofWork(IConfiguration configuration)
        {
            _connection = new SqlConnection(configuration.GetConnectionString("npm"));
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public IPracticeRepository PracticeRepository
        {
            get { return _practiceRepository ?? (_practiceRepository = new PracticeReposiroty(_transaction)); }
        }

        public ISynchronizationRepository SynchronizationRepository
        {
            get { return _synchronizationRepository ?? (_synchronizationRepository = new SynchronizationRepository(_transaction)); }
        }

        public ISynchronizationLogRepository SynchronizationLogRepository
        {
            get { return _synchronizationLogRepository ?? (_synchronizationLogRepository = new SynchronizationLogRepository(_transaction)); }
        }

        public IPatientRepository PatientRepository
        {
            get { return _petientRepository ?? (_petientRepository = new PatientReposiroty(_transaction)); }
        }

        public IProviderRepository ProviderRepository
        {
            get { return _providerRepository ?? (_providerRepository = new ProviderRepository(_transaction)); }
        }

        public IPracticeLocationsRepository PracticeLocationsRepository
        {
            get { return _practiceLocationsRepository ?? (_practiceLocationsRepository = new PracticeReposiroty(_transaction)); }
        }

        public void SaveChanges()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = _connection.BeginTransaction();
                resetRepositories();
            }
        }

        private void resetRepositories()
        {
            _practiceRepository = null;
            _synchronizationRepository = null;
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }

        ~DapperUnitofWork()
        {
            dispose(false);
        }
    }
}
