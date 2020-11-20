using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public class SprintInfoService : ISprintInfoService
    {
        private const string Id = "currentSprint";
        private Container _container;

        public SprintInfoService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<SprintInfo> GetCurrentSprintInfoAsync()
        {
            try
            {
                ItemResponse<SprintInfo> response = await this._container.ReadItemAsync<SprintInfo>(Id, new PartitionKey(Id));

                return response.Resource;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateCurrentSprintInfoAsync(SprintInfo sprintInfo)
        {
            await this._container.UpsertItemAsync<SprintInfo>(sprintInfo, new PartitionKey(sprintInfo.Id.ToString()));
        }
    }
}
