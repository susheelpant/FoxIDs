﻿using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Client.Models.ViewModels
{
    public class SamlClaimTransformClaimInViewModel : SamlClaimTransformViewModel
    {
        [Required]
        [MaxLength(Constants.Models.Claim.SamlTypeLength)]
        [RegularExpression(Constants.Models.Claim.SamlTypeRegExPattern)]
        [Display(Name = "Select claim")]
        public string ClaimIn { get; set; }
    }
}
