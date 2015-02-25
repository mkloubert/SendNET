// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarcelJoachimKloubert.SendNET
{
    /// <summary>
    /// Stores application settings.
    /// </summary>
    public sealed class AppSettings
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings" /> class.
        /// </summary>
        public AppSettings()
        {
            this.FilesToSend = new List<FileInfo>();
            this.Iterations = 1000;
            this.Port = 5979;
            this.Salt = Encoding.UTF8.GetBytes("YRr260WOR6ZSQ8GC");
        }

        #endregion Constructors (1)

        #region Properties (6)

        /// <summary>
        /// Gets the list of files to send.
        /// </summary>
        public List<FileInfo> FilesToSend
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the iteration count.
        /// </summary>
        public int Iterations
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the TCP port for the host.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        public byte[] Salt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the target directory.
        /// </summary>
        public DirectoryInfo TargetDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operation type.
        /// </summary>
        public SendOperationType Type
        {
            get;
            set;
        }

        #endregion Properties (6)
    }
}