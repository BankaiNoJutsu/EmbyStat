﻿using System.Linq;
using EmbyStat.Common.Models.Tasks;
using EmbyStat.Common.Models.Tasks.Enum;
using EmbyStat.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmbyStat.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        public TaskResult GetTaskResultById(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.TaskResults.SingleOrDefault(x => x.Id == id);
            }
        }

        public void AddOrUpdateTaskResult(TaskResult lastExecutionResult)
        {
            using (var context = new ApplicationDbContext())
            {
                var result = context.TaskResults.AsNoTracking().SingleOrDefault(x => x.Id == lastExecutionResult.Id);
                if (result == null)
                {
                    context.TaskResults.Add(lastExecutionResult);
                }
                else
                {
                    context.Entry(lastExecutionResult).State = EntityState.Modified;
                }
                
                context.SaveChanges();
            }
        }

        public TaskResult GetLatestTaskByKeyAndStatus(string key, TaskCompletionStatus status)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.TaskResults
                    .Where(x => x.Key == key && x.Status == status)
                    .OrderByDescending(x => x.EndTimeUtc)
                    .FirstOrDefault();
            }
        }
    }
}
