﻿using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ITfoxtec.Identity;

namespace FoxIDs.Models.Api
{
    public abstract class ClaimTransform : IValidatableObject
    {
        [Required]
        public ClaimTransformTypes Type { get; set; }

        [Range(Constants.Models.Claim.TransformOrderMin, Constants.Models.Claim.TransformOrderMax)]
        public int Order { get; set; }

        [Display(Name = "Select claims")]
        public abstract List<string> ClaimsIn { get; set; }

        [Required]
        public abstract string ClaimOut { get; set; }

        [Required]
        [Display(Name = "Action")]
        public ClaimTransformActions Action { get; set; } = ClaimTransformActions.Add;

        [Display(Name = "Transformation")]
        public abstract string Transformation { get; set; }

        [Display(Name = "Transformation extension")]
        public abstract string TransformationExtension { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (Action == ClaimTransformActions.Add || Action == ClaimTransformActions.Replace)
            {
                switch (Type)
                {
                    case ClaimTransformTypes.Constant:
                        if (Transformation.IsNullOrWhiteSpace())
                        {
                            results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
                        }
                        break;

                    case ClaimTransformTypes.MatchClaim:
                        ValidateMatchClaimAddReplace(results);
                        break;

                    case ClaimTransformTypes.Match:
                    case ClaimTransformTypes.RegexMatch:
                        ValidateMatchAddReplace(results);
                        break;

                    case ClaimTransformTypes.Map:
                        if (ClaimsIn?.Count() != 1)
                        {
                            results.Add(new ValidationResult($"Exactly one is required in the field {nameof(ClaimsIn)} for claim transformation type '{Type}'.", new[] { nameof(ClaimsIn) }));
                        }
                        break;

                    case ClaimTransformTypes.RegexMap:
                        if (ClaimsIn?.Count() != 1)
                        {
                            results.Add(new ValidationResult($"Exactly one is required in the field {nameof(ClaimsIn)} for claim transformation type '{Type}'.", new[] { nameof(ClaimsIn) }));
                        }
                        if (Transformation.IsNullOrWhiteSpace())
                        {
                            results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
                        }
                        break;

                    case ClaimTransformTypes.Concatenate:
                        if (ClaimsIn?.Count() < 1)
                        {
                            results.Add(new ValidationResult($"At least one is required in the field {nameof(ClaimsIn)} for claim transformation type '{Type}'.", new[] { nameof(ClaimsIn) }));
                        }
                        if (Transformation.IsNullOrWhiteSpace())
                        {
                            results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Claim transformation type '{Type}' not supported.");
                }
            }
            else if (Action == ClaimTransformActions.AddIfNot || Action == ClaimTransformActions.ReplaceIfNot)
            {
                switch (Type)
                {
                    case ClaimTransformTypes.MatchClaim:
                        ValidateMatchClaimAddReplace(results);
                        break;

                    case ClaimTransformTypes.Match:
                    case ClaimTransformTypes.RegexMatch:
                        ValidateMatchAddReplace(results);
                        break;

                    default:
                        throw new NotSupportedException($"Claim transformation type '{Type}' not supported.");
                }
            }
            else if (Action == ClaimTransformActions.Remove)
            {
                switch (Type)
                {
                    case ClaimTransformTypes.MatchClaim:
                        break;

                    case ClaimTransformTypes.Match:
                    case ClaimTransformTypes.RegexMatch:
                        if (Transformation.IsNullOrWhiteSpace())
                        {
                            results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
                        }
                        break;

                    default:
                        throw new NotSupportedException($"Claim transformation type '{Type}' not supported.");
                }
            }
            return results;
        }

        private void ValidateMatchClaimAddReplace(List<ValidationResult> results)
        {
            if (ClaimsIn?.Count() != 1)
            {
                results.Add(new ValidationResult($"Exactly one is required in the field {nameof(ClaimsIn)} for claim transformation type '{Type}'.", new[] { nameof(ClaimsIn) }));
            }
            if (Transformation.IsNullOrWhiteSpace())
            {
                results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
            }
        }

        private void ValidateMatchAddReplace(List<ValidationResult> results)
        {
            if (ClaimsIn?.Count() != 1)
            {
                results.Add(new ValidationResult($"Exactly one is required in the field {nameof(ClaimsIn)} for claim transformation type '{Type}'.", new[] { nameof(ClaimsIn) }));
            }
            if (Transformation.IsNullOrWhiteSpace())
            {
                results.Add(new ValidationResult($"The field {nameof(Transformation)} is required for claim transformation type '{Type}'.", new[] { nameof(Transformation) }));
            }
            if (TransformationExtension.IsNullOrWhiteSpace())
            {
                results.Add(new ValidationResult($"The field {nameof(TransformationExtension)} is required for claim transformation type '{Type}'.", new[] { nameof(TransformationExtension) }));
            }
        }
    }
}
