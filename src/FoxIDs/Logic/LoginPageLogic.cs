﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ITfoxtec.Identity.Saml2.Schemas;
using FoxIDs.Models.Logic;
using FoxIDs.Models.Sequences;
using System.Linq;
using FoxIDs.Models.Session;
using ITfoxtec.Identity.Util;

namespace FoxIDs.Logic
{
    public class LoginPageLogic : LogicBase
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly SessionLoginUpPartyLogic sessionLogic;
        private readonly SequenceLogic sequenceLogic;
        private readonly ClaimTransformLogic claimTransformLogic;
        private readonly LoginUpLogic loginUpLogic;

        public LoginPageLogic(TelemetryScopedLogger logger, IServiceProvider serviceProvider, SessionLoginUpPartyLogic sessionLogic, SequenceLogic sequenceLogic, ClaimTransformLogic claimTransformLogic, LoginUpLogic loginUpLogic, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.sessionLogic = sessionLogic;
            this.sequenceLogic = sequenceLogic;
            this.claimTransformLogic = claimTransformLogic;
            this.loginUpLogic = loginUpLogic;
        }

        public void CheckUpParty(UpSequenceData sequenceData)
        {
            if (!sequenceData.UpPartyId.Equals(RouteBinding.UpParty.Id, StringComparison.Ordinal))
            {
                throw new Exception("Invalid up-party id.");
            }
            logger.SetScopeProperty(Constants.Logs.UpPartyId, sequenceData.UpPartyId);

            if (RouteBinding.UpParty.Type != PartyTypes.Login)
            {
                throw new NotSupportedException($"Party type '{RouteBinding.UpParty.Type}' not supported.");
            }
        }

        public bool GetRequereMfa(User user, LoginUpParty loginUpParty, LoginUpSequenceData sequenceData)
        {
            if (user.RequireMultiFactor)
            {
                return true;
            }
            else if (loginUpParty.RequireTwoFactor)
            {
                return true;
            }
            else if (sequenceData.Acr?.Where(v => v.Equals(Constants.Oidc.Acr.Mfa, StringComparison.Ordinal))?.Count() > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<IActionResult> LoginResponseAsync(LoginUpParty loginUpParty, DownPartySessionLink newDownPartyLink, User user, IEnumerable<string> authMethods, IEnumerable<Claim> acrClaims = null, SessionLoginUpPartyCookie session = null)
        {
            var authTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            List<Claim> claims = null;
            if (session != null && await sessionLogic.UpdateSessionAsync(loginUpParty, newDownPartyLink, session, acrClaims))
            {
                claims = session.Claims.ToClaimList();
            }
            else
            {
                var sessionId = RandomGenerator.Generate(24);
                claims = await GetClaimsAsync(loginUpParty, user, authTime, authMethods, sessionId, acrClaims);
                await sessionLogic.CreateSessionAsync(loginUpParty, newDownPartyLink, authTime, claims);
            }

            return await loginUpLogic.LoginResponseAsync(claims);
        }

        public async Task<IActionResult> LoginResponseUpdateSessionAsync(LoginUpParty loginUpParty, DownPartySessionLink newDownPartyLink, SessionLoginUpPartyCookie session)
        {
            if (session != null && await sessionLogic.UpdateSessionAsync(loginUpParty, newDownPartyLink, session))
            {
                return await loginUpLogic.LoginResponseAsync(session.Claims.ToClaimList());
            }
            else
            {
                throw new InvalidOperationException("Session do not exist or can not be updated.");
            }
        }

        private async Task<List<Claim>> GetClaimsAsync(LoginUpParty party, User user, long authTime, IEnumerable<string> authMethods, string sessionId, IEnumerable<Claim> acrClaims = null)
        {
            var claims = new List<Claim>();
            claims.AddClaim(JwtClaimTypes.Subject, user.UserId);
            claims.AddClaim(JwtClaimTypes.AuthTime, authTime.ToString());
            claims.AddRange(authMethods.Select(am => new Claim(JwtClaimTypes.Amr, am)));
            if (acrClaims?.Count() > 0)
            {
                claims.AddRange(acrClaims);
            }
            claims.AddClaim(JwtClaimTypes.SessionId, sessionId);
            claims.AddClaim(JwtClaimTypes.PreferredUsername, user.Email);
            claims.AddClaim(JwtClaimTypes.Email, user.Email);
            claims.AddClaim(JwtClaimTypes.EmailVerified, user.EmailVerified.ToString().ToLower());
            claims.AddClaim(Constants.JwtClaimTypes.UpParty, party.Name);
            claims.AddClaim(Constants.JwtClaimTypes.UpPartyType, party.Type.ToString().ToLower());
            if (user.Claims?.Count() > 0)
            {
                claims.AddRange(user.Claims.ToClaimList());
            }
            logger.ScopeTrace(() => $"Up, OIDC created JWT claims '{claims.ToFormattedString()}'", traceType: TraceTypes.Claim);

            var transformedClaims = await claimTransformLogic.Transform(party.ClaimTransforms?.ConvertAll(t => (ClaimTransform)t), claims);
            logger.ScopeTrace(() => $"Up, OIDC output JWT claims '{transformedClaims.ToFormattedString()}'", traceType: TraceTypes.Claim);
            return transformedClaims;
        }
    }
}
