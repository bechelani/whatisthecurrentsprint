using System.Threading.Tasks;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public interface ISprintInfoService
    {
        Task<SprintInfo> GetCurrentSprintInfoAsync();
        Task UpdateCurrentSprintInfoAsync(SprintInfo sprintInfo);
    }
}
