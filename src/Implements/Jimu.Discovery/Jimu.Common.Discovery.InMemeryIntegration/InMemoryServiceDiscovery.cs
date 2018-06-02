﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jimu.Server;

namespace Jimu.Common.Discovery.InMemoryIntegration
{
    public class InMemoryServiceDiscovery : IServiceDiscovery
    {
        private static readonly List<JimuServiceRoute> _routes = new List<JimuServiceRoute>();

        private readonly ISerializer _serializer;
        public InMemoryServiceDiscovery(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public Task ClearAsync()
        {
            _routes.Clear();
            return Task.CompletedTask;
        }

        public Task ClearAsync(string address)
        {
            _routes.Clear();
            return Task.CompletedTask;
        }

        public Task ClearAsyncByServiceId(string serviceId)
        {
            _routes.Remove(_routes.FirstOrDefault(x => x.ServiceDescriptor.Id == serviceId));
            return Task.CompletedTask;
        }

        public Task SetRoutesAsync(IEnumerable<JimuServiceRoute> routes)
        {
            // remote exists route
            _routes.RemoveAll(x => routes.Any(y => y.ServiceDescriptor.Id == x.ServiceDescriptor.Id));
            // add news routes
            _routes.AddRange(routes.ToList());
            return Task.CompletedTask;
        }

        public Task<List<JimuServiceRoute>> GetRoutesAsync()
        {
            return Task.FromResult(_routes);
        }

        [JimuService(Id = "Jimu.ServiceDiscovery.InMemory.GetRoutesDescAsync", CreatedBy = "grissom", CreatedDate = "2018-06-01", Comment = "get all service routes in this server")]
        public Task<List<JimuServiceRouteDesc>> GetRoutesDescAsync()
        {
            var routeDescriptors = new List<JimuServiceRouteDesc>(_routes.Count());
            foreach (var route in _routes)
            {
                routeDescriptors.Add(new JimuServiceRouteDesc
                {
                    ServiceDescriptor = route.ServiceDescriptor,
                    AddressDescriptors = route.Address?.Select(x => new JimuAddressDesc
                    {
                        Type = $"{x.GetType().FullName}, {x.GetType().Assembly.GetName()}",
                        Value = _serializer.Serialize<string>(x)
                    })
                });
            }

            return Task.FromResult(routeDescriptors);
        }

        public Task<List<JimuAddress>> GetAddressAsync()
        {
            return Task.FromResult(_routes.SelectMany(x => x.Address).Distinct().ToList());
        }
    }
}
