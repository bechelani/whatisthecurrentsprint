using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public interface ITriggerService
    {
        Task<List<Trigger>> GetAllTriggersAsync();
        Task UpdateTriggerAsync(Trigger trigger);
    }
}
