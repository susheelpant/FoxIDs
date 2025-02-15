﻿using FoxIDs.Models;
using FoxIDs.Repository;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlCombineLib;
using static ITfoxtec.Identity.IdentityConstants;

namespace FoxIDs.Logic
{
    public class MasterTenantLogic : LogicBase
    {
        const string loginName = "login";

        private readonly ITenantRepository tenantRepository;
        private readonly BaseAccountLogic accountLogic;
        private readonly TrackLogic trackLogic;

        public MasterTenantLogic(ITenantRepository tenantRepository, BaseAccountLogic accountLogic, TrackLogic trackLogic, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.tenantRepository = tenantRepository;
            this.accountLogic = accountLogic;
            this.trackLogic = trackLogic;
        }

        public async Task CreateMasterTrackDocumentAsync(string tenantName)
        {
            tenantName = tenantName?.ToLower();
            var trackName = Constants.Routes.MasterTrackName;

            var mTrack = new Track
            {
                Name = trackName,
                SequenceLifetime = 1800,
                MaxFailingLogins = 5,
                FailingLoginCountLifetime = 36000,
                FailingLoginObservationPeriod = 600,
                PasswordLength = 8,
                CheckPasswordComplexity = true,
                CheckPasswordRisk = true
            };
            await mTrack.SetIdAsync(new Track.IdKey { TenantName = tenantName, TrackName = trackName });

            await trackLogic.CreateTrackDocumentAsync(mTrack, tenantName, trackName);
        }

        public async Task CreateTrackDocumentAsync(string tenantName, Track mTrack)
        {
            tenantName = tenantName?.ToLower();

            await mTrack.SetIdAsync(new Track.IdKey { TenantName = tenantName, TrackName = mTrack.Name });

            await trackLogic.CreateTrackDocumentAsync(mTrack, tenantName, mTrack.Name);
        }

        public async Task<LoginUpParty> CreateMasterLoginDocumentAsync(string tenantName)
        {
            var mLoginUpParty = new LoginUpParty
            {
                Name = loginName,
                EnableCreateUser = false,
                EnableCancelLogin = false,
                SessionLifetime = 0,
                PersistentSessionLifetimeUnlimited = false,
                LogoutConsent = LoginUpPartyLogoutConsent.IfRequired
            };
            await mLoginUpParty.SetIdAsync(new Party.IdKey { TenantName = tenantName?.ToLower(), TrackName = Constants.Routes.MasterTrackName, PartyName = loginName });

            await tenantRepository.CreateAsync(mLoginUpParty);
            return mLoginUpParty;
        }

        public async Task<LoginUpParty> CreateLoginDocumentAsync(string tenantName, string trackName)
        {
            var mLoginUpParty = new LoginUpParty
            {
                Name = loginName,
                EnableCreateUser = true,
                EnableCancelLogin = false,
                SessionLifetime = 36000, // 10 hours
                PersistentSessionLifetimeUnlimited = false,
                LogoutConsent = LoginUpPartyLogoutConsent.IfRequired
            };
            await mLoginUpParty.SetIdAsync(new Party.IdKey { TenantName = tenantName?.ToLower(), TrackName = trackName, PartyName = loginName });

            await tenantRepository.CreateAsync(mLoginUpParty);
            return mLoginUpParty;
        }

        public async Task CreateFirstAdminUserDocumentAsync(string tenantName, string email, string password, bool confirmAccount)
        {
            var claims = new List<Claim> { new Claim(JwtClaimTypes.Role, Constants.ControlApi.Role.TenantAdmin) };
            await accountLogic.CreateUser(email, password, changePassword: true, claims: claims, tenantName: tenantName?.ToLower(), trackName: Constants.Routes.MasterTrackName, checkUserAndPasswordPolicy: false, confirmAccount: confirmAccount);
        }

        public async Task CreateMasterFoxIDsControlApiResourceDocumentAsync(string tenantName, bool includeMasterTenantScope = false)
        {
            var mControlApiResourceDownParty = new OAuthDownParty
            {
                Name = Constants.ControlApi.ResourceName
            };
            await mControlApiResourceDownParty.SetIdAsync(new Party.IdKey { TenantName = tenantName?.ToLower(), TrackName = Constants.Routes.MasterTrackName, PartyName = Constants.ControlApi.ResourceName });
           
            var scopes = new List<string> { Constants.ControlApi.Scope.Tenant };
            if (includeMasterTenantScope)
            {
                scopes.Add(Constants.ControlApi.Scope.Master);
            }
            mControlApiResourceDownParty.Resource = new OAuthDownResource()
            {
                Scopes = scopes
            };

            await tenantRepository.CreateAsync(mControlApiResourceDownParty);
        }

        public async Task CreateMasterControlClientDocmentAsync(string tenantName, string controlClientBaseUri, LoginUpParty loginUpParty, bool includeMasterTenantScope = false)
        {
            var mControlClientDownParty = new OidcDownParty
            {
                Name = Constants.ControlClient.ClientId
            };
            await mControlClientDownParty.SetIdAsync(new Party.IdKey { TenantName = tenantName?.ToLower(), TrackName = Constants.Routes.MasterTrackName, PartyName = Constants.ControlClient.ClientId });
            mControlClientDownParty.AllowUpParties = new List<UpPartyLink> { new UpPartyLink { Name = loginUpParty.Name?.ToLower(), Type = loginUpParty.Type } };
            mControlClientDownParty.AllowCorsOrigins = GetControlClientAllowCorsOrigins(controlClientBaseUri);

            var scopes = new List<string> { Constants.ControlApi.Scope.Tenant };
            if (includeMasterTenantScope)
            {
                scopes.Add(Constants.ControlApi.Scope.Master);
            }
            mControlClientDownParty.Client = new OidcDownClient
            {
                RedirectUris = GetControlClientRedirectUris(tenantName?.ToLower(), controlClientBaseUri).ToList(),
                ResourceScopes = new List<OAuthDownResourceScope> { new OAuthDownResourceScope { Resource = Constants.ControlApi.ResourceName, Scopes = scopes } },
                ResponseTypes = new[] { "code" }.ToList(),
                Scopes = GetControlClientScopes(),
                Claims = GetControlClientClaims(),
                RequirePkce = true,
                AuthorizationCodeLifetime = 30,
                IdTokenLifetime = 3600, // 1 hours
                AccessTokenLifetime = 3600, // 1 hours
                RefreshTokenLifetime = 7200, // 2 hours
                RefreshTokenAbsoluteLifetime = 21600, // 6 hours
                RefreshTokenUseOneTime = true,
                RefreshTokenLifetimeUnlimited = false,
                RequireLogoutIdTokenHint = false,
            };
            
            await tenantRepository.CreateAsync(mControlClientDownParty);
        }

        private List<string> GetControlClientAllowCorsOrigins(string controlClientBaseUri)
        {
            return new List<string> { controlClientBaseUri.TrimEnd('/') };
        }

        private List<OidcDownScope> GetControlClientScopes()
        {
            return new List<OidcDownScope>
            {
                new OidcDownScope { Scope = DefaultOidcScopes.OfflineAccess },
                new OidcDownScope { Scope = DefaultOidcScopes.Profile, VoluntaryClaims = new List<OidcDownClaim>
                    {
                        new OidcDownClaim { Claim = JwtClaimTypes.Name, InIdToken = true  },
                        new OidcDownClaim { Claim = JwtClaimTypes.FamilyName, InIdToken = true  },
                        new OidcDownClaim { Claim = JwtClaimTypes.GivenName, InIdToken = true  },
                        new OidcDownClaim { Claim = JwtClaimTypes.MiddleName, InIdToken = true  },
                        new OidcDownClaim { Claim = JwtClaimTypes.Locale }
                    }
                },
                new OidcDownScope { Scope = DefaultOidcScopes.Email, VoluntaryClaims = new List<OidcDownClaim>
                    {
                        new OidcDownClaim { Claim = JwtClaimTypes.Email, InIdToken = true  },
                        new OidcDownClaim { Claim = JwtClaimTypes.EmailVerified }
                    }
                },
            };
        }

        private List<OidcDownClaim> GetControlClientClaims()
        {
            return new List<OidcDownClaim> { new OidcDownClaim { Claim = JwtClaimTypes.Role, InIdToken = true } };
        }

        private IEnumerable<string> GetControlClientRedirectUris(string tenantName, string baseUrl)
        {
            yield return UrlCombine.Combine(baseUrl, tenantName, "authentication/login_callback");
            yield return UrlCombine.Combine(baseUrl, tenantName, "authentication/logout_callback");
        }
    }
}
