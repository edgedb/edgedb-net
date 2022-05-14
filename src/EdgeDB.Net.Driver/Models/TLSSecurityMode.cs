using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public enum TLSSecurityMode
    {
        /// <summary>
        ///     Certificates and hostnames will be verified.
        /// </summary>
        /// <remarks>
        ///     This is the default behavior 
        /// </remarks>
        Strict,

        /// <summary>
        ///     Verify certificates but not hostnames.
        /// </summary>
        NoHostnameVerification,

        /// <summary>
        ///     Client libraries will trust self-signed TLS certificates. useful for self-signed or custom certificates.
        /// </summary>
        Insecure,

        /// <summary>
        ///     The default value, equivalent to <see cref="Strict"/>
        /// </summary>
        Default = Strict
    }
}
