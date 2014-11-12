﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OnlineVideos.MPUrlSourceFilter
{
    /// <summary>
    /// Represent base class for HTTP urls for MediaPortal Url Source Splitter.
	/// All parameter values will be UrlEncoded, so make sure you set them UrlDecoded!
    /// </summary>
    [Serializable]
    public class HttpUrl : SimpleUrl
    {
        #region Private fields

        private String referer;
        private String userAgent;
        Version version;
        private CookieCollection cookies;
        private bool ignoreContentLength;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </summary>
        /// <param name="url">The URL to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </overloads>
        public HttpUrl(String url)
            : this(new Uri(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier.</param>
        /// <exception cref="ArgumentException">
        /// <para>The protocol supplied by <paramref name="uri"/> is not supported.</para>
        /// </exception>
        public HttpUrl(Uri uri)
            : base(uri)
        {
            if (this.Uri.Scheme != "http")
            {
                throw new ArgumentException("The protocol is not supported.", "uri");
            }

            this.cookies = new CookieCollection();

            this.Referer = String.Empty;
            this.UserAgent = String.Empty;
            this.Version = null;
            this.IgnoreContentLength = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets referer HTTP header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Referer"/> is <see langword="null"/>.</para>
        /// </exception>
        public String Referer
        {
            get { return this.referer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Referer");
                }

                this.referer = value;
            }
        }

        /// <summary>
        /// Gets or sets user agent HTTP header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="UserAgent"/> is <see langword="null"/>.</para>
        /// </exception>
        public String UserAgent
        {
            get { return this.userAgent; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("UserAgent");
                }

                this.userAgent = value;
            }
        }

        /// <summary>
        /// Gets or sets HTTP version.
        /// </summary>
        /// <remarks>
        /// If <see cref="Version"/> is <see langword="null"/>, than version supported by remote server is used.
        /// </remarks>
        public Version Version
        {
            get { return this.version; }
            set
            {
                this.version = value;
            }
        }

        /// <summary>
        /// Gets or sets ignore content length flag.
        /// </summary>
        /// <remarks>
        /// This is useful to set for Apache 1.x (and similar servers) which will report incorrect content length for files over 2 gigabytes.
        /// </remarks>
        public Boolean IgnoreContentLength
        {
            get { return this.ignoreContentLength; }
            set
            {
                this.ignoreContentLength = value;
            }
        }

        /// <summary>
        /// Gets collection of cookies.
        /// </summary>
        public CookieCollection Cookies
        {
            get { return this.cookies; }
        }

        #endregion

        #region Methods
        #endregion

        #region Constants

        // common parameters of HTTP protocol for MediaPortal Url Source Splitter

        /// <summary>
        /// Specifies referer HTTP header sent to remote server.
        /// </summary>
        protected static String ParameterReferer = "HttpReferer";

        /// <summary>
        /// Specifies user agent HTTP header sent to remote server.
        /// </summary>
        protected static String ParameterUserAgent = "HttpUserAgent";

        /// <summary>
        /// Specifies cookies sent to remote server.
        /// </summary>
        protected static String ParameterCookie = "HttpCookie";

        /// <summary>
        /// Specifies version of HTTP protocol to use.
        /// </summary>
        protected static String ParameterVersion = "HttpVersion";

        /// <summary>
        /// Specifies if content length should be ignored.
        /// </summary>
        protected static String ParameterIgnoreContentLength = "HttpIgnoreContentLength";

        /// <summary>
        /// Specifies that version of HTTP protocol is not specified.
        /// </summary>
        protected const int HttpVersionNone = 0;

        /// <summary>
        /// Forces to use HTTP version 1.0.
        /// </summary>
        protected const int HttpVersionForce10 = 1;

        /// <summary>
        /// Forces to use HTTP version 1.1.
        /// </summary>
        protected const int HttpVersionForce11 = 2;

        // default values for some parameters

        /// <summary>
        /// Default referer for MediaPortal Url Source Splitter.
        /// </summary>
        /// <remarks>
        /// This values is <see cref="System.String.Empty"/>.
        /// </remarks>
        public static String DefaultReferer = String.Empty;

        /// <summary>
        /// Default user agent for MediaPortal Url Source Splitter.
        /// </summary>
        /// <remarks>
        /// This values is <see cref="System.String.Empty"/>.
        /// </remarks>
        public static String DefaultUserAgent = String.Empty;

        /// <summary>
        /// Default HTTP version for MediaPortal Url Source Splitter.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/>.
        /// </remarks>
        public static HttpVersion DefaultVersion = null;

        /// <summary>
        /// Default ignore content length flag for MediaPortal Url Source Splitter.
        /// </summary>
        /// <remarks>
        /// This values if <see langword="false"/>.
        /// </remarks>
        public static Boolean DefaultIgnoreContentLength = false;

        #endregion
    }
}
