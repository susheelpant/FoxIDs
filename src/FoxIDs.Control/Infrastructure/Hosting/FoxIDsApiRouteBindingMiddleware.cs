﻿using FoxIDs.Models;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace FoxIDs.Infrastructure.Hosting
{
    public class FoxIDsApiRouteBindingMiddleware : RouteBindingMiddleware
    {
        public FoxIDsApiRouteBindingMiddleware(RequestDelegate next, ITenantRepository tenantRepository) : base(next, tenantRepository)
        { }

        protected override ValueTask SeedAsync(IServiceProvider requestServices) => default;

        protected override ValueTask<bool> PreAsync(HttpContext httpContext, string[] route) => new ValueTask<bool>(true);

        protected override Track.IdKey GetTrackIdKey(string[] route)
        {
            var trackIdKey = new Track.IdKey();
            trackIdKey.TrackName = Constants.Routes.MasterTrackName;

            if (route.Length >= 2 && route[0].Equals(Constants.Routes.MasterApiName, StringComparison.InvariantCultureIgnoreCase) && route[1].StartsWith(Constants.Routes.PreApikey, StringComparison.InvariantCulture))
            {
                trackIdKey.TenantName = Constants.Routes.MasterTenantName;
            }
            else if (route.Length >= 2 && route[1].StartsWith(Constants.Routes.PreApikey, StringComparison.InvariantCulture))
            {
                trackIdKey.TenantName = route[0].ToLower();
            }
            else if (route.Length >= 3 && route[2].StartsWith(Constants.Routes.PreApikey, StringComparison.InvariantCulture))
            {
                trackIdKey.TenantName = route[0].ToLower();
                trackIdKey.TrackName = route[1].ToLower();
            }
            else
            {
                throw new NotSupportedException($"FoxIDs API route '{string.Join('/', route)}' not supported.");
            }

            return trackIdKey;
        }

        protected override string GetPartyNameAndbinding(string[] route) => null;

        protected override ValueTask<RouteBinding> PostRouteDataAsync(TelemetryScopedLogger scopedLogger, IServiceProvider requestServices, Track.IdKey trackIdKey, Track track, RouteBinding routeBinding, string partyNameAndBinding = null) => new ValueTask<RouteBinding>(routeBinding);
    }
}
