﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Models;
using System.Linq;

namespace FoxIDs.Models
{
    public class SamlUpParty : UpParty, IValidatableObject
    {
        public SamlUpParty()
        {
            Type = PartyTypes.Saml2;
        }

        [Required]
        [JsonProperty(PropertyName = "update_state")]
        public PartyUpdateStates UpdateState { get; set; } = PartyUpdateStates.Manual;

        [Range(Constants.Models.SamlParty.MetadataUpdateRateMin, Constants.Models.SamlParty.MetadataUpdateRateMax)]
        [JsonProperty(PropertyName = "metadata_update_rate")]
        public int? MetadataUpdateRate { get; set; }

        [MaxLength(Constants.Models.SamlParty.MetadataUrlLength)]
        [JsonProperty(PropertyName = "metadata_url")]
        public string MetadataUrl { get; set; }

        // Property can not be updated through API
        [Required]
        [JsonProperty(PropertyName = "last_updated")]
        public long LastUpdated { get; set; }

        [MaxLength(Constants.Models.SamlParty.IssuerLength)]
        [JsonProperty(PropertyName = "sp_issuer")]
        public string SpIssuer { get; set; }

        [Length(Constants.Models.Claim.TransformsMin, Constants.Models.Claim.TransformsMax)]
        [JsonProperty(PropertyName = "claim_transforms")]
        public List<SamlClaimTransform> ClaimTransforms { get; set; }

        [Length(Constants.Models.SamlParty.ClaimsMin, Constants.Models.SamlParty.ClaimsMax, Constants.Models.Claim.SamlTypeLength, Constants.Models.Claim.SamlTypeWildcardRegExPattern)]
        [JsonProperty(PropertyName = "claims")]
        public List<string> Claims { get; set; }

        [Required]
        [MaxLength(Constants.Models.SamlParty.SignatureAlgorithmLength)]
        [JsonProperty(PropertyName = "signature_algorithm")]
        public string SignatureAlgorithm { get; set; }

        [Required]
        [JsonProperty(PropertyName = "certificate_validation_mode")]
        public X509CertificateValidationMode CertificateValidationMode { get; set; }

        [Required]
        [JsonProperty(PropertyName = "revocation_mode")]
        public X509RevocationMode RevocationMode { get; set; }

        [Required]
        [MaxLength(Constants.Models.SamlParty.IssuerLength)]
        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [Required]
        [JsonProperty(PropertyName = "authn_binding")]
        public SamlBinding AuthnBinding { get; set; }

        [Required]
        [MaxLength(Constants.Models.SamlParty.Up.AuthnUrlLength)]
        [JsonProperty(PropertyName = "authn_url")]
        public string AuthnUrl { get; set; }

        [JsonProperty(PropertyName = "sign_authn_request")]
        public bool SignAuthnRequest { get; set; }

        [Required]
        [Length(Constants.Models.SamlParty.Up.KeysMin, Constants.Models.SamlParty.KeysMax)]
        [JsonProperty(PropertyName = "keys")]
        public List<JsonWebKey> Keys { get; set; }

        [ValidateComplexType]
        [JsonProperty(PropertyName = "logout_binding")]
        public SamlBinding LogoutBinding { get; set; }

        [MaxLength(Constants.Models.SamlParty.Up.LogoutUrlLength)]
        [JsonProperty(PropertyName = "logout_url")]
        public string LogoutUrl { get; set; }

        [MaxLength(Constants.Models.SamlParty.Up.LogoutUrlLength)]
        [JsonProperty(PropertyName = "single_logout_response_url")]
        public string SingleLogoutResponseUrl { get; set; }

        [JsonProperty(PropertyName = "authn_context_comparison")]
        public SamlAuthnContextComparisonTypes? AuthnContextComparison { get; set; }

        [Length(Constants.Models.SamlParty.Up.AuthnContextClassReferencesMin, Constants.Models.SamlParty.Up.AuthnContextClassReferencesMax, Constants.Models.Claim.ValueLength)]
        [JsonProperty(PropertyName = "authn_context_class_refs")]
        public List<string> AuthnContextClassReferences { get; set; }

        [JsonProperty(PropertyName = "sign_metadata")]
        public bool SignMetadata { get; set; }

        [JsonProperty(PropertyName = "metadata_include_enc_certs")]
        public bool MetadataIncludeEncryptionCertificates { get; set; }

        [Length(Constants.Models.SamlParty.MetadataNameIdFormatsMin, Constants.Models.SamlParty.MetadataNameIdFormatsMax, Constants.Models.Claim.ValueLength)]
        [JsonProperty(PropertyName = "metadata_nameid_formats")]
        public List<string> MetadataNameIdFormats { get; set; }

        [Length(Constants.Models.SamlParty.MetadataAttributeConsumingServicesMin, Constants.Models.SamlParty.MetadataAttributeConsumingServicesMax)]
        [JsonProperty(PropertyName = "metadata_attribute_consuming_service")]
        public List<SamlMetadataAttributeConsumingService> MetadataAttributeConsumingServices { get; set; }

        [Length(Constants.Models.SamlParty.MetadataContactPersonsMin, Constants.Models.SamlParty.MetadataContactPersonsMax)]
        [JsonProperty(PropertyName = "metadata_contact_persons")]
        public List<SamlMetadataContactPerson> MetadataContactPersons { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (Claims?.Where(c => c == "*").Count() > 1)
            {
                results.Add(new ValidationResult($"Only one allow all wildcard (*) is allowed in the field {nameof(Claims)}.", new[] { nameof(Claims) }));
            }
            return results;
        }
    }
}
