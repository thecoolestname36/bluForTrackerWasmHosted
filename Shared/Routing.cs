namespace BluForTracker.Shared;

public struct Routing {
    public struct MarkerHub {
        public const string Path = "hubs/marker";
        public struct Server {
            public const string BroadcastMarker = "BroadcastMarker";
            public const string RemoveMarker = "RemoveMarker";
            public const string BroadcastInfoMarker = "BroadcastInfoMarker";
            public const string RemoveInfoMarker = "RemoveInfoMarker";
            public const string UpdateConnectionId = "UpdateConnectionId";
        }
        public struct Client {
            public const string ReceiveConnectionId = "ReceiveConnectionId";
            public const string SyncMarkers = "SyncMarkers";
            public const string ReceiveMarker = "ReceiveMarker";
            public const string RemoveMarker = "RemoveMarker";
            public const string ReceiveInfoMarker = "ReceiveInfoMarker";
            public const string SyncInfoMarkers = "SyncInfoMarkers";
            public const string RemoveInfoMarker = "RemoveInfoMarker";
        }
    }
}
