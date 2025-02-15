﻿using ITfoxtec.Identity;
using ITfoxtec.Identity.Saml2.Claims;
using ITfoxtec.Identity.Saml2.Schemas;
using FoxIDs.Infrastructure;
using FoxIDs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FoxIDs.Logic
{
    public class ClaimsDownLogic<TClient, TScope, TClaim> : LogicBase where TClient : OAuthDownClient<TScope, TClaim> where TScope : OAuthDownScope<TClaim> where TClaim : OAuthDownClaim
    {
        private readonly TelemetryScopedLogger logger;

        public ClaimsDownLogic(TelemetryScopedLogger logger, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.logger = logger;
        }

        public Task<List<Claim>> FilterJwtClaimsAsync(TClient client, IEnumerable<Claim> jwtClaims, IEnumerable<string> selectedScopes, bool includeIdTokenClaims = false, bool includeAccessTokenClaims = false)
        {
            if (!includeIdTokenClaims && !includeAccessTokenClaims)
            {
                throw new ArgumentException($"{nameof(includeIdTokenClaims)} and {nameof(includeAccessTokenClaims)} can not be false at the same time.");
            }

            if (jwtClaims == null)
            {
                return Task.FromResult(new List<Claim>(jwtClaims));
            }

            var filterClaimTypes = GetFilterJwtClaimTypes(client, selectedScopes, includeIdTokenClaims, includeAccessTokenClaims);
            var acceptAllClaims = filterClaimTypes.Where(c => c == "*").Count() > 0;
            if (acceptAllClaims)
            {
                return Task.FromResult(TruncateJwtClaimValues(jwtClaims));
            }
            else
            {
                var filterClaims = jwtClaims.Where(c => filterClaimTypes.Contains(c.Type));
                return Task.FromResult(TruncateJwtClaimValues(filterClaims));
            }
        }

        private List<string> GetFilterJwtClaimTypes(TClient client, IEnumerable<string> selectedScopes, bool includeIdTokenClaims, bool includeAccessTokenClaims)
        {
            if(includeIdTokenClaims && !(client is OidcDownClient))
            {
                throw new InvalidOperationException("Include ID Token claims only possible for OIDC Down Client.");
            }

            var filterClaimTypes = new List<string>();

            if (includeIdTokenClaims)
            {
                var acceptAllIdTokenClaims = client.Claims?.Cast<OidcDownClaim>()?.Where(c => c.Claim == "*" && c.InIdToken)?.Count() > 0;
                if (acceptAllIdTokenClaims)
                {
                    filterClaimTypes.Add("*");
                    return filterClaimTypes;
                }

                filterClaimTypes = filterClaimTypes.ConcatOnce(client.Claims?.Cast<OidcDownClaim>().Where(c => c.InIdToken).Select(c => c.Claim));
                filterClaimTypes = filterClaimTypes.ConcatOnce(Constants.DefaultClaims.IdToken);
            }
            if (includeAccessTokenClaims)
            {

                var acceptAllAccessTokenClaims = client.Claims?.Where(c => c.Claim == "*")?.Count() > 0;
                if (acceptAllAccessTokenClaims)
                {
                    filterClaimTypes.Add("*");
                    return filterClaimTypes;
                }

                filterClaimTypes = filterClaimTypes.ConcatOnce(client.Claims?.Select(c => c.Claim));
                filterClaimTypes = filterClaimTypes.ConcatOnce(Constants.DefaultClaims.AccessToken);
            }

            if (selectedScopes?.Count() > 0)
            {
                if (includeIdTokenClaims)
                {
                    var idTokenVoluntaryClaims = (client as OidcDownClient).Scopes?.Where(s => s.VoluntaryClaims != null && selectedScopes.Any(ss => ss == s.Scope)).SelectMany(s => s.VoluntaryClaims).Where(c => c.InIdToken).Select(c => c.Claim).ToList();
                    filterClaimTypes = filterClaimTypes.ConcatOnce(idTokenVoluntaryClaims);
                }
                if(includeAccessTokenClaims)
                {
                    if(client is OidcDownClient)
                    {
                        var accessTokenVoluntaryClaims = (client as OidcDownClient).Scopes?.Where(s => s.VoluntaryClaims != null && selectedScopes.Any(ss => ss == s.Scope)).SelectMany(s => s.VoluntaryClaims).Select(c => c.Claim).ToList();
                        filterClaimTypes = filterClaimTypes.ConcatOnce(accessTokenVoluntaryClaims);
                    }
                    else
                    {
                        var accessTokenVoluntaryClaims = client.Scopes?.Where(s => s.VoluntaryClaims != null && selectedScopes.Any(ss => ss == s.Scope)).SelectMany(s => s.VoluntaryClaims).Select(c => c.Claim).ToList();
                        filterClaimTypes = filterClaimTypes.ConcatOnce(accessTokenVoluntaryClaims);
                    }
                }
            }

            return filterClaimTypes;
        }

        public List<Claim> GetClientJwtClaims(TClient client, bool onlyIdTokenClaims = false)
        {
            var claims = client.Claims?.Where(c => c.Values?.Count() > 0);

            if (onlyIdTokenClaims)
            {
                claims = claims?.Cast<OidcDownClaim>().Where(c => c.InIdToken).Cast<TClaim>();
            }

            return claims?.SelectMany(item => item.Values.Select(value => new Claim(item.Claim, value))).ToList();
        }

        public Task<List<Claim>> FromJwtToSamlClaimsAsync(IEnumerable<Claim> jwtClaims)
        {
            try
            {
                var mappings = GetMappings(RouteBinding);

                var samlClaims = new List<Claim>();

                FromJwtAuthTimeToSaml(samlClaims, jwtClaims);
                FromJwtAmrToSaml(samlClaims, jwtClaims);

                foreach (var jwtClaim in jwtClaims.Where(c => c.Type != JwtClaimTypes.AuthTime))
                {
                    var claimMap = mappings.FirstOrDefault(m => m.JwtClaim.Equals(jwtClaim.Type, StringComparison.InvariantCultureIgnoreCase));
                    if (claimMap != null)
                    {
                        samlClaims.Add(new Claim(claimMap.SamlClaim, jwtClaim.Value, jwtClaim.ValueType, jwtClaim.Issuer, jwtClaim.OriginalIssuer));
                    }
                    else
                    {
                        samlClaims.Add(jwtClaim);
                    }
                }
                if (!samlClaims.Where(c => c.Type == Saml2ClaimTypes.NameIdFormat).Any())
                {
                    samlClaims.AddClaim(Saml2ClaimTypes.NameIdFormat, NameIdentifierFormats.Persistent.OriginalString);
                }
                return Task.FromResult(samlClaims);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to map JWT claims to SAML claims.");
                throw;
            }
        }

        private void FromJwtAuthTimeToSaml(List<Claim> samlClaims, IEnumerable<Claim> jwtClaims)
        {
            var jwtClaim = jwtClaims.Where(c => c.Type == JwtClaimTypes.AuthTime).FirstOrDefault();

            var authTime = jwtClaim == null ? DateTimeOffset.UtcNow : DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(jwtClaim.Value));
            var authTimeValue = authTime.UtcDateTime.ToString("o");
            if(jwtClaim == null)
            {
                samlClaims.Add(new Claim(ClaimTypes.AuthenticationInstant, authTimeValue));
            }
            else
            {
                samlClaims.Add(new Claim(ClaimTypes.AuthenticationInstant, authTimeValue, jwtClaim.ValueType, jwtClaim.Issuer, jwtClaim.OriginalIssuer));
            }
        }

        private void FromJwtAmrToSaml(List<Claim> samlClaims, IEnumerable<Claim> jwtClaims)
        {
            var jwtClaimValues = jwtClaims.Where(c => c.Type == JwtClaimTypes.Amr).Select(c => c.Value).ToList();

            if (jwtClaimValues?.Contains(IdentityConstants.AuthenticationMethodReferenceValues.Mfa) == true)
            {
                samlClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, Constants.Saml.AuthnContextClassTypes.Mfa));
            }
            else if (jwtClaimValues?.Contains(IdentityConstants.AuthenticationMethodReferenceValues.Pwd) == true)
            {
                samlClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, AuthnContextClassTypes.PasswordProtectedTransport.OriginalString));
            }
            else
            {
                samlClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, AuthnContextClassTypes.Unspecified.OriginalString));
            }
        }

        public Task<List<Claim>> FromSamlToJwtClaimsAsync(IEnumerable<Claim> samlClaims)
        {
            try
            {
                var mappings = GetMappings(RouteBinding);

                var jwtClaims = new List<Claim>();

                FromSamlAuthTimeToJwt(jwtClaims, samlClaims);
                FromSamlAmrToJwt(jwtClaims, samlClaims);

                foreach (var samlClaim in samlClaims)
                {
                    var claimMaps = mappings.Where(m => m.SamlClaim.Equals(samlClaim.Type, StringComparison.InvariantCultureIgnoreCase));
                    if (claimMaps?.Count() > 0)
                    {
                        foreach(var claimMap in claimMaps)
                        {
                            jwtClaims.Add(new Claim(claimMap.JwtClaim, samlClaim.Value, samlClaim.ValueType, samlClaim.Issuer, samlClaim.OriginalIssuer));
                        }
                    }
                    else if(!MappedClaimType(samlClaim.Type))
                    {
                        var jwtClaim = new Claim(
                            samlClaim.Type?.Length > Constants.Models.Claim.JwtTypeLength ? samlClaim.Type.Substring(0, Constants.Models.Claim.JwtTypeLength) : samlClaim.Type,
                            samlClaim.Value, samlClaim.ValueType, samlClaim.Issuer, samlClaim.OriginalIssuer);

                        jwtClaims.Add(jwtClaim);
                    }
                }
                return Task.FromResult(TruncateJwtClaimValues(jwtClaims));

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to map SAML claims to JWT claims.");
                throw;
            }
        }

        private bool MappedClaimType(string type)
        {
            return type switch
            {
                ClaimTypes.AuthenticationInstant => true,
                ClaimTypes.AuthenticationMethod => true,
                _ => false
            };

        }

        private List<Claim> TruncateJwtClaimValues(IEnumerable<Claim> jwtClaims)
        {
            var truncateClaims = new List<Claim>();
            foreach (var claim in jwtClaims)
            {
                if (Constants.EmbeddedJwtToken.JwtTokenClaims.Contains(claim.Type))
                {
                    if (claim.Value?.Length > Constants.EmbeddedJwtToken.ValueLength)
                    {
                        truncateClaims.AddClaim(claim.Type, claim.Value.Substring(0, Constants.EmbeddedJwtToken.ValueLength), claim.ValueType, claim.Issuer);
                    }
                    else
                    {
                        truncateClaims.Add(claim);
                    }
                }
                else
                {
                    if (claim.Value?.Length > Constants.Models.Claim.ValueLength)
                    {
                        truncateClaims.AddClaim(claim.Type, claim.Value.Substring(0, Constants.Models.Claim.ValueLength), claim.ValueType, claim.Issuer);
                    }
                    else
                    {
                        truncateClaims.Add(claim);
                    }
                }
            }
            return truncateClaims;
        }

        private void FromSamlAuthTimeToJwt(List<Claim> jwtClaims, IEnumerable<Claim> samlClaims)
        {
            var samlClaim = samlClaims.Where(c => c.Type == ClaimTypes.AuthenticationInstant).FirstOrDefault();
            if (samlClaim != null)
            {
                var value = new DateTimeOffset(DateTime.Parse(samlClaim.Value)).ToUnixTimeSeconds();
                jwtClaims.Add(new Claim(JwtClaimTypes.AuthTime, value.ToString(), samlClaim.ValueType, samlClaim.Issuer, samlClaim.OriginalIssuer));
            }
        }

        private void FromSamlAmrToJwt(List<Claim> jwtClaims, IEnumerable<Claim> samlClaims)
        {
            if(samlClaims.Where(c => c.Type == Constants.SamlClaimTypes.Amr).Any())
            {
                return;
            }

            var samlClaimValues = samlClaims.Where(c => c.Type == ClaimTypes.AuthenticationMethod).Select(c => c.Value).ToList();

            if(samlClaimValues?.Count > 0)
            {
                if (samlClaimValues.Any(c => AuthnContextClassTypes.PasswordProtectedTransport.OriginalString.Equals(c, StringComparison.InvariantCultureIgnoreCase) || AuthnContextClassTypes.UserNameAndPassword.OriginalString.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
                {
                    jwtClaims.Add(new Claim(JwtClaimTypes.Amr, IdentityConstants.AuthenticationMethodReferenceValues.Pwd));
                }
                else if (samlClaimValues.Any(c => Constants.Saml.AuthnContextClassTypes.Mfa.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
                {
                    jwtClaims.Add(new Claim(JwtClaimTypes.Amr, IdentityConstants.AuthenticationMethodReferenceValues.Mfa));
                }
            }
        }

        private IEnumerable<ClaimMap> GetMappings(RouteBinding RouteBinding)
        {
            var mappings = Constants.DefaultClaimMappings.LockedMappings.Select(cm => new ClaimMap { JwtClaim = cm.JwtClaim, SamlClaim = cm.SamlClaim });

            if (RouteBinding.ClaimMappings != null && RouteBinding.ClaimMappings?.Count() > 0)
            {
                mappings = mappings.ConcatOnce(RouteBinding.ClaimMappings, (f, s) => s.JwtClaim == f.JwtClaim);
            }

            mappings = mappings.ConcatOnce(Constants.DefaultClaimMappings.ChangeableMappings.Select(cm => new ClaimMap { JwtClaim = cm.JwtClaim, SamlClaim = cm.SamlClaim }), (f, s) => s.JwtClaim == f.JwtClaim);

            return mappings;
        }
    }
}
