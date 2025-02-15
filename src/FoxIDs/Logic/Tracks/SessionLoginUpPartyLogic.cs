﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using FoxIDs.Models.Config;
using FoxIDs.Models.Session;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoxIDs.Logic
{
    public class SessionLoginUpPartyLogic : SessionBaseLogic
    {
        private readonly TelemetryScopedLogger logger;
        private readonly ITenantRepository tenantRepository;
        private readonly SingleCookieRepository<SessionLoginUpPartyCookie> sessionCookieRepository;

        public SessionLoginUpPartyLogic(FoxIDsSettings settings, TelemetryScopedLogger logger, ITenantRepository tenantRepository, SingleCookieRepository<SessionLoginUpPartyCookie> sessionCookieRepository, IHttpContextAccessor httpContextAccessor) : base(settings, httpContextAccessor)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.sessionCookieRepository = sessionCookieRepository;
        }

        public async Task CreateSessionAsync(LoginUpParty loginUpParty, DownPartySessionLink newDownPartyLink, long authTime, IEnumerable<Claim> claims)
        {
            if(SessionEnabled(loginUpParty))
            {
                logger.ScopeTrace(() => $"Create session, Route '{RouteBinding.Route}'.");

                var session = new SessionLoginUpPartyCookie
                {
                    Claims = claims.ToClaimAndValues(),      
                };
                AddDownPartyLink(session, newDownPartyLink);
                session.CreateTime = authTime;
                session.LastUpdated = authTime;
                await sessionCookieRepository.SaveAsync(loginUpParty, session, GetPersistentCookieExpires(loginUpParty, session.CreateTime));
                logger.ScopeTrace(() => $"Session created, User id '{session.UserId}', Session id '{session.SessionId}'.", GetSessionScopeProperties(session));
            }
        }

        public async Task<bool> UpdateSessionAsync(LoginUpParty loginUpParty, DownPartySessionLink newDownPartyLink, SessionLoginUpPartyCookie session, IEnumerable<Claim> claims = null)
        {
            logger.ScopeTrace(() => $"Update session, Route '{RouteBinding.Route}'.");

            var sessionEnabled = SessionEnabled(loginUpParty);
            var sessionValid = SessionValid(loginUpParty, session);

            if (sessionEnabled && sessionValid)
            {
                AddDownPartyLink(session, newDownPartyLink);
                session.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (claims?.Count() > 0)
                {
                    session.Claims = UpdateClaims(session, claims);
                }
                await sessionCookieRepository.SaveAsync(loginUpParty, session, GetPersistentCookieExpires(loginUpParty, session.CreateTime));
                logger.ScopeTrace(() => $"Session updated, Session id '{session.SessionId}'.", GetSessionScopeProperties(session));
                return true;
            }

            await sessionCookieRepository.DeleteAsync(loginUpParty);
            logger.ScopeTrace(() => $"Session deleted, Session id '{session.SessionId}'.");
            return false;
        }

        private IEnumerable<ClaimAndValues> UpdateClaims(SessionLoginUpPartyCookie session, IEnumerable<Claim> claims)
        {
            var sessionClaims = new List<ClaimAndValues>(session.Claims);
            var addClaims = claims.ToClaimAndValues();
            foreach(var addClaim in addClaims)
            {
                var claim = sessionClaims.Where(c => c.Claim == addClaim.Claim).FirstOrDefault();
                if (claim == null)
                {
                    sessionClaims.Add(addClaim);
                }
                else
                {
                    foreach(var addValue in addClaim.Values)
                    {
                        if (!claim.Values.Where(v => v == addValue).Any())
                        {
                            claim.Values.Add(addValue);
                        }
                    }
                }
            }

            return sessionClaims;
        }

        public async Task<(SessionLoginUpPartyCookie, User)> GetAndUpdateSessionCheckUserAsync(LoginUpParty loginUpParty, DownPartySessionLink newDownPartyLink)
        {
            logger.ScopeTrace(() => $"Get and update session and check user, Route '{RouteBinding.Route}'.");

            var session = await sessionCookieRepository.GetAsync(loginUpParty);
            if (session != null)
            {
                var sessionEnabled = SessionEnabled(loginUpParty);
                var sessionValid = SessionValid(loginUpParty, session);

                logger.ScopeTrace(() => $"User id '{session.UserId}' session exists, Enabled '{sessionEnabled}', Valid '{sessionValid}', Session id '{session.SessionId}', Route '{RouteBinding.Route}'.");
                if (sessionEnabled && sessionValid)
                {
                    var id = await User.IdFormat(new User.IdKey { TenantName = RouteBinding.TenantName, TrackName = RouteBinding.TrackName, Email = session.Email });
                    var user = await tenantRepository.GetAsync<User>(id, false);
                    if (user != null && !user.DisableAccount)
                    {
                        if (user.UserId != session.UserId)
                        {
                            throw new Exception("Session user id and the Users user id do not match, this should not be able to occur.");
                        }

                        AddDownPartyLink(session, newDownPartyLink);
                        session.LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        await sessionCookieRepository.SaveAsync(loginUpParty, session, GetPersistentCookieExpires(loginUpParty, session.CreateTime));
                        logger.ScopeTrace(() => $"Session updated, Session id '{session.SessionId}'.", GetSessionScopeProperties(session));

                        return (session, user);
                    }
                }
                else
                {
                    await sessionCookieRepository.DeleteAsync(loginUpParty);
                    logger.ScopeTrace(() => $"Session deleted, Session id '{session.SessionId}'.");
                }
            }
            else
            {
                logger.ScopeTrace(() => "Session do not exists.");
            }

            return (null, null);
        }

        public async Task<SessionLoginUpPartyCookie> GetSessionAsync(LoginUpParty loginUpParty)
        {
            logger.ScopeTrace(() => $"Get session, Route '{RouteBinding.Route}'.");

            var session = await sessionCookieRepository.GetAsync(loginUpParty);
            if (session != null)
            {
                var sessionEnabled = SessionEnabled(loginUpParty);
                var sessionValid = SessionValid(loginUpParty, session);

                logger.ScopeTrace(() => $"User id '{session.UserId}' session exists, Enabled '{sessionEnabled}', Valid '{sessionValid}', Session id '{session.SessionId}', Route '{RouteBinding.Route}'.");
                if (sessionEnabled && sessionValid)
                {
                    logger.SetScopeProperty(Constants.Logs.SessionId, session.SessionId);
                    logger.SetScopeProperty(Constants.Logs.UserId, session.UserId);
                    logger.SetScopeProperty(Constants.Logs.Email, session.Email);
                    return session;
                }

                await sessionCookieRepository.DeleteAsync(loginUpParty);
                logger.ScopeTrace(() => $"Session deleted, Session id '{session.SessionId}'.");
            }
            else
            {
                logger.ScopeTrace(() => "Session do not exists.");
            }

            return null;
        }

        public async Task<SessionLoginUpPartyCookie> DeleteSessionAsync(LoginUpParty loginUpParty)
        {
            logger.ScopeTrace(() => $"Delete session, Route '{RouteBinding.Route}'.");
            var session = await sessionCookieRepository.GetAsync(loginUpParty);
            if (session != null)
            {
                await sessionCookieRepository.DeleteAsync(loginUpParty);
                logger.ScopeTrace(() => $"Session deleted, Session id '{session.SessionId}'.");
                return session;
            }
            else
            {
                logger.ScopeTrace(() => "Session do not exists.");
                return null;
            }
        }
    }
}
