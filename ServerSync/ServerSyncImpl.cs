// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using SINFONI;
using System;
using System.Collections.Generic;
using System.Configuration;
using TerminalPlugin;

namespace ServerSyncPlugin
{
    // TODO: Write a test for this class. As it constructs WorldSync, DomainSync and other classes, writing a unit test
    // is non-trivial and requires major refactoring. However, there is a sufficient functionality in this class, which
    // should be tested. See relevant issue: https://github.com/rryk/FiVES/issues/97.

    /// <summary>
    /// Implements the ServerSync plugin interface.
    /// </summary>
    class ServerSyncImpl : IServerSync
    {
        /// <summary>
        /// Initializes the object of this class. We have chosen to use this method instead of a constructor as various
        /// other methods called from Initilize require a ServerSync.Instance to be set, which requires a construct to
        /// have finished constructing the object already.
        /// </summary>
        public void Initialize()
        {
            localServer = new LocalServerImpl();
            remoteServers = new Dictionary<Connection, IRemoteServer>();

            domainSync = new DomainSync();
            worldSync = new WorldSync();
            componentSync = new ComponentSync();

            localServer.Service.OnNewClient += HandleNewServerConnected;

            ConnectToRemoteServers();

            PluginManager.Instance.AddPluginLoadedHandler("Terminal", RegisterTerminalCommands);
        }

        /// <summary>
        /// Collection of remote servers.
        /// </summary>
        public IEnumerable<IRemoteServer> RemoteServers
        {
            get
            {
                lock (remoteServers)
                {
                    return new List<IRemoteServer>(remoteServers.Values);
                }
            }
        }

        /// <summary>
        /// Local server.
        /// </summary>
        public ILocalServer LocalServer
        {
            get { return localServer; }
        }

        /// <summary>
        /// Triggered when a remote server is added to the RemoteServers collection.
        /// </summary>
        public event EventHandler<ServerEventArgs> AddedServer;

        /// <summary>
        /// Triggered when a remote server is removed from the RemoteServers collection.
        /// </summary>
        public event EventHandler<ServerEventArgs> RemovedServer;

        void RegisterTerminalCommands()
        {
            Terminal.Instance.RegisterCommand("print-server-info", "Prints info on all remote servers", false,
                PrintServerInfo, new List<string> { "psi" });
        }

        void PrintServerInfo(string commandLine)
        {
            foreach (IRemoteServer server in ServerSync.RemoteServers)
            {
                string message = String.Format("{0}: doi = [{1}], dor = [{2}]", server.SyncID, server.DoI, server.DoR);
                Terminal.Instance.WriteLine(message);
            }
        }

        void ConnectToRemoteServers()
        {
            List<string> remoteServerUris = GetListOfRemoteServers();
            foreach (string uri in remoteServerUris)
            {
                ServiceWrapper remoteService = ServiceFactory.Discover(uri);

                localServer.RegisterSyncIDAPI(remoteService);
                worldSync.RegisterWorldSyncAPI(remoteService);
                domainSync.RegisterDomainSyncAPI(remoteService);
                componentSync.RegisterComponentSyncAPI(remoteService);

                remoteService.OnConnected += ServerSyncTools.ConfigureJsonSerializer;
                remoteService.OnConnected += HandleNewServerConnected;
            }
        }

        List<string> GetListOfRemoteServers()
        {
            List<string> remoteServers = new List<string>();
            Configuration serverSyncConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            RemoteServerCollection remoteServersCollection = ((RemoteServersSection)serverSyncConfig.GetSection("RemoteServers")).Servers;
            foreach(RemoteServerElement server in remoteServersCollection)
            {
                remoteServers.Add(server.Url);
            }
            return remoteServers;
        }

        void HandleNewServerConnected(Connection connection)
        {
            IDomainOfInterest doi = null;
            IDomainOfResponsibility dor = null;
            Guid syncID = Guid.Empty;

            connection["serverSync.getDoR"]().OnSuccess<string>(delegate(string serializedDoR) {
                dor = StringSerialization.DeserializeObject<IDomainOfResponsibility>(serializedDoR);
                if (doi != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            });

            connection["serverSync.getDoI"]().OnSuccess<string>(delegate(string serializedDoI)
            {
                doi = StringSerialization.DeserializeObject<IDomainOfInterest>(serializedDoI);
                if (dor != null && syncID != Guid.Empty)
                    AddRemoteServer(connection, dor, doi, syncID);
            });

            connection["serverSync.getSyncID"]().OnSuccess<Guid>(delegate(Guid remoteSyncID)
            {
                syncID = remoteSyncID;
                if (dor != null && doi != null)
                    AddRemoteServer(connection, dor, doi, remoteSyncID);
            });
        }

        void AddRemoteServer(Connection connection, IDomainOfResponsibility dor, IDomainOfInterest doi, Guid syncID)
        {
            if (syncID == ServerSync.LocalServer.SyncID)
                return;

            var newServer = new RemoteServerImpl(connection, doi, dor, syncID);

            lock (remoteServers)
                remoteServers.Add(connection, newServer);

            if (AddedServer != null)
                AddedServer(this, new ServerEventArgs(newServer));

            connection.Closed += delegate(object sender, EventArgs args)
            {
                lock (remoteServers)
                    remoteServers.Remove(connection);

                if (RemovedServer != null)
                    RemovedServer(this, new ServerEventArgs(newServer));
            };
        }

        Dictionary<Connection, IRemoteServer> remoteServers;
        LocalServerImpl localServer;
        WorldSync worldSync;
        DomainSync domainSync;
        ComponentSync componentSync;
    }
}
