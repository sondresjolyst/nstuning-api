using SjolystInnovation.AppKit.SiteSettings;

namespace nstuning_api.Infrastructure
{
    /// <summary>
    /// Starting text for the legal pages, written once while a page has none. An admin edits
    /// these from the admin panel afterwards. The seeder never overwrites an existing page.
    /// </summary>
    public static class LegalSeedText
    {
        /// <summary>The one language the site is written in.</summary>
        public const string Locale = "en";

        public static readonly LegalPageSeed[] All =
        [
            new("terms", Locale, "Terms of Service", Terms),
            new("privacy", Locale, "Privacy Policy", Privacy),
            new("cookies", Locale, "Cookie Policy", Cookies),
        ];

        private const string Terms = """
## 1. About us

This website is operated by:

- **Company name:** Nordmark Service AS
- **Organisation number:** 923 202 374 MVA
- **Address:** Håbakken 7, 4355 Kvernaland

## 2. Acceptance of terms

By using NS Tuning you agree to these terms. If you do not agree, please do not use the site.

## 3. Use of the service

NS Tuning provides information about our dyno and performance tuning, documented dyno run reports, and the ability to create an account and contact us. You are responsible for keeping your login credentials secure.

## 4. Intellectual property

All content on this site, including dyno run reports, graphs, text, images, and the NS Tuning name and logo, is owned by Nordmark Service AS. You may view it for personal, non-commercial use. You may not republish, resell, or redistribute it without our written permission.

## 5. Enquiries and tuning work

Submitting the contact form is a request. It is not a binding agreement. Any tuning work, including its scope and price, is agreed directly between you and Nordmark Service AS.

## 6. Results

Dyno figures and results shown on this site relate to specific vehicles under specific conditions and are not a guarantee of the results for any other vehicle. Modifying a vehicle may affect its warranty, road-legal status, and insurance. You are responsible for ensuring any work complies with applicable rules.

## 7. Availability

We aim for high availability but do not guarantee uninterrupted access. The service may be updated or taken offline for maintenance at any time.

## 8. Data and privacy

See our [Privacy Policy](/privacy) for how we collect, use, and protect your personal data, and our [Cookie Policy](/cookies) for cookies.

## 9. Governing law

Norwegian law applies to these terms. Disputes that cannot be resolved directly may be brought before the Norwegian courts.

## 10. Changes to terms

We may update these terms from time to time. The current version always applies, with the date shown above.

## 11. Contact

Questions about these terms? [Contact us](/contact).
""";

        private const string Privacy = """
## Who is responsible

Nordmark Service AS (NS Tuning) is the data controller for personal data collected through this website.

## What data we collect

- Account information: username, first and last name, email address, hashed password.
- Contact enquiries: the name, email, phone number, vehicle, and message you submit through the contact form.
- Usage data: basic server logs (opaque user IDs, request paths) for debugging and security purposes.

We do not collect payment data through this site.

## How we use your data

- To create and operate your account.
- To send account-related emails (email confirmation, password reset).
- To answer your enquiry and arrange any tuning work you ask about.
- To improve the service and diagnose technical issues.

We do not use your data for advertising and we do not sell or trade it.

## Legal basis for processing

We process your personal data under the following legal bases (GDPR Article 6):

- **Contract (Art. 6(1)(b)):** account data and account-related emails are necessary to provide the account you signed up for.
- **Legitimate interests (Art. 6(1)(f)):** contact enquiries are processed so we can respond to you, and server logs are retained for security monitoring and debugging. Our interest does not override your rights. You can object at any time.

## Data sharing

We do not sell or trade your personal data. The platform is self-hosted on infrastructure under our direct control. There is no upstream cloud or hosting provider. We use the following third-party data processor:

- [Brevo](https://www.brevo.com/legal/termsofuse/) (France, EEA): transactional email delivery (email confirmation, password reset) and delivery of contact-form enquiries. Your email address and the email body are shared with Brevo solely for this purpose.

We may also disclose data when required by law.

## Data retention

- **Account & preferences:** until you delete your account, after which your username, name, email and password are scrubbed.
- **Contact enquiries:** kept for as long as needed to handle your request and any follow-up, then deleted.
- **Server logs:** 90 days. **Metrics:** 60 days.
- **Database backups:** encrypted at rest. After account deletion, residual data may persist in backups for up to about 6 months until rotation completes. Backups are not used for any processing.

## Your rights

Under GDPR you have the right to:

- **Access & portability:** download a copy of your account data in JSON format from your [account page](/profile). For a copy of any enquiry we still hold, [contact us](/contact).
- **Erasure:** permanently delete your account and associated data from your [account page](/profile).
- **Correction:** update your name from your [account page](/profile). For email or username changes, [contact us](/contact).
- **Object:** object to processing carried out on the basis of our legitimate interests.

For any other request, [contact us](/contact). You may also lodge a complaint with the Norwegian Data Protection Authority.

## Cookies

We use cookies for authentication and session management. See our [Cookie Policy](/cookies) for details.
""";

        private const string Cookies = """
## What are cookies?

Cookies are small text files stored in your browser. They help a site remember your session between requests.

## Cookies we use

Browsing the site sets no tracking or advertising cookies. The cookies below are only set when you sign in to an account.

| Name | Purpose | Duration |
| --- | --- | --- |
| `next-auth.session-token` | Keeps you signed in | Session / 30 days |
| `next-auth.csrf-token` | Security: prevents cross-site request forgery | Session |
| `next-auth.callback-url` | Remembers where to redirect after login | Session |

## Third-party cookies

We do not use any third-party analytics, advertising, or tracking cookies.

## Managing cookies

You can clear or block cookies through your browser settings. Blocking the cookies above only affects staying signed in. The rest of the site works without them.

## Questions?

[Contact us](/contact) if you have any questions about how we use cookies.
""";
    }
}
