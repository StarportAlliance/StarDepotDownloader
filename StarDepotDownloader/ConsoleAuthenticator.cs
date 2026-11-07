// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Threading.Tasks;
using SteamKit2.Authentication;

namespace StarDepotDownloader
{
    // This is practically copied from https://github.com/SteamRE/SteamKit/blob/master/SteamKit2/SteamKit2/Steam/Authentication/UserConsoleAuthenticator.cs
    internal class ConsoleAuthenticator : IAuthenticator
    {
        /// <inheritdoc />
        public Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
        {
            EnsureInteractive();
            if (previousCodeWasIncorrect)
            {
                Console.Error.WriteLine("The previous 2-factor auth code you have provided is incorrect.");
            }

            string code;

            do
            {
                Console.Error.Write("STEAM GUARD! Please enter your 2-factor auth code from your authenticator app: ");
                code = Console.ReadLine()?.Trim();

                if (code == null)
                {
                    break;
                }
            }
            while (string.IsNullOrEmpty(code));

            return Task.FromResult(code!);
        }

        /// <inheritdoc />
        public Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
        {
            EnsureInteractive();
            if (previousCodeWasIncorrect)
            {
                Console.Error.WriteLine("The previous 2-factor auth code you have provided is incorrect.");
            }

            string code;

            do
            {
                Console.Error.Write($"STEAM GUARD! Please enter the auth code sent to the email at {email}: ");
                code = Console.ReadLine()?.Trim();

                if (code == null)
                {
                    break;
                }
            }
            while (string.IsNullOrEmpty(code));

            return Task.FromResult(code!);
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            EnsureInteractive();
            if (ContentDownloader.Config.SkipAppConfirmation)
            {
                return Task.FromResult(false);
            }

            Console.Error.WriteLine("STEAM GUARD! Use the Steam Mobile App to confirm your sign in...");

            return Task.FromResult(true);
        }

        static void EnsureInteractive()
        {
            if (ContentDownloader.Config.Interactive)
            {
                return;
            }

            ContentDownloader.SetAuthFailure(SddErrorCodes.AuthRequired);
            throw new SddException(
                SddErrorCodes.AuthRequired,
                "Steam requires interactive verification, but interactive login is disabled.",
                2
            );
        }
    }
}
