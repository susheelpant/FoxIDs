﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FoxIDs.Client.Logic;
using FoxIDs.Client.Models.ViewModels;
using FoxIDs.Client.Services;
using Microsoft.AspNetCore.Components;
using ITfoxtec.Identity.BlazorWebAssembly.OpenidConnect;
using ITfoxtec.Identity;
using FoxIDs.Client.Models.Config;
using FoxIDs.Client.Models;
using System;
using FoxIDs.Models.Api;
using Blazored.Toast.Services;

namespace FoxIDs.Client.Pages.Components
{
    public class UpPartyBase : ComponentBase
    {
        [Inject]
        public IToastService toastService { get; set; }

        [Inject]
        public RouteBindingLogic RouteBindingLogic { get; set; }

        [Inject]
        public ClientSettings ClientSettings { get; set; }

        [Inject]
        public TrackSelectedLogic TrackSelectedLogic { get; set; }

        [Inject]
        public OpenidConnectPkce OpenidConnectPkce { get; set; }

        [Inject]
        public UpPartyService UpPartyService { get; set; }

        [Parameter]
        public EventCallback<GeneralUpPartyViewModel> OnStateHasChanged { get; set; }

        [Parameter]
        public List<GeneralUpPartyViewModel> UpParties { get; set; }

        [Parameter]
        public GeneralUpPartyViewModel UpParty { get; set; }

        [Parameter]
        public string TenantName { get; set; }

        public void ShowLoginTab(GeneralLoginUpPartyViewModel downParty, LoginTabTypes samlTabTypes)
        {
            switch (samlTabTypes)
            {
                case LoginTabTypes.Login:
                    downParty.ShowLoginTab = true;
                    downParty.ShowClaimTransformTab = false;
                    break;
                case LoginTabTypes.ClaimsTransform:
                    downParty.ShowLoginTab = false;
                    downParty.ShowClaimTransformTab = true;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void ShowOAuthTab(IGeneralOAuthUpPartyTabViewModel upParty, OAuthTabTypes oauthTabTypes)
        {
            switch (oauthTabTypes)
            {
                case OAuthTabTypes.Client:
                    upParty.ShowClientTab = true;
                    upParty.ShowClaimTransformTab = false;
                    break;
                case OAuthTabTypes.ClaimsTransform:
                    upParty.ShowClientTab = false;
                    upParty.ShowClaimTransformTab = true;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void ShowSamlTab(GeneralSamlUpPartyViewModel downParty, SamlTabTypes samlTabTypes)
        {
            switch (samlTabTypes)
            {
                case SamlTabTypes.Saml:
                    downParty.ShowSamlTab = true;
                    downParty.ShowClaimTransformTab = false;
                    break;
                case SamlTabTypes.ClaimsTransform:
                    downParty.ShowSamlTab = false;
                    downParty.ShowClaimTransformTab = true;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public (string, string, string) GetRedirectAndLogoutUrls(string partyName, PartyBindingPatterns partyBindingPattern)
        {
            var partyBinding = (partyName.IsNullOrEmpty() ? "--up-party-name--" : partyName.ToLower()).ToUpPartyBinding(partyBindingPattern);
            var oauthUrl = $"{RouteBindingLogic.GetFoxIDsTenantEndpoint()}/{(RouteBindingLogic.IsMasterTenant ? "master" : TrackSelectedLogic.Track.Name)}/{partyBinding}/{Constants.Routes.OAuthController}/";
            return (oauthUrl + Constants.Endpoints.AuthorizationResponse, oauthUrl + Constants.Endpoints.EndSessionResponse, oauthUrl + Constants.Endpoints.FrontChannelLogout);
        }

        public string GetSamlMetadata(string partyName, PartyBindingPatterns partyBindingPattern)
        {
            var partyBinding = (partyName.IsNullOrEmpty() ? "--up-party-name--" : partyName.ToLower()).ToUpPartyBinding(partyBindingPattern);
            return $"{RouteBindingLogic.GetFoxIDsTenantEndpoint()}/{(RouteBindingLogic.IsMasterTenant ? "master" : TrackSelectedLogic.Track.Name)}/{partyBinding}/{Constants.Routes.SamlController}/{Constants.Endpoints.SamlSPMetadata}";
        }

        public async Task UpPartyCancelAsync(GeneralUpPartyViewModel upParty)
        {
            if (upParty.CreateMode)
            {
                UpParties.Remove(upParty);
            }
            else
            {
                UpParty.Edit = false;
            }
            await OnStateHasChanged.InvokeAsync(UpParty);
        }
    }
}
