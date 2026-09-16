using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using Studentst.DTOs;

namespace Studentst.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        private static readonly List<string> ValidDomains = new List<string>
        {
            "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "icloud.com"
        };

        public RegisterValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

            // Email Validation - ENHANCED
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Matches(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$")
                .WithMessage("Email must be in format: user@domain.com")
                .Must(NotHaveRepeatedCharacters).WithMessage("Email domain contains repeated characters (e.g., gmailll.com)")
                .Must(BeAValidDomain).WithMessage((dto, email) =>
                {
                    var domain = email.Split('@')[1].ToLower();
                    var suggestion = GetClosestDomain(domain);

                    return suggestion != null
                        ? $"Invalid email domain. Did you mean '{email.Split('@')[0]}@{suggestion}'?"
                        : "Invalid email domain.";
                });

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number");
        }

        // 👇 NEW: Check for repeated characters like "gmaillll.com"
        private bool NotHaveRepeatedCharacters(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return true;

            var domain = email.Split('@')[1].ToLower();

            // Check for 3 or more repeated consecutive characters
            // Pattern: (.)\1{2,} means "any character repeated 3+ times"
            return !Regex.IsMatch(domain, @"(.)\1{2,}");
        }

        private bool BeAValidDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return false;

            var domain = email.Split('@')[1].ToLower();

            // Exact match
            if (ValidDomains.Contains(domain))
                return true;

            // Check for typos
            var closest = GetClosestDomain(domain);
            return closest == null;
        }

        private string GetClosestDomain(string domain)
        {
            foreach (var validDomain in ValidDomains)
            {
                int distance = LevenshteinDistance(domain, validDomain);

                // Allow distance up to 3 now (for cases like gmaill.com)
                if (distance <= 3 && distance > 0)
                {
                    return validDomain;
                }
            }
            return null;
        }

        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }
            return d[n, m];
        }
    }
}