﻿using FoxIDs.Models.Config;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using UrlCombineLib;

namespace FoxIDs
{
    public static class HttpContextExtensions
    {
        public static string GetHost(this HttpContext context, bool addTrailingSlash = true)
        {
            var settings = context.RequestServices.GetService<Settings>();
            if (settings != null)
            {
                if (!settings.FoxIDsControlEndpoint.IsNullOrEmpty())
                {
                    return AddSlash(settings.FoxIDsControlEndpoint, addTrailingSlash);
                }
                if (!settings.FoxIDsEndpoint.IsNullOrEmpty())
                {
                    return AddSlash(settings.FoxIDsEndpoint, addTrailingSlash);
                } 
            }

            return AddSlash($"{context.Request.Scheme}://{context.Request.Host.ToUriComponent()}/", addTrailingSlash);
        }

        public static string GetHostWithTenantAndTrack(this HttpContext context)
        {
            var routeBinding = context.GetRouteBinding();
            if (!routeBinding.HasCustomDomain)
            {
                return UrlCombine.Combine(context.GetHost(), routeBinding.TenantName, routeBinding.TrackName);
            }
            else
            {
                return UrlCombine.Combine(context.GetHost(), routeBinding.TrackName);
            }
        }

        public static Uri GetHostUri(this HttpContext context)
        {
            return new Uri(context.GetHost());
        }

        private static string AddSlash(string url, bool addTrailingSlash = true)
        {
            if (url.EndsWith('/'))
            {
                return addTrailingSlash ? url : url.Substring(0, url.Length - 1);
            }
            else
            {
                return addTrailingSlash ? $"{url}/" : url;
            }
        }
    }
}
