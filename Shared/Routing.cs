namespace BluForTracker.Shared;

public struct Routing {
    public struct MarkerHub {
        public const string Path = "hubs/marker";
        public struct Server {
            public const string StartSync = "StartSync";
            public const string BroadcastSpike = "BroadcastSpike";
            public const string BroadcastMarker = "BroadcastMarker";
            public const string RemoveMarker = "RemoveMarker";
            public const string BroadcastInfoMarker = "BroadcastInfoMarker";
            public const string RemoveInfoMarker = "RemoveInfoMarker";
            public const string UpdateConnectionId = "UpdateConnectionId";
        }
        public struct Client {
            public const string ReceiveSync = "ReceiveSync";
            public const string ReceiveSpikeBroadcast = "ReceiveSpikeBroadcast";
            public const string ReceiveMarker = "ReceiveMarker";
            public const string ReceiveMarkers = "ReceiveMarkers";
            public const string RemoveMarker = "RemoveMarker";
            public const string ReceiveInfoMarker = "ReceiveInfoMarker";
            public const string ReceiveInfoMarkers = "ReceiveInfoMarkers";
            public const string RemoveInfoMarker = "RemoveInfoMarker";
        }
    }
}
