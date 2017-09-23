//
//  Copyright 2012-2016, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

#if ! PORTABLE && ! NETFX_CORE && ! (WINDOWS_PHONE && SILVERLIGHT) && ! NETSTANDARD1_6
using System.Runtime.Serialization.Formatters.Binary;
#endif

#if ! AZURE_MOBILE_SERVICES
namespace Xamarin.Auth
#else
namespace Xamarin.Auth._MobileServices
#endif
{
    /// <summary>
    /// An Account that represents an authenticated user of a social network.
    /// </summary>
    #if XAMARIN_AUTH_INTERNAL
    internal class Account
    #else
    public class Account
    #endif
    {
        /// <summary>
        /// The username used as a key when storing this account.
        /// </summary>
        public virtual string Username { get; set; }

        /// <summary>
        /// A key-value store associated with this account. These get encrypted when the account is stored.
        /// </summary>
        public virtual Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Cookies that are stored with the account for web services that control access using cookies.
        /// </summary>
        public virtual CookieContainer Cookies { get; private set; }

        /// <summary>
        /// Initializes a new blank <see cref="Xamarin.Auth.Account"/>.
        /// </summary>
        public Account()
            : this("", null, null)
        {
        }

        /// <summary>
        /// Initializes an <see cref="Xamarin.Auth.Account"/> with the given username.
        /// </summary>
        /// <param name='username'>
        /// The username for the account.
        /// </param>
        public Account(string username)
            : this(username, null, null)
        {
        }

        /// <summary>
        /// Initializes an <see cref="Xamarin.Auth.Account"/> with the given username and cookies.
        /// </summary>
        /// <param name='username'>
        /// The username for the account.
        /// </param>
        /// <param name='cookies'>
        /// The cookies to be stored with the account.
        /// </param>
        public Account(string username, CookieContainer cookies)
            : this(username, null, cookies)
        {
        }

        /// <summary>
        /// Initializes an <see cref="Xamarin.Auth.Account"/> with the given username and cookies.
        /// </summary>
        /// <param name='username'>
        /// The username for the account.
        /// </param>
        /// <param name='properties'>
        /// Properties for the account.
        /// </param>
        public Account(string username, IDictionary<string, string> properties)
            : this(username, properties, null)
        {
        }

        /// <summary>
        /// Initializes an <see cref="Xamarin.Auth.Account"/> with the given username and cookies.
        /// </summary>
        /// <param name='username'>
        /// The username for the account.
        /// </param>
        /// <param name='properties'>
        /// Properties for the account.
        /// </param>
        /// <param name='cookies'>
        /// The cookies to be stored with the account.
        /// </param>
        public Account(string username, IDictionary<string, string> properties, CookieContainer cookies)
        {
            Username = username;
            Properties = (properties == null) ?
                new Dictionary<string, string>() :
                new Dictionary<string, string>(properties);
            Cookies = (cookies == null) ?
                new CookieContainer() :
                cookies;
        }

        /// <summary>
        /// Serialize this account into a string that can be deserialized.
        /// </summary>
        /// <returns>A <c>string</c> representing the <see cref="Account"/> instance.</returns>
        /// <seealso cref="Deserialize"/>
        public string Serialize()
        {
            var sb = new StringBuilder();

            sb.Append("__username__=");
            sb.Append(Uri.EscapeDataString(Username));

            foreach (var p in Properties)
            {
                sb.Append("&");
                sb.Append(Uri.EscapeDataString(p.Key));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(p.Value));
            }

            if (Cookies.Count > 0)
            {
                sb.Append("&__cookies__=");
                sb.Append(Uri.EscapeDataString(SerializeCookies()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Restores an account from its serialized string representation.
        /// </summary>
        /// <param name='serializedString'>The serialized account generated by <see cref="Serialize"/>.</param>
        /// <returns>An <see cref="Account"/> instance represented by <paramref name="serializedString"/>.</returns>
        /// <seealso cref="Serialize"/>
        public static Account Deserialize(string serializedString)
        {
            var acct = new Account();

            foreach (var p in serializedString.Split('&'))
            {
                var kv = p.Split('=');

                var key = Uri.UnescapeDataString(kv[0]);
                var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";

                if (key == "__cookies__")
                {
                    acct.Cookies = DeserializeCookies(val);
                }
                else if (key == "__username__")
                {
                    acct.Username = val;
                }
                else
                {
                    acct.Properties[key] = val;
                }
            }

            return acct;
        }

        string SerializeCookies()
        {
            #if !PORTABLE && !NETFX_CORE && !(WINDOWS_PHONE && SILVERLIGHT) && !NETSTANDARD1_6
            var f = new BinaryFormatter();
            using (var s = new MemoryStream())
            {
                f.Serialize(s, Cookies);
                return Convert.ToBase64String(s.GetBuffer(), 0, (int)s.Length);
            }
            #else
			return String.Empty;
            #endif
        }

        static CookieContainer DeserializeCookies(string cookiesString)
        {
            #if !PORTABLE && !NETFX_CORE && !(WINDOWS_PHONE && SILVERLIGHT) && !NETSTANDARD1_6
            var f = new BinaryFormatter();
            using (var s = new MemoryStream(Convert.FromBase64String(cookiesString)))
            {
                return (CookieContainer)f.Deserialize(s);
            }
            #else
            return new CookieContainer();
            #endif
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Xamarin.Auth.Account"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="Xamarin.Auth.Account"/>.
        /// </returns>
        public override string ToString()
        {
            return Serialize();
        }
    }
}

