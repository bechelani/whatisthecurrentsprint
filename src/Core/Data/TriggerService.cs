using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhatIsTheCurrentSprint.Core.Data
{
    public class TriggerService : ITriggerService
    {
        private Container _container;

        public TriggerService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<List<Trigger>> GetAllTriggersAsync()
        {
            List<Trigger> triggers = new List<Trigger>();

            try
            {
                using (FeedIterator<Trigger> resultSet = this._container.GetItemQueryIterator<Trigger>(
                    queryDefinition: null,
                    requestOptions: new QueryRequestOptions()
                    {

                    }))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<Trigger> response = await resultSet.ReadNextAsync();

                        if (response.Count > 0)
                        {
                            Trigger trigger = response.First();
                            Console.WriteLine($"\n Trigger: {trigger.Name}; Id: {trigger.Id}; Enabled: {trigger.Enabled}");
                        }

                        if (response.Diagnostics != null)
                        {
                            Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                        }

                        triggers.AddRange(response);
                    }
                }
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            return triggers;
        }

        public async Task UpdateTriggerAsync(Trigger trigger)
        {
            if (trigger.Id == Guid.Empty)
            {
                trigger.Id = Guid.NewGuid();
            }

            await this._container.UpsertItemAsync<Trigger>(trigger, new PartitionKey(trigger.Id.ToString()));
        }
    }
}
