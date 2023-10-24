namespace BluForTracker.Shared;

public struct Routing {
    public struct MarkerHub {
        public const string Path = "hubs/marker";
        public struct Server {
            public const string BroadcastMarker = "BroadcastMarker";
            public const string RemoveMarker = "RemoveMarker";
        }
        public struct Client {
            public const string ReceiveMarkers = "ReceiveMarkers";
            public const string ReceiveMarker = "ReceiveMarker";
            public const string RemoveMarker = "RemoveMarker";
            public const string UserDisconnected = "UserDisconnected";
        }
    }
}
