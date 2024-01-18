using HangfireWatermarker.Data;
using HangfireWatermarker.Models;
using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using System;

namespace HangfireWatermarker.Repositories
{
    public class SqlJobStatusRepository : IJobStatusRepository
    {
        private readonly AppDbContext _dbContext;

        public SqlJobStatusRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public JobItem GetJobItem(Guid jobId)
        {
            return _dbContext.JobItems.SingleOrDefault(ji => ji.JobId == jobId);
        }

        public void AddJobItem(JobItem jobItem)
        {
            _dbContext.JobItems.Add(jobItem);
            _dbContext.SaveChanges();
        }

        public void UpdateJobItem(JobItem jobItem)
        {
            try
            {
                //var originalJobItem = GetJobItem(jobItem.JobId);

                //_dbContext.Entry(jobItem).State = EntityState.Detached;

                // Ensure that the version in the database matches the version of the entity being updated
                _dbContext.Entry(jobItem).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflict
                var entry = ex.Entries.Single();
                var databaseValues = entry.GetDatabaseValues();

                if (databaseValues == null)
                {
                    // The record has been deleted by another user
                    throw new Exception("The record you attempted to edit was deleted by another user.");
                }
                else
                {
                    // The record has been modified by another user
                    var databaseEntry = databaseValues.ToObject();
                    entry.OriginalValues.SetValues(databaseValues);
                    entry.CurrentValues.SetValues(databaseEntry);

                    // Retry the update
                    _dbContext.SaveChanges();
                }
            }
        }

        public List<JobItem> GetAllJobItems()
        {
            return _dbContext.JobItems.ToList();
        }
    }
}
