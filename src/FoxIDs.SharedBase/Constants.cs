﻿using FoxI = ITfoxtec.Identity;
using System.Security.Claims;
using ITfoxtec.Identity.Saml2.Claims;

namespace FoxIDs
{
    public static class Constants
    {
        public static class DefaultAdminAccount
        {
            public const string Email = "admin@foxids.com";
            public const string Password = "FirstAccess!";
        }

        public static class Routes
        {
            public const string RouteTransformerPathKey = "path";

            public const string DefaultSiteAction = "index";
            public const string DefaultSiteController = "w";
            public const string DefaultClientController = "client";

            public const string OidcDiscoveryAction = "OpenidConfiguration";
            public const string OidcDiscoveryKeyAction = "Keys";
            public const string OidcDiscoveryController = "OpenIDConfig";

            public const string OAuthController = "oauth";
            public const string SamlController = "saml";

            public const string RouteControllerKey = "controller";
            public const string RouteActionKey = "action";
            public const string RouteBindingKey = "binding";

            public const string SequenceStringKey = Sequence.String;

            public const string PreApikey = "!";
            public const string MasterApiName = "@master";
            public const string MasterTenantName = "master";
            public const string MasterTrackName = "master";

            public const string ApiControllerPreMasterKey = "m";
            public const string ApiControllerPreTenantTrackKey = "t";
        }

        public static class Models
        {
            public const int MasterPartitionIdLength = 30;
            public const string MasterPartitionIdExPattern = @"^[\w:@]*$";
            public const int DocumentPartitionIdLength = 70;
            public const string DocumentPartitionIdExPattern = @"^[\w:\-]*$";

            public static class Master
            {
                public const int IdLength = 10;
                public const string IdRegExPattern = @"^[\w@]*$";
            }

            public static class RiskPassword
            {
                public const int IdLength = 70;
                public const string IdRegExPattern = @"^[\w@:\-]*$";
                public const int CountMin = 1;
                public const int PasswordSha1HashLength = 40;
                public const string PasswordSha1HashRegExPattern = @"^[A-F0-9]*$";
            }

            public static class SecretHash
            {
                public const int IdLength = 40;
                public const int InfoLength = 3;
                public const int SecretLength = 300;
                public const int HashAlgorithmLength = 20;
                public const int HashLength = 2048;
                public const int HashSaltLength = 512;
            }

            public static class Resource
            {
                public const int EnvelopeIdLength = 70;
                public const string EnvelopeIdRegExPattern = @"^[\w@:\-]*$";
                public const int SupportedCulturesMin = 0;
                public const int SupportedCulturesMax = 50;
                public const int SupportedCulturesLength = 5;
                public const int ResourcesMin = 1;
                public const int ResourcesMax = 5000;
                public const int CultureLength = 5;
                public const int NameLength = 500;
                public const int ValueLength = 500;
            }

            public static class Tenant
            {
                public const int IdLength = 50;
                public const string IdRegExPattern = @"^[\w:\-]*$";
                public const int NameLength = 30;
                public const string NameRegExPattern = @"^\w[\w\-]*$";
            }

            public static class Track
            {
                public const int IdLength = 80;
                public const string IdRegExPattern = @"^[\w:\-]*$";
                public const int NameLength = 30;
                public const string NameRegExPattern = @"^[\w\-]*$";

                public const int KeysMin = 0;
                public const int KeysMax = 2;

                public const int KeyExternalValidityInMonthsMin = 1;
                public const int KeyExternalValidityInMonthsMax = 12;
                public const int KeyExternalAutoRenewDaysBeforeExpiryMin = 4;
                public const int KeyExternalAutoRenewDaysBeforeExpiryMax = 30;
                public const int KeyExternalPrimaryAfterDaysMin = 2;
                public const int KeyExternalPrimaryAfterDaysMax = 20;
                public const int KeyExternalCacheLifetimeMin = 3600;
                public const int KeyExternalCacheLifetimeMax = 86400;

                public const int ResourcesMin = 0;
                public const int ResourcesMax = 5000;
                public const int SequenceLifetimeMin = 30;
                public const int SequenceLifetimeMax = 10800;

                public const int MaxFailingLoginsMin = 2;
                public const int MaxFailingLoginsMax = 20;
                public const int FailingLoginCountLifetimeMin = 900; // 15 minutes
                public const int FailingLoginCountLifetimeMax = 345600;  // 96 hours / 4 days
                public const int FailingLoginObservationPeriodMin = 60; // 1 minute
                public const int FailingLoginObservationPeriodMax = 14400; // 4 hours

                public const int PasswordLengthMin = 4;
                public const int PasswordLengthMax = 50;

                public const int AllowIframeOnDomainsMin = 0;
                public const int AllowIframeOnDomainsMax = 40;
                public const int AllowIframeOnDomainsLength = 200;

                public const int MasterTrackControlClientBaseUri = 400;

                public static class SendEmail
                {
                    public const int SendgridApiKeyLength = 200;
                }
            }

            public static class User
            {
                public const int IdLength = 140;
                public const string IdRegExPattern = @"^[\w:\-.+@]*$";
                public const int UserIdLength = 40;
                public const int ClaimsMin = 0;
                public const int ClaimsMax = 100;
                public const int EmailLength = 60;
                public const string EmailRegExPattern = @"^[\w:\-.+@]*$";
            }

            public static class Claim
            {
                public const int JwtTypeLength = 80;
                public const string JwtTypeRegExPattern = @"^[\w:\/\-.+]*$";
                public const int SamlTypeLength = 300;
                public const string SamlTypeRegExPattern = @"^[\w:\/\-.+]*$";

                public const int ValuesOAuthMin = 0;
                public const int ValuesUserMin = 1;
                public const int ValuesMax = 100;
                public const int ValueLength = 100;

                public const int MapIdLength = 90;
                public const int MapMin = 0;
                public const int MapMax = 500;
            }

            public static class Party
            {
                public const int NameLength = 30;
                public const string NameRegExPattern = @"^[\w\-]*$";
                public const int IdLength = 110;
                public const string IdRegExPattern = @"^[\w:\-]*$";

                public const string NameAndGuidIdRegExPattern = @"^[\w\-]*$";

                public const int ClaimTransformationMin = 0;
                public const int ClaimTransformationMax = 100;
                public const int ClaimTransformationLength = 300;
                public const int ClaimTransformationClaimsMin = 0;
                public const int ClaimTransformationClaimsMax = 10;
                public const int ClaimTransformationOrderMin = 0;
                public const int ClaimTransformationOrderMax = 1000;
            }

            public static class DownParty
            {
                public const int AllowUpPartyNamesMin = 0;
                public const int AllowUpPartyNamesMax = 2000;            
            }

            public static class OAuthDownParty
            {
                public const int AllowCorsOriginsMin = 0;
                public const int AllowCorsOriginsMax = 40;
                public const int AllowCorsOriginLength = 200;

                public const int ScopesLength = 50;
                public const string ScopeRegExPattern = @"^[\w:\-.]*$";

                public static class Client
                {
                    public const int ResourceScopesMin = 1;
                    public const int ResourceScopesMax = 50;
                    public const int ScopesMin = 0;
                    public const int ScopesMax = 100;
                    public const int ClaimsMin = 0;
                    public const int ClaimsMax = 100;
                    public const int VoluntaryClaimsMin = 0;
                    public const int VoluntaryClaimsMax = 100;                    
                    public const int ResponseTypesMin = 1;
                    public const int ResponseTypesMax = 5;
                    public const int ResponseTypeLength = 30;
                    public const int RedirectUrisMin = 1;
                    public const int RedirectUrisMax = 40;
                    public const int RedirectUriLength = 500;
                    public const int SecretsMin = 0;
                    public const int SecretsMax = 10;

                    public const int AuthorizationCodeLifetimeMin = 10; // 10 seconds 
                    public const int AuthorizationCodeLifetimeMax = 900; // 15 minutes
                    public const int AccessTokenLifetimeMin = 300; // 5 minutes
                    public const int AccessTokenLifetimeMax = 86400; // 24 hours
                    public const int RefreshTokenLifetimeMin = 900; // 15 minutes 
                    public const int RefreshTokenLifetimeMax = 15768000; // 6 month
                    public const int RefreshTokenAbsoluteLifetimeMin = 900; // 15 minutes 
                    public const int RefreshTokenAbsoluteLifetimeMax = 31536000; // 12 month
                }

                public static class Resource
                {
                    public const int ScopesMin = 1;
                    public const int ScopesMax = 100;
                }
            }

            public static class OidcDownParty
            {
                public static class Client
                {
                    public const int IdTokenLifetimeMin = 300; // 5 minutes
                    public const int IdTokenLifetimeMax = 86400; // 24 hours
                }
            }

            public static class LoginUpParty
            {
                public const int SessionLifetimeMin = 0; // 0 minutes 
                public const int SessionLifetimeMax = 43200; // 12 hours
                public const int SessionAbsoluteLifetimeMin = 0; // 0 minutes 
                public const int SessionAbsoluteLifetimeMax = 172800; // 48 hours
                public const int PersistentAbsoluteSessionLifetimeMin = 0; // 0 minutes 
                public const int PersistentAbsoluteSessionLifetimeMax = 31536000; // 12 month
                public const int CssStyleLength = 4000;
            }

            public static class SamlParty
            {
                public const int IssuerLength = 300;
                public const int MetadataLifetimeMin = 86400; // 24 hours 
                public const int MetadataLifetimeMax = 31536000; // 12 month
                public const int SignatureAlgorithmLength = 100;
                public const int KeysMax = 10;

                public const int ClaimsMin = 0;
                public const int ClaimsMax = 500;
                public const int ClaimValueLength = 500;

                public static class Up
                {
                    public const int KeysMin = 1;
                    public const int AuthnUrlLength = 500;
                    public const int LogoutUrlLength = 500;
                }

                public static class Down
                {
                    public const int KeysMin = 0;
                    public const int SubjectConfirmationLifetimeMin = 60; // 1 minutes 
                    public const int SubjectConfirmationLifetimeMax = 900; // 15 minutes
                    public const int IssuedTokenLifetimeMin = 300; // 5 minutes 
                    public const int IssuedTokenLifetimeMax = 86400; // 24 hours
                    public const int AcsUrlsMin = 1;
                    public const int AcsUrlsMax = 10;
                    public const int AcsUrlsLength = 500;
                    public const int SingleLogoutUrlLength = 500;
                    public const int LoggedOutUrlLength = 500;
                }
            }
        }

        public static class ControlClient
        {
            public const string ClientId = "foxids_control_client";
        }

        public static class ControlApi
        {
            public const string Version = "v1";
            public readonly static string[] SupportedApiHttpMethods = { "GET", "PUT", "POST", "DELETE" };

            public const string ResourceName = "foxids_control_api";

            public static class ResourceAndScope
            {
                public readonly static string Master = $"{ResourceName}:{Scope.Master}";
                public readonly static string Tenant = $"{ResourceName}:{Scope.Tenant}";
            }

            public static class Scope
            {
                public const string Master = "foxids:master";
                public const string Tenant = "foxids:tenant";
            }

            public static class Role
            {
                public const string TenantAdmin = "foxids:tenant.admin";
            }
        }

        public static class Sequence
        {
            public const string Object = "sequence_object";
            public const string String = "sequence_string";
            public const string Start = "sequence_start";
            public const string Valid = "sequence_valid";
        }

        public static class FormAction
        {
            public const string Domains = "form_action_domains";
        }

        public static class Endpoints
        {
            public const string Authorize = "authorize";
            public const string AuthorizeResponse = "authorizeresponse";
            public const string Token = "token";
            public const string UserInfo = "userinfo";
            public const string EndSession = "endsession";

            public const string SamlAuthn = "authn";
            public const string SamlLogout = "logout";
            public const string SamlAcs = "acs";
            public const string SamlSingleLogout = "singlelogout";
            public const string SamlLoggedOut = "loggedout";
        }

        public static class OAuth
        {
            public static class ResponseErrors
            {
                /// <summary>
                /// Login canceled by user.
                /// </summary>
                public const string LoginCanceled = "login_canceled";

            }
        }

        public static class Saml
        {
            public const string RelayState = "RelayState";
        }

        /// <summary>
        /// Default claims.
        /// </summary>
        public static class DefaultClaims
        {
            /// <summary>
            /// Default ID Token claims.
            /// </summary>
            public readonly static string[] IdToken = FoxI.IdentityConstants.DefaultJwtClaims.IdToken.ConcatOnce(
                new string[] { JwtClaimTypes.SubFormat } ).ToArray();

            /// <summary>
            /// Default Access Token claims.
            /// </summary>
            public readonly static string[] AccessToken = FoxI.IdentityConstants.DefaultJwtClaims.AccessToken.ConcatOnce(
                new string[] { JwtClaimTypes.SubFormat } ).ToArray();

            /// <summary>
            /// Default SAML claims.
            /// </summary>
            public readonly static string[] SamlClaims = { ClaimTypes.NameIdentifier, Saml2ClaimTypes.NameIdFormat, Saml2ClaimTypes.SessionIndex, ClaimTypes.Upn, ClaimTypes.AuthenticationMethod, ClaimTypes.AuthenticationInstant };
        }

        public static class JwtClaimTypes
        {
            public const string SubFormat = "sub_format";

        }

        /// <summary>
        /// Default mappings between JWT and SAML claim types.
        /// </summary>
        public static class DefaultClaimMappings
        {
            /// <summary>
            /// Default locked claim mappings.
            /// </summary>
            public readonly static ClaimMap[] LockedMappings = new ClaimMap[] 
            {
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.Subject, SamlClaim = ClaimTypes.NameIdentifier },
                new ClaimMap { JwtClaim = JwtClaimTypes.SubFormat, SamlClaim = Saml2ClaimTypes.NameIdFormat },
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.SessionId, SamlClaim = Saml2ClaimTypes.SessionIndex },
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.Email, SamlClaim = ClaimTypes.Email }
            };

            /// <summary>
            /// Default changeable claim mappings.
            /// </summary>
            public readonly static ClaimMap[] ChangeableMappings = new ClaimMap[]
            {
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.GivenName, SamlClaim = ClaimTypes.GivenName },
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.FamilyName, SamlClaim = ClaimTypes.Surname },
                new ClaimMap { JwtClaim = FoxI.JwtClaimTypes.Role, SamlClaim = ClaimTypes.Role },
            };

            public class ClaimMap
            {
                public string JwtClaim { get; set; }
                public string SamlClaim { get; set; }
            }
        }
    }
}